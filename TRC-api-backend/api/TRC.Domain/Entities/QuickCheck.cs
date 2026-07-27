using TRC.Domain.Enums;

namespace TRC.Domain.Entities;

// Lightweight prospect gauge flow (FR-3.6, §8.2) — no full import required.
public class QuickCheck : BaseEntity
{
    public Guid? UserId { get; set; }
    public User? User { get; set; }
    public decimal PaymentValue { get; set; }
    public decimal BoeValue { get; set; }
    public decimal VariancePct { get; set; }
    public RiskLevel RiskBand { get; set; }
}
