using Microsoft.Extensions.Options;
using TRC.Application.DTOs;
using TRC.Application.Interfaces;
using TRC.Application.Options;
using TRC.Domain.Enums;

namespace TRC.Application.Services;

// §8.2: VariancePct = |Payment - BOE| / BOE * 100, rendered on a color-coded gauge.
public class VarianceService : IVarianceService
{
    private readonly VarianceOptions _opts;
    public VarianceService(IOptions<VarianceOptions> opts) => _opts = opts.Value;

    public QuickCheckResult Evaluate(decimal paymentValue, decimal boeValue)
    {
        if (boeValue <= 0)
            return new QuickCheckResult(paymentValue, boeValue, 0, "amber", RiskLevel.Medium,
                "BOE value must be greater than zero to compute variance.");

        var variance = Math.Round(Math.Abs(paymentValue - boeValue) / boeValue * 100m, 2, MidpointRounding.AwayFromZero);

        var (color, band, msg) = variance < _opts.GreenBelow
            ? ("green", RiskLevel.Low, "Declared values are closely aligned. Low audit risk.")
            : variance <= _opts.AmberBelow
                ? ("amber", RiskLevel.Medium, "Moderate deviation detected. Consider a professional review.")
                : ("red", RiskLevel.High, "Significant deviation from the BOE value — elevated audit risk.");

        return new QuickCheckResult(paymentValue, boeValue, variance, color, band, msg);
    }
}
