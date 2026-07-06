using TRC.Domain.Enums;

namespace TRC.Application.DTOs;

public record QuickCheckRequest(decimal PaymentValue, decimal BoeValue, string? Phone);

public record QuickCheckResult(
    decimal PaymentValue,
    decimal BoeValue,
    decimal VariancePct,
    string GaugeColor,       // green / amber / red
    RiskLevel RiskBand,
    string Message);
