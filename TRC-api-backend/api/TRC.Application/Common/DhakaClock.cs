namespace TRC.Application.Common;

// All booking rules ("4 PM window", "12:00 cutoff") are Asia/Dhaka wall-clock, but the
// server (Render) runs UTC. Everything is stored UTC and only projected to Dhaka here.
// Bangladesh has no DST, so a fixed +06:00 offset is a safe fallback if the tz db is absent.
public interface IDhakaClock
{
    DateTime UtcNow { get; }
    DateTime Now { get; }        // Dhaka wall-clock
    DateOnly Today { get; }      // Dhaka date
    TimeOnly TimeOfDay { get; }  // Dhaka time
    DateTime ToUtc(DateOnly date, TimeOnly time);
}

public class DhakaClock : IDhakaClock
{
    private static readonly TimeSpan FixedOffset = TimeSpan.FromHours(6);
    private static readonly TimeZoneInfo? Tz = Resolve();

    private static TimeZoneInfo? Resolve()
    {
        foreach (var id in new[] { "Asia/Dhaka", "Bangladesh Standard Time" })
        {
            try { return TimeZoneInfo.FindSystemTimeZoneById(id); }
            catch (TimeZoneNotFoundException) { }
            catch (InvalidTimeZoneException) { }
        }
        return null; // fall back to the fixed +06:00 offset
    }

    public DateTime UtcNow => DateTime.UtcNow;

    public DateTime Now => Tz is null
        ? DateTime.UtcNow.Add(FixedOffset)
        : TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, Tz);

    public DateOnly Today => DateOnly.FromDateTime(Now);
    public TimeOnly TimeOfDay => TimeOnly.FromDateTime(Now);

    public DateTime ToUtc(DateOnly date, TimeOnly time)
    {
        var local = date.ToDateTime(time);
        return Tz is null
            ? DateTime.SpecifyKind(local - FixedOffset, DateTimeKind.Utc)
            : TimeZoneInfo.ConvertTimeToUtc(DateTime.SpecifyKind(local, DateTimeKind.Unspecified), Tz);
    }
}
