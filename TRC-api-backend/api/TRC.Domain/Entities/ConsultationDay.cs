namespace TRC.Domain.Entities;

// Counselor-published day; 16:00–18:00 window divided at cutoff (FR-11.1/11.2).
public class ConsultationDay : BaseEntity
{
    public DateOnly Date { get; set; }
    public TimeOnly WindowStart { get; set; } = new(16, 0);
    public TimeOnly WindowEnd { get; set; } = new(18, 0);
    public TimeOnly BookingCutoff { get; set; } = new(12, 0);
    public int MaxBookings { get; set; } = 8;
    public bool IsPublished { get; set; }
    public DateTime? TimesAssignedAt { get; set; }

    public ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();
}
