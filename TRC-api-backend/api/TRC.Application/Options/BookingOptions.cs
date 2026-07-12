namespace TRC.Application.Options;

// Client-confirmed booking rules (WhatsApp sign-off, 12 Jul 2026):
//   window 16:00-18:00, 20-min session + 10-min buffer, max 4/day,
//   same-day cutoff 12:00, 7-day advance window, Friday closed, block on 3rd miss.
// Everything here is config-driven so TRC can change the shape without a redeploy.
public class BookingOptions
{
    public const string SectionName = "Booking";

    public string WindowStart { get; set; } = "16:00";
    public string WindowEnd { get; set; } = "18:00";
    public int SessionMinutes { get; set; } = 20;
    public int BufferMinutes { get; set; } = 10;
    public int MaxBookingsPerDay { get; set; } = 4;
    public string CutoffTime { get; set; } = "12:00";
    public int AdvanceDays { get; set; } = 7;
    public string[] ClosedWeekdays { get; set; } = { "Friday" };
    public int MissedBlockThreshold { get; set; } = 3;

    // Fallback link used until Google Calendar/Meet automation lands (see IMeetingLinkProvider).
    public string? DefaultMeetingLink { get; set; }

    public TimeOnly WindowStartTime => TimeOnly.Parse(WindowStart);
    public TimeOnly WindowEndTime => TimeOnly.Parse(WindowEnd);
    public TimeOnly Cutoff => TimeOnly.Parse(CutoffTime);
    public int SlotStrideMinutes => SessionMinutes + BufferMinutes;

    public bool IsClosedOn(DayOfWeek day) =>
        ClosedWeekdays.Any(d => string.Equals(d, day.ToString(), StringComparison.OrdinalIgnoreCase));
}
