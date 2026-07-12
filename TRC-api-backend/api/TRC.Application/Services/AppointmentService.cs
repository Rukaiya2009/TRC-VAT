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
    private readonly IPhoneProfileRepository _phones;
    private readonly IMeetingLinkProvider _links;
    private readonly INotificationService _notify;
    private readonly IUnitOfWork _uow;
    private readonly IDhakaClock _clock;
    private readonly BookingOptions _opts;

    public AppointmentService(
        IConsultationDayRepository days,
        IAppointmentRepository appts,
        IPhoneProfileRepository phones,
        IMeetingLinkProvider links,
        INotificationService notify,
        IUnitOfWork uow,
        IDhakaClock clock,
        IOptions<BookingOptions> opts)
    {
        _days = days;
        _appts = appts;
        _phones = phones;
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

    public async Task<AppointmentDto> BookAsync(Guid phoneProfileId, BookAppointmentRequest request, CancellationToken ct = default)
    {
        var profile = await _phones.GetByIdAsync(phoneProfileId, ct)
            ?? throw new InvalidOperationException("Phone profile not found.");

        if (profile.IsBlocked)
            throw new InvalidOperationException(
                "This number has been blocked after repeated missed appointments. Please contact TRC directly.");

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

        if (live.Any(a => a.PhoneProfileId == phoneProfileId))
            throw new InvalidOperationException("You already have a booking on this day.");

        var (start, end) = SlotTimes(day, request.SlotIndex);

        var appointment = new Appointment
        {
            ConsultationDayId = day.Id,
            PhoneProfileId = phoneProfileId,
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

        await _notify.SendAsync(profile.PhoneNumber, Channel.WhatsApp, "BookingConfirmed", profile.PreferredLanguage, ct);

        return Project(appointment, day, profile.PhoneNumber);
    }

    public async Task<IReadOnlyList<AppointmentDto>> GetMineAsync(Guid phoneProfileId, CancellationToken ct = default)
    {
        var profile = await _phones.GetByIdAsync(phoneProfileId, ct)
            ?? throw new InvalidOperationException("Phone profile not found.");

        var appts = await _appts.GetForPhoneAsync(phoneProfileId, ct);
        return appts.Select(a => Project(a, a.ConsultationDay, profile.PhoneNumber))
                    .OrderByDescending(a => a.Date).ThenBy(a => a.SlotIndex)
                    .ToList();
    }

    public async Task CancelAsync(Guid phoneProfileId, Guid appointmentId, CancellationToken ct = default)
    {
        var appt = await _appts.GetWithDayAsync(appointmentId, ct)
            ?? throw new InvalidOperationException("Booking not found.");

        if (appt.PhoneProfileId != phoneProfileId)
            throw new UnauthorizedAccessException("That booking belongs to another number.");

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

        var profile = await _phones.GetByIdAsync(phoneProfileId, ct);
        if (profile is not null)
            await _notify.SendAsync(profile.PhoneNumber, Channel.WhatsApp, "BookingCancelled", profile.PreferredLanguage, ct);
    }

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
            var profile = await _phones.GetByIdAsync(a.PhoneProfileId, ct);
            result.Add(Project(a, day, profile?.PhoneNumber ?? "unknown"));
        }
        return result;
    }

    public async Task<AppointmentDto> SetStatusAsync(Guid appointmentId, AppointmentStatus status, CancellationToken ct = default)
    {
        var appt = await _appts.GetWithDayAsync(appointmentId, ct)
            ?? throw new InvalidOperationException("Booking not found.");

        var profile = await _phones.GetByIdAsync(appt.PhoneProfileId, ct)
            ?? throw new InvalidOperationException("Phone profile not found.");

        var wasMissed = appt.Status == AppointmentStatus.Missed;
        appt.Status = status;

        // Only a *transition into* Missed counts, so re-saving the same status can't double-count.
        if (status == AppointmentStatus.Missed && !wasMissed)
        {
            profile.MissedCount++;
            if (profile.MissedCount >= _opts.MissedBlockThreshold && !profile.IsBlocked)
            {
                profile.IsBlocked = true;
                profile.BlockedAt = _clock.UtcNow;
                await _notify.SendAsync(profile.PhoneNumber, Channel.WhatsApp, "PhoneBlocked", profile.PreferredLanguage, ct);
            }
            _phones.Update(profile);
        }

        _appts.Update(appt);
        await _uow.SaveChangesAsync(ct);

        return Project(appt, appt.ConsultationDay, profile.PhoneNumber);
    }

    // Fallback path: admin pastes a Meet/Zoom link (or a replacement if the call platform changes).
    public async Task<AppointmentDto> SetMeetingLinkAsync(Guid appointmentId, string meetingLink, CancellationToken ct = default)
    {
        var appt = await _appts.GetWithDayAsync(appointmentId, ct)
            ?? throw new InvalidOperationException("Booking not found.");

        var profile = await _phones.GetByIdAsync(appt.PhoneProfileId, ct)
            ?? throw new InvalidOperationException("Phone profile not found.");

        appt.MeetingLink = meetingLink.Trim();
        _appts.Update(appt);
        await _uow.SaveChangesAsync(ct);

        await _notify.SendAsync(profile.PhoneNumber, Channel.WhatsApp, "MeetingLinkUpdated", profile.PreferredLanguage, ct);

        return Project(appt, appt.ConsultationDay, profile.PhoneNumber);
    }

    public async Task UnblockPhoneAsync(string phone, CancellationToken ct = default)
    {
        var normalized = PhoneNumber.Normalize(phone);
        var profile = await _phones.GetByPhoneAsync(normalized, ct)
            ?? throw new InvalidOperationException("No profile exists for that number.");

        profile.IsBlocked = false;
        profile.BlockedAt = null;
        profile.MissedCount = 0;
        _phones.Update(profile);
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
