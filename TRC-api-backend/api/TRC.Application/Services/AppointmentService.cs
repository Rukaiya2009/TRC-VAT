using Microsoft.Extensions.Options;
using TRC.Application.Common;
using TRC.Application.DTOs;
using TRC.Application.Interfaces;
using TRC.Application.Options;
using TRC.Domain.Entities;
using TRC.Domain.Enums;
using TRC.Domain.Repositories;

namespace TRC.Application.Services;

// M11 — Appointment booking (FR-11.x), per client sign-off 12 Jul 2026:
//   • Fixed slots: 20-min session + 10-min buffer -> 4 slots in the 16:00–18:00 window.
//   • Under-booked days simply stay short; nobody's session is extended.
//   • Same-day bookings close at the 12:00 (Asia/Dhaka) cutoff; future days stay open.
//   • Friday closed; 7-day rolling advance window.
//   • 3rd MISSED appointment blocks the phone. Cancellations do NOT count.
public class AppointmentService : IAppointmentService
{
    private readonly IConsultationDayRepository _days;
    private readonly IAppointmentRepository _appts;
    private readonly IUserRepository _users;
    private readonly IMeetingLinkProvider _links;
    private readonly INotificationService _notify;
    private readonly IUnitOfWork _uow;
    private readonly IDhakaClock _clock;
    private readonly BookingOptions _opts;

    public AppointmentService(
        IConsultationDayRepository days,
        IAppointmentRepository appts,
        IUserRepository users,
        IMeetingLinkProvider links,
        INotificationService notify,
        IUnitOfWork uow,
        IDhakaClock clock,
        IOptions<BookingOptions> opts)
    {
        _days = days;
        _appts = appts;
        _users = users;
        _links = links;
        _notify = notify;
        _uow = uow;
        _clock = clock;
        _opts = opts.Value;
    }

    // ----------------------------------------------------------------- availability (public)

    public async Task<IReadOnlyList<ConsultationDayDto>> GetAvailabilityAsync(CancellationToken ct = default)
    {
        var today = _clock.Today;
        var last = today.AddDays(_opts.AdvanceDays);

        await EnsureDaysExistAsync(today, last, ct);

        var days = await _days.GetRangeAsync(today, last, ct);
        var result = new List<ConsultationDayDto>();

        foreach (var day in days.OrderBy(d => d.Date))
        {
            var appts = await _appts.GetForDayAsync(day.Id, ct);
            result.Add(Project(day, appts));
        }
        return result;
    }

    // Rolling generation keeps GET /consultation-days from ever being empty, with no cron job.
    private async Task EnsureDaysExistAsync(DateOnly from, DateOnly to, CancellationToken ct)
    {
        var created = false;
        for (var d = from; d <= to; d = d.AddDays(1))
        {
            if (_opts.IsClosedOn(d.DayOfWeek)) continue;          // Friday
            if (await _days.GetByDateAsync(d, ct) is not null) continue;

            await _days.AddAsync(new ConsultationDay
            {
                Date = d,
                WindowStart = _opts.WindowStartTime,
                WindowEnd = _opts.WindowEndTime,
                BookingCutoff = _opts.Cutoff,
                SessionMinutes = _opts.SessionMinutes,
                BufferMinutes = _opts.BufferMinutes,
                MaxBookings = _opts.MaxBookingsPerDay,
                IsPublished = true,
            }, ct);
            created = true;
        }
        if (created) await _uow.SaveChangesAsync(ct);
    }

    // ----------------------------------------------------------------- booking (phone token)

    public async Task<AppointmentDto> BookAsync(Guid userId, BookAppointmentRequest request, CancellationToken ct = default)
    {
        var user = await _users.GetByIdAsync(userId, ct)
            ?? throw new InvalidOperationException("User account not found.");

        if (!user.EmailConfirmed)
            throw new InvalidOperationException("Please confirm your email address before booking.");

        if (user.IsBlocked)
            throw new InvalidOperationException(
                "This account has been blocked after repeated missed appointments. Please contact TRC directly.");

        var day = await _days.GetByIdAsync(request.ConsultationDayId, ct)
            ?? throw new InvalidOperationException("That consultation day does not exist.");

        var (bookable, reason) = Bookability(day);
        if (!bookable) throw new InvalidOperationException(reason!);

        if (request.SlotIndex < 0 || request.SlotIndex >= day.MaxBookings)
            throw new InvalidOperationException("That slot does not exist on this day.");

        var existing = await _appts.GetForDayAsync(day.Id, ct);
        var live = existing.Where(a => a.Status != AppointmentStatus.Cancelled).ToList();

        if (live.Any(a => a.SlotIndex == request.SlotIndex))
            throw new InvalidOperationException("That slot has just been taken. Please choose another.");

        if (live.Any(a => a.UserId == userId))
            throw new InvalidOperationException("You already have a booking on this day.");

        var (start, end) = SlotTimes(day, request.SlotIndex);

        var appointment = new Appointment
        {
            ConsultationDayId = day.Id,
            UserId = userId,
            SlotIndex = request.SlotIndex,
            BookingOrder = live.Count + 1,
            AssignedStart = start,
            AssignedEnd = end,
            Status = AppointmentStatus.Booked,
        };

        // Manual provider returns the configured fallback link (or null for an admin to fill in).
        appointment.MeetingLink = await _links.CreateAsync(
            _clock.ToUtc(day.Date, start),
            _clock.ToUtc(day.Date, end),
            $"TRC VAT Consultation — {day.Date:yyyy-MM-dd} {start:HH\\:mm}",
            ct);

        await _appts.AddAsync(appointment, ct);
        await _uow.SaveChangesAsync(ct);

        await _notify.SendAsync((user.PhoneNumber ?? "unknown"), Channel.WhatsApp, "BookingConfirmed", user.PreferredLanguage, ct);

        return Project(appointment, day, (user.PhoneNumber ?? "unknown"));
    }

    public async Task<IReadOnlyList<AppointmentDto>> GetMineAsync(Guid userId, CancellationToken ct = default)
    {
        var user = await _users.GetByIdAsync(userId, ct)
            ?? throw new InvalidOperationException("User account not found.");

        var appts = await _appts.GetForUserAsync(userId, ct);
        return appts.Select(a => Project(a, a.ConsultationDay, (user.PhoneNumber ?? "unknown")))
                    .OrderByDescending(a => a.Date).ThenBy(a => a.SlotIndex)
                    .ToList();
    }

    public async Task CancelAsync(Guid userId, Guid appointmentId, CancellationToken ct = default)
    {
        var appt = await _appts.GetWithDayAsync(appointmentId, ct)
            ?? throw new InvalidOperationException("Booking not found.");

        if (appt.UserId != userId)
            throw new UnauthorizedAccessException("That booking belongs to another account.");

        if (appt.Status == AppointmentStatus.Cancelled)
            throw new InvalidOperationException("That booking is already cancelled.");

        if (appt.Status is AppointmentStatus.Completed or AppointmentStatus.Missed)
            throw new InvalidOperationException("That booking can no longer be cancelled.");

        // Cancellation is only allowed while the day is still open for changes.
        var (open, _) = Bookability(appt.ConsultationDay);
        if (!open)
            throw new InvalidOperationException(
                "The cutoff for this day has passed, so it can no longer be cancelled. Please contact TRC.");

        appt.Status = AppointmentStatus.Cancelled;   // does NOT increment MissedCount
        appt.CancelledAt = _clock.UtcNow;
        _appts.Update(appt);
        await _uow.SaveChangesAsync(ct);

        var user = await _users.GetByIdAsync(userId, ct);
        if (user is not null)
            await _notify.SendAsync((user.PhoneNumber ?? "unknown"), Channel.WhatsApp, "BookingCancelled", user.PreferredLanguage, ct);
    }

    // ----------------------------------------------------------------- reschedule

    // Moving a booking is deliberately NOT cancel-then-rebook: the row is reused, so
    // BookingOrder and the user's MissedCount are untouched (a reschedule is not a no-show).
    // Lead time is measured from the CURRENT slot's start, in Asia/Dhaka:
    //   • prospect: 6h  — after that they can only cancel
    //   • staff:    1h  — and staff may also move a booking past the 12:00 same-day cutoff
    public Task<AppointmentDto> RescheduleAsync(Guid userId, Guid appointmentId, RescheduleAppointmentRequest request, CancellationToken ct = default)
        => MoveAsync(appointmentId, request, userId, _opts.ProspectRescheduleLeadHours, ct);

    public Task<AppointmentDto> RescheduleAsAdminAsync(Guid appointmentId, RescheduleAppointmentRequest request, CancellationToken ct = default)
        => MoveAsync(appointmentId, request, null, _opts.AdminRescheduleLeadHours, ct);

    private async Task<AppointmentDto> MoveAsync(
        Guid appointmentId,
        RescheduleAppointmentRequest request,
        Guid? actingUserId,                 // null => staff acting on the client's behalf
        double leadHours,
        CancellationToken ct)
    {
        var isSelf = actingUserId is not null;

        var appt = await _appts.GetWithDayAsync(appointmentId, ct)
            ?? throw new InvalidOperationException("Booking not found.");

        // Ownership comes from the token, never the body (same rule as CancelAsync).
        if (actingUserId is Guid uid && appt.UserId != uid)
            throw new UnauthorizedAccessException("That booking belongs to another account.");

        if (appt.Status == AppointmentStatus.Cancelled)
            throw new InvalidOperationException("That booking is cancelled. Please make a new booking instead.");

        if (appt.Status is AppointmentStatus.Completed or AppointmentStatus.Missed)
            throw new InvalidOperationException("That booking has already been closed and can no longer be moved.");

        var user = await _users.GetByIdAsync(appt.UserId, ct)
            ?? throw new InvalidOperationException("User account not found.");

        if (isSelf && user.IsBlocked)
            throw new InvalidOperationException(
                "This account has been blocked after repeated missed appointments. Please contact TRC directly.");

        var currentDay = appt.ConsultationDay;

        // --- lead-time gate on the slot they currently hold -------------------------------
        var currentStartUtc = _clock.ToUtc(currentDay.Date, appt.AssignedStart ?? currentDay.WindowStart);
        if (_clock.UtcNow >= currentStartUtc.AddHours(-leadHours))
            throw new InvalidOperationException(isSelf
                ? $"Rescheduling closes {Hours(leadHours)} before your slot. You can still cancel this booking, or contact TRC for help."
                : $"This slot starts in under {Hours(leadHours)}, so it can no longer be moved.");

        // --- target day ------------------------------------------------------------------
        var targetDay = appt.ConsultationDayId == request.ConsultationDayId
            ? currentDay
            : await _days.GetByIdAsync(request.ConsultationDayId, ct)
              ?? throw new InvalidOperationException("That consultation day does not exist.");

        if (!targetDay.IsPublished)
            throw new InvalidOperationException("That day is closed for bookings.");

        if (targetDay.Date < _clock.Today)
            throw new InvalidOperationException("That date has passed.");

        // Prospects are held to the normal booking rules (incl. the 12:00 same-day cutoff);
        // staff are intentionally allowed past the cutoff so they can fix things day-of.
        if (isSelf)
        {
            var (bookable, reason) = Bookability(targetDay);
            if (!bookable) throw new InvalidOperationException(reason!);
        }

        if (request.SlotIndex < 0 || request.SlotIndex >= targetDay.MaxBookings)
            throw new InvalidOperationException("That slot does not exist on this day.");

        if (targetDay.Id == appt.ConsultationDayId && request.SlotIndex == appt.SlotIndex)
            throw new InvalidOperationException("This booking is already in that slot.");

        var (start, end) = SlotTimes(targetDay, request.SlotIndex);
        var newStartUtc = _clock.ToUtc(targetDay.Date, start);

        // The slot they move INTO must also respect the same lead time.
        if (newStartUtc < _clock.UtcNow.AddHours(leadHours))
            throw new InvalidOperationException(
                $"That slot starts too soon — please pick one at least {Hours(leadHours)} from now.");

        var onTarget = await _appts.GetForDayAsync(targetDay.Id, ct);
        var live = onTarget.Where(a => a.Status != AppointmentStatus.Cancelled && a.Id != appt.Id).ToList();

        if (live.Any(a => a.SlotIndex == request.SlotIndex))
            throw new InvalidOperationException("That slot has just been taken. Please choose another.");

        if (live.Any(a => a.UserId == appt.UserId))
            throw new InvalidOperationException("This account already has another booking on that day.");

        // --- move it ---------------------------------------------------------------------
        appt.ConsultationDayId = targetDay.Id;
        appt.SlotIndex = request.SlotIndex;
        appt.AssignedStart = start;
        appt.AssignedEnd = end;
        // BookingOrder, Status and MissedCount deliberately unchanged.

        // Manual provider hands back the configured fallback link; keep the existing one if
        // it returns null so a link an admin pasted in by hand isn't silently wiped.
        var link = await _links.CreateAsync(
            newStartUtc,
            _clock.ToUtc(targetDay.Date, end),
            $"TRC VAT Consultation — {targetDay.Date:yyyy-MM-dd} {start:HH\:mm}",
            ct);
        if (!string.IsNullOrWhiteSpace(link)) appt.MeetingLink = link;

        _appts.Update(appt);
        await _uow.SaveChangesAsync(ct);

        await _notify.SendAsync((user.PhoneNumber ?? "unknown"), Channel.WhatsApp, "BookingRescheduled", user.PreferredLanguage, ct);

        return Project(appt, targetDay, (user.PhoneNumber ?? "unknown"));
    }

    private static string Hours(double h) =>
        Math.Abs(h - 1) < 0.001 ? "1 hour" : $"{h:0.##} hours";

    // ----------------------------------------------------------------- admin

    public async Task<ConsultationDayDto> PublishDayAsync(CreateConsultationDayRequest request, CancellationToken ct = default)
    {
        var day = await _days.GetByDateAsync(request.Date, ct);
        if (day is null)
        {
            day = new ConsultationDay { Date = request.Date };
            await _days.AddAsync(day, ct);
        }

        day.WindowStart = _opts.WindowStartTime;
        day.WindowEnd = _opts.WindowEndTime;
        day.BookingCutoff = _opts.Cutoff;
        day.SessionMinutes = request.SessionMinutes ?? _opts.SessionMinutes;
        day.BufferMinutes = request.BufferMinutes ?? _opts.BufferMinutes;
        day.MaxBookings = request.MaxBookings ?? _opts.MaxBookingsPerDay;
        day.IsPublished = true;

        _days.Update(day);
        await _uow.SaveChangesAsync(ct);

        var appts = await _appts.GetForDayAsync(day.Id, ct);
        return Project(day, appts);
    }

    public async Task<ConsultationDayDto> CloseDayAsync(Guid consultationDayId, CancellationToken ct = default)
    {
        var day = await _days.GetByIdAsync(consultationDayId, ct)
            ?? throw new InvalidOperationException("That consultation day does not exist.");

        day.IsPublished = false;   // holidays, Eid, leave
        _days.Update(day);
        await _uow.SaveChangesAsync(ct);

        var appts = await _appts.GetForDayAsync(day.Id, ct);
        return Project(day, appts);
    }

    public async Task<IReadOnlyList<AppointmentDto>> GetForDayAsync(Guid consultationDayId, CancellationToken ct = default)
    {
        var day = await _days.GetByIdAsync(consultationDayId, ct)
            ?? throw new InvalidOperationException("That consultation day does not exist.");

        var appts = await _appts.GetForDayAsync(consultationDayId, ct);
        var result = new List<AppointmentDto>();
        foreach (var a in appts.OrderBy(a => a.SlotIndex))
        {
            var user = await _users.GetByIdAsync(a.UserId, ct);
            result.Add(Project(a, day, user?.PhoneNumber ?? "unknown"));
        }
        return result;
    }

    public async Task<AppointmentDto> SetStatusAsync(Guid appointmentId, AppointmentStatus status, CancellationToken ct = default)
    {
        var appt = await _appts.GetWithDayAsync(appointmentId, ct)
            ?? throw new InvalidOperationException("Booking not found.");

        var user = await _users.GetByIdAsync(appt.UserId, ct)
            ?? throw new InvalidOperationException("User account not found.");

        var wasMissed = appt.Status == AppointmentStatus.Missed;
        appt.Status = status;

        // Only a *transition into* Missed counts, so re-saving the same status can't double-count.
        if (status == AppointmentStatus.Missed && !wasMissed)
        {
            user.MissedCount++;
            if (user.MissedCount >= _opts.MissedBlockThreshold && !user.IsBlocked)
            {
                user.IsBlocked = true;
                user.BlockedAt = _clock.UtcNow;
                await _notify.SendAsync((user.PhoneNumber ?? "unknown"), Channel.WhatsApp, "PhoneBlocked", user.PreferredLanguage, ct);
            }
            _users.Update(user);
        }

        _appts.Update(appt);
        await _uow.SaveChangesAsync(ct);

        return Project(appt, appt.ConsultationDay, (user.PhoneNumber ?? "unknown"));
    }

    // Fallback path: admin pastes a Meet/Zoom link (or a replacement if the call platform changes).
    public async Task<AppointmentDto> SetMeetingLinkAsync(Guid appointmentId, string meetingLink, CancellationToken ct = default)
    {
        var appt = await _appts.GetWithDayAsync(appointmentId, ct)
            ?? throw new InvalidOperationException("Booking not found.");

        var user = await _users.GetByIdAsync(appt.UserId, ct)
            ?? throw new InvalidOperationException("User account not found.");

        appt.MeetingLink = meetingLink.Trim();
        _appts.Update(appt);
        await _uow.SaveChangesAsync(ct);

        await _notify.SendAsync((user.PhoneNumber ?? "unknown"), Channel.WhatsApp, "MeetingLinkUpdated", user.PreferredLanguage, ct);

        return Project(appt, appt.ConsultationDay, (user.PhoneNumber ?? "unknown"));
    }

    public async Task UnblockPhoneAsync(string phone, CancellationToken ct = default)
    {
        var normalized = PhoneNumber.Normalize(phone);
        var user = await _users.GetByPhoneAsync(normalized, ct)
            ?? throw new InvalidOperationException("No account exists for that number.");

        user.IsBlocked = false;
        user.BlockedAt = null;
        user.MissedCount = 0;
        _users.Update(user);
        await _uow.SaveChangesAsync(ct);
    }

    // ----------------------------------------------------------------- slot maths & projection

    // Slot i runs [WindowStart + i*(session+buffer), +session). The buffer is dead air by design —
    // it's the consultant's breathing room between back-to-back online calls.
    private static (TimeOnly start, TimeOnly end) SlotTimes(ConsultationDay day, int index)
    {
        var stride = day.SessionMinutes + day.BufferMinutes;
        var start = day.WindowStart.AddMinutes(index * stride);
        return (start, start.AddMinutes(day.SessionMinutes));
    }

    private (bool bookable, string? reason) Bookability(ConsultationDay day)
    {
        if (!day.IsPublished) return (false, "That day is closed for bookings.");
        if (day.Date < _clock.Today) return (false, "That date has passed.");

        // The cutoff only ever closes *today*. Future days stay open.
        if (day.Date == _clock.Today && _clock.TimeOfDay >= day.BookingCutoff)
            return (false, $"Same-day bookings closed at {day.BookingCutoff:HH\\:mm}. Please pick a later date.");

        return (true, null);
    }

    private ConsultationDayDto Project(ConsultationDay day, IReadOnlyList<Appointment> appts)
    {
        var live = appts.Where(a => a.Status != AppointmentStatus.Cancelled).ToList();
        var taken = live.Select(a => a.SlotIndex).ToHashSet();
        var (bookable, reason) = Bookability(day);

        var slots = Enumerable.Range(0, day.MaxBookings).Select(i =>
        {
            var (s, e) = SlotTimes(day, i);
            return new SlotDto(i, s.ToString("HH:mm"), e.ToString("HH:mm"),
                bookable && !taken.Contains(i));
        }).ToList();

        var available = slots.Count(s => s.Available);
        if (bookable && available == 0)
            (bookable, reason) = (false, "All slots on this day are booked.");

        return new ConsultationDayDto(
            day.Id, day.Date, day.Date.DayOfWeek.ToString(),
            bookable, reason,
            day.MaxBookings, live.Count, available, slots);
    }

    private static AppointmentDto Project(Appointment a, ConsultationDay day, string phone) =>
        new(a.Id, day.Date, a.SlotIndex,
            (a.AssignedStart ?? day.WindowStart).ToString("HH:mm"),
            (a.AssignedEnd ?? day.WindowStart).ToString("HH:mm"),
            a.Status, a.MeetingLink, phone, a.CreatedAt);
}
