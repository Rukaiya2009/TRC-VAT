using TRC.Domain.Enums;

namespace TRC.Domain.Entities;

// Identity anchor for prospects (SRS §7). 2 missed slots => blocked (FR-11.6).
public class PhoneProfile : BaseEntity
{
    public string PhoneNumber { get; set; } = null!;
    public Language PreferredLanguage { get; set; } = Language.En;
    public int MissedCount { get; set; }
    public bool IsBlocked { get; set; }
    public DateTime? BlockedAt { get; set; }

    public ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();
    public ICollection<OtpCode> OtpCodes { get; set; } = new List<OtpCode>();
    public ICollection<QuickCheck> QuickChecks { get; set; } = new List<QuickCheck>();
}
