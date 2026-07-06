namespace TRC.Application.Options;

// Rates held in configuration (FR-4.1). Bound from "TaxRates" section.
public class TaxRateOptions
{
    public const string SectionName = "TaxRates";

    public decimal CD { get; set; } = 0.15m;
    public decimal RD { get; set; } = 0.00m;
    public decimal SD { get; set; } = 0.00m;
    public decimal VAT { get; set; } = 0.15m;
    public decimal AIT { get; set; } = 0.02m;
    public decimal AT { get; set; } = 0.02m;
    public decimal ATV { get; set; } = 0.00m;
}
