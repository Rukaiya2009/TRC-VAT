using TRC.Domain.Enums;

namespace TRC.Application.DTOs;

public record TriggeredRuleDto(string Code, string Name, int Points);

public record RiskAssessmentResult(
    Guid ImportId,
    int Score,
    RiskLevel Level,
    IReadOnlyList<TriggeredRuleDto> Triggered,
    IReadOnlyList<string> NotEvaluable);

public record RiskRuleDto(
    Guid Id, string Code, string Name, int Weight, bool Enabled, RulePhase Phase, string? Rationale);

public record UpdateRiskRuleRequest(int Weight, bool Enabled);
