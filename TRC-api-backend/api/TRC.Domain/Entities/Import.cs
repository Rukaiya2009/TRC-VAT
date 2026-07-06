using TRC.Domain.Enums;

namespace TRC.Domain.Entities;

// An import declaration. On save, tax breakdown is computed (FR-3.2, Section 8).
public class Import : BaseEntity
{
    public Guid? UserId { get; set; }
    public User? User { get; set; }

    public DateTime Date { get; set; } = DateTime.UtcNow;
    public string Consignee { get; set; } = null!;
    public string HSCode { get; set; } = null!;
    public string Description { get; set; } = null!;

    public decimal InvoiceValueUsd { get; set; }
    public decimal ExchangeRate { get; set; }
    public decimal CfrValue { get; set; }
    public decimal OtherCosts { get; set; }

    public decimal AssessableValue { get; set; }
    public decimal TotalTax { get; set; }

    public ImportStatus Status { get; set; } = ImportStatus.Draft;
    public DateTime? UpdatedAt { get; set; }

    // Document flags feeding R11 (missing/inconsistent documentation)
    public bool HasCommercialInvoice { get; set; } = true;
    public bool HasBillOfLading { get; set; } = true;
    public bool HasMushak { get; set; } = true;

    public ICollection<TaxBreakdown> TaxBreakdowns { get; set; } = new List<TaxBreakdown>();
    public ICollection<RiskAssessment> RiskAssessments { get; set; } = new List<RiskAssessment>();
}
