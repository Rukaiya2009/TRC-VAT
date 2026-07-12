using TRC.Domain.Enums;

namespace TRC.Application.DTOs;

// One fixed slot inside the window. Times are Asia/Dhaka wall-clock ("16:00").
public record SlotDto(int SlotIndex, string Start, string End, bool Available);

public record ConsultationDayDto(
    Guid Id,
    DateOnly Date,
    string DayOfWeek,
    bool Bookable,               // false once the same-day 12:00 cutoff passes, or day is closed/full
    string? UnavailableReason,
    int Capacity,
    int BookedCount,
    int AvailableCount,
    IReadOnlyList<SlotDto> Slots);

public record BookAppointmentRequest(Guid ConsultationDayId, int SlotIndex);

public record AppointmentDto(
    Guid Id,
    DateOnly Date,
    int SlotIndex,
    string Start,
    string End,
    AppointmentStatus Status,
    string? MeetingLink,
    string Phone,
    DateTime CreatedAt);

// Admin: publish/override a specific date.
public record CreateConsultationDayRequest(
    DateOnly Date,
    int? MaxBookings,
    int? SessionMinutes,
    int? BufferMinutes);

public record UpdateAppointmentStatusRequest(AppointmentStatus Status);

public record UpdateMeetingLinkRequest(string MeetingLink);
