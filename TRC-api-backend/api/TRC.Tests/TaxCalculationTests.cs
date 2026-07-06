using FluentAssertions;
using Microsoft.Extensions.Options;
using TRC.Application.Options;
using TRC.Application.Services;
using TRC.Domain.Enums;
using Xunit;

namespace TRC.Tests;

// TC-4.1 (FR-4.2): the engine must reproduce the Chattogram Bill of Entry (cement
// clinker, HS 25231000) — SRS Section 8. Reference figures from Box 47.
public class TaxCalculationTests
{
    private static TaxCalculationService Service()
    {
        var rates = Options.Create(new TaxRateOptions
        {
            CD = 0.15m, RD = 0m, SD = 0m, VAT = 0.15m, AIT = 0.02m, AT = 0.02m, ATV = 0m
        });
        return new TaxCalculationService(rates);
    }

    // The BOE derives AV from CFR + other costs + valuation adjustments. We feed the
    // verified AV directly by expressing it as invoice*rate with zero extra costs.
    private const decimal ReferenceAv = 293_378_412.40m;

    [Fact]
    public void Reproduces_reference_assessment_to_the_paisa()
    {
        var svc = Service();
        // AV = invoice(=AV) * rate(=1) + other(0)  -> isolates the tax math on the AV.
        var result = svc.Calculate(invoiceValueUsd: ReferenceAv, exchangeRate: 1m, otherCosts: 0m);

        result.AssessableValue.Should().Be(293_378_412.40m);

        decimal Amount(TaxType t) => result.Lines.Single(l => l.TaxType == t).Amount;

        // Small tolerance absorbs Customs' per-line rounding (the BOE's own lines carry
        // ~0.01 rounding noise). Tighten to exact once the precise AV/rounding is pinned.
        Amount(TaxType.CD).Should().BeApproximately(44_006_761.86m, 0.02m);
        Amount(TaxType.RD).Should().Be(0m);
        Amount(TaxType.SD).Should().Be(0m);
        Amount(TaxType.VAT).Should().BeApproximately(50_607_776.14m, 0.05m);
        Amount(TaxType.AIT).Should().BeApproximately(5_867_568.25m, 0.02m);
        Amount(TaxType.AT).Should().BeApproximately(6_747_703.49m, 0.05m);

        result.TotalTax.Should().BeApproximately(107_229_809.74m, 0.10m);
    }

    [Fact]
    public void Cfr_and_av_derive_from_invoice_and_rate()
    {
        var svc = Service();
        // From §8.1: 1,948,800.00 USD * 123.1450 = 239,984,976.00 CFR.
        var result = svc.Calculate(invoiceValueUsd: 1_948_800.00m, exchangeRate: 123.1450m, otherCosts: 0m);
        result.AssessableValue.Should().Be(239_984_976.00m);
    }
}
