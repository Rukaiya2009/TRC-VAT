namespace TRC.Domain.Entities;

// Codes stored hashed; rate limits enforced in service (FR-9.2).
public class OtpCode : BaseEntity
{
    public Guid PhoneProfileId { get; set; }
    public PhoneProfile PhoneProfile { get; set; } = null!;
    public string CodeHash { get; set; } = null!;
    public DateTime ExpiresAt { get; set; }
    public int Attempts { get; set; }
    public DateTime? VerifiedAt { get; set; }
}
