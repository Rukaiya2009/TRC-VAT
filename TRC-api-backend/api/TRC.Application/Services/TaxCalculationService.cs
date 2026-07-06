using Microsoft.Extensions.Options;
using TRC.Application.DTOs;
using TRC.Application.Interfaces;
using TRC.Application.Options;
using TRC.Domain.Enums;

namespace TRC.Application.Services;

/// <summary>
/// Reproduces the Bangladesh Customs assessment (SRS Section 8), verified against
/// the Chattogram Bill of Entry for cement clinker (HS 25231000).
/// Bases:
///   AV       = (InvoiceUsd * ExchangeRate) + OtherCosts
///   CD/RD/SD = AV * rate
///   VAT base = AV + CD + RD + SD
///   AIT      = AV * rate
///   AT base  = AV + CD + RD + SD   (the "VAT base")
/// Every line rounds to 2 dp; totals must match the reference to the paisa (TC-4.1).
/// </summary>
public class TaxCalculationService : ITaxCalculationService
{
    private readonly TaxRateOptions _rates;

    public TaxCalculationService(IOptions<TaxRateOptions> rates) => _rates = rates.Value;

    private static decimal R(decimal v) => Math.Round(v, 2, MidpointRounding.AwayFromZero);

    public TaxCalculationResult Calculate(decimal invoiceValueUsd, decimal exchangeRate, decimal otherCosts)
    {
        var cfr = R(invoiceValueUsd * exchangeRate);
        var av = R(cfr + otherCosts);

        var cd = R(av * _rates.CD);
        var rd = R(av * _rates.RD);
        var sd = R(av * _rates.SD);

        var vatBase = R(av + cd + rd + sd);
        var vat = R(vatBase * _rates.VAT);

        var ait = R(av * _rates.AIT);

        var atBase = vatBase;                 // AV + CD + RD + SD
        var at = R(atBase * _rates.AT);

        var atvBase = R(vatBase + vat);
        var atv = R(atvBase * _rates.ATV);

        var lines = new List<TaxLineDto>
        {
            new(TaxType.CD,  av,      _rates.CD,  cd),
            new(TaxType.RD,  av,      _rates.RD,  rd),
            new(TaxType.SD,  av,      _rates.SD,  sd),
            new(TaxType.VAT, vatBase, _rates.VAT, vat),
            new(TaxType.AIT, av,      _rates.AIT, ait),
            new(TaxType.AT,  atBase,  _rates.AT,  at),
            new(TaxType.ATV, atvBase, _rates.ATV, atv),
        };

        var total = R(lines.Sum(l => l.Amount));
        return new TaxCalculationResult(av, total, lines);
    }
}
