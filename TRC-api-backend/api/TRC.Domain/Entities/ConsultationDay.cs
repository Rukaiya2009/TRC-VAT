namespace TRC.Domain.Entities;

// A bookable date. Slot shape is snapshotted onto the row at creation so that changing
// the config later never rewrites the times of days people have already booked.
public class ConsultationDay : BaseEntity
{
    public DateOnly Date { get; set; }
    public TimeOnly WindowStart { get; set; } = new(16, 0);
    public TimeOnly WindowEnd { get; set; } = new(18, 0);
    public TimeOnly BookingCutoff { get; set; } = new(12, 0);   // same-day bookings close at noon

    public int SessionMinutes { get; set; } = 20;
    public int BufferMinutes { get; set; } = 10;                // breathing room between calls
    public int MaxBookings { get; set; } = 4;

    public bool IsPublished { get; set; } = true;               // admin can close a date (holiday/Eid)
    public DateTime? TimesAssignedAt { get; set; }

    public ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();
}
