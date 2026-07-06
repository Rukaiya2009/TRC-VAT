namespace TRC.Application.Options;

// Configurable gauge thresholds (§8.2). Defaults: green <5, amber 5-15, red >15.
public class VarianceOptions
{
    public const string SectionName = "Variance";
    public decimal GreenBelow { get; set; } = 5m;
    public decimal AmberBelow { get; set; } = 15m;
}
