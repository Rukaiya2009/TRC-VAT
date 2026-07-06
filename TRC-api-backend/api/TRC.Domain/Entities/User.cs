using TRC.Domain.Enums;

namespace TRC.Domain.Entities;

// TRC staff / registered accounts (SRS §7). Prospects use PhoneProfile instead.
public class User : BaseEntity
{
    public string Email { get; set; } = null!;
    public string PasswordHash { get; set; } = null!;
    public string FullName { get; set; } = null!;
    public UserRole Role { get; set; } = UserRole.Prospect;
    public Language PreferredLanguage { get; set; } = Language.En;
    public bool IsActive { get; set; } = true;
    public DateTime? LastLogin { get; set; }

    // For JWT refresh (FR-1.3)
    public string? RefreshTokenHash { get; set; }
    public DateTime? RefreshTokenExpiresAt { get; set; }

    public ICollection<Import> Imports { get; set; } = new List<Import>();
    public ICollection<AuditLog> AuditLogs { get; set; } = new List<AuditLog>();
}
