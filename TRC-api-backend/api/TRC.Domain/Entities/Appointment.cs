using TRC.Domain.Enums;

namespace TRC.Domain.Entities;

// Slots are FIXED (20 min session + 10 min buffer), so the time is known the moment the
// prospect books — no cutoff-time division job is needed. Client sign-off, 12 Jul 2026.
public class Appointment : BaseEntity
{
    public Guid ConsultationDayId { get; set; }
    public ConsultationDay ConsultationDay { get; set; } = null!;
    public Guid PhoneProfileId { get; set; }
    public PhoneProfile PhoneProfile { get; set; } = null!;

    public int SlotIndex { get; set; }              // 0-based position within the window
    public int BookingOrder { get; set; }           // order the booking arrived in
    public TimeOnly? AssignedStart { get; set; }    // Asia/Dhaka wall-clock
    public TimeOnly? AssignedEnd { get; set; }

    // Manually set by an admin today; auto-generated once Google Calendar/Meet is wired up.
    public string? MeetingLink { get; set; }

    public AppointmentStatus Status { get; set; } = AppointmentStatus.Booked;
    public DateTime? CancelledAt { get; set; }
}
