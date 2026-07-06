using TRC.Domain.Enums;

namespace TRC.Domain.Entities;

// Latest record is the current one (SRS §7). Triggered/NotEvaluable stored as jsonb.
public class RiskAssessment : BaseEntity
{
    public Guid ImportId { get; set; }
    public Import Import { get; set; } = null!;
    public int RiskScore { get; set; }
    public RiskLevel RiskLevel { get; set; }
    public string TriggeredRules { get; set; } = "[]";   // jsonb
    public string NotEvaluable { get; set; } = "[]";     // jsonb
    public DateTime AssessedAt { get; set; } = DateTime.UtcNow;
    public Guid? AssessedByUserId { get; set; }
}
