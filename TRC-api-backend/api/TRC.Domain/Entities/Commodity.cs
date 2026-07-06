namespace TRC.Domain.Entities;

// HS code catalogue. Null benchmark => R4/R8 Not Evaluable (SRS §7, A3).
public class Commodity : BaseEntity
{
    public string HSCode { get; set; } = null!;
    public string Description { get; set; } = null!;
    public string Unit { get; set; } = null!;
    public decimal? BenchmarkPrice { get; set; }
}
