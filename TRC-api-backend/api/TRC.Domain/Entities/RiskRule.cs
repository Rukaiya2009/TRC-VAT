using TRC.Domain.Enums;

namespace TRC.Domain.Entities;

// The 12 approved rules (TRC-RSK-002). Editable by Admin at runtime — FR-6.2.
public class RiskRule : BaseEntity
{
    public string Code { get; set; } = null!;   // R1..R12
    public string Name { get; set; } = null!;
    public int Weight { get; set; }
    public string? ThresholdJson { get; set; }   // rule-specific config
    public bool Enabled { get; set; } = true;
    public RulePhase PhaseTag { get; set; }
    public DateTime? LastReviewed { get; set; }
    public string? Rationale { get; set; }
}
