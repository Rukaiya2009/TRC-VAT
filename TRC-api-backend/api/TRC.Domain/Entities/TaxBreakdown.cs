using TRC.Domain.Enums;

namespace TRC.Domain.Entities;

// One row per tax line (CD, RD, SD, VAT, AIT, AT, ATV) — FR-4.3.
public class TaxBreakdown : BaseEntity
{
    public Guid ImportId { get; set; }
    public Import Import { get; set; } = null!;
    public TaxType TaxType { get; set; }
    public decimal BaseAmount { get; set; }
    public decimal Rate { get; set; }   // fraction, e.g. 0.15
    public decimal Amount { get; set; }
}
