using TRC.Domain.Enums;

namespace TRC.Domain.Entities;

// Assigned times stay null until the cutoff division runs (SRS §10).
public class Appointment : BaseEntity
{
    public Guid ConsultationDayId { get; set; }
    public ConsultationDay ConsultationDay { get; set; } = null!;
    public Guid PhoneProfileId { get; set; }
    public PhoneProfile PhoneProfile { get; set; } = null!;
    public int BookingOrder { get; set; }
    public TimeOnly? AssignedStart { get; set; }
    public TimeOnly? AssignedEnd { get; set; }
    public AppointmentStatus Status { get; set; } = AppointmentStatus.Booked;
}
