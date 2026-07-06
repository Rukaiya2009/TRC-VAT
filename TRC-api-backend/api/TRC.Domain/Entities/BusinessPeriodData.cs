namespace TRC.Domain.Entities;

// Monthly sales/purchases/VAT per business (FR-3.5). Powers rules R1 & R6.
public class BusinessPeriodData : BaseEntity
{
    public Guid? UserId { get; set; }
    public Guid? PhoneProfileId { get; set; }
    public int Year { get; set; }
    public int Month { get; set; }
    public decimal TotalSales { get; set; }
    public decimal TotalPurchases { get; set; }
    public decimal VatDeclared { get; set; }
}
