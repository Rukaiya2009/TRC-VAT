using TRC.Domain.Enums;

namespace TRC.Domain.Entities;

// Identity anchor for prospects (SRS §7). Blocked on the 3rd missed slot (client sign-off).
// Cancellations do NOT count as misses — we want people to cancel rather than no-show.
public class PhoneProfile : BaseEntity
{
    public string PhoneNumber { get; set; } = null!;            // E.164, e.g. +8801799707090
    public Language PreferredLanguage { get; set; } = Language.En;
    public int MissedCount { get; set; }
    public bool IsBlocked { get; set; }
    public DateTime? BlockedAt { get; set; }
    public DateTime? LastVerifiedAt { get; set; }

    public ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();
    public ICollection<OtpCode> OtpCodes { get; set; } = new List<OtpCode>();
    public ICollection<QuickCheck> QuickChecks { get; set; } = new List<QuickCheck>();
}
