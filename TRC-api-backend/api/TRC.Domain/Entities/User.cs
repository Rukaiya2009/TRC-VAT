using Microsoft.AspNetCore.Identity;
using TRC.Domain.Enums;

namespace TRC.Domain.Entities;

// Now backed by ASP.NET Core Identity. IdentityUser<Guid> supplies Id, Email, UserName,
// PasswordHash, PhoneNumber, EmailConfirmed, LockoutEnd, AccessFailedCount, TwoFactorEnabled.
// Prospects AND staff are Users now; the old PhoneProfile is gone (email pivot, 26 Jul 2026).
public class User : IdentityUser<Guid>
{
    public User() { Id = Guid.NewGuid(); }

    public string FullName { get; set; } = null!;
    public UserRole Role { get; set; } = UserRole.Prospect;
    public Language PreferredLanguage { get; set; } = Language.En;
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;   // previously on BaseEntity
    public DateTime? LastLogin { get; set; }

    // Prospect booking-block state (moved off PhoneProfile). 3rd miss blocks; cancels don't count.
    public int MissedCount { get; set; }
    public bool IsBlocked { get; set; }
    public DateTime? BlockedAt { get; set; }

    // Custom JWT refresh (Identity doesn't manage refresh tokens).
    public string? RefreshTokenHash { get; set; }
    public DateTime? RefreshTokenExpiresAt { get; set; }

    public ICollection<Import> Imports { get; set; } = new List<Import>();
    public ICollection<AuditLog> AuditLogs { get; set; } = new List<AuditLog>();
    public ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();
}
