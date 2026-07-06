using System.Text.Json;
using TRC.Application.DTOs;
using TRC.Application.Interfaces;
using TRC.Domain.Entities;
using TRC.Domain.Enums;
using TRC.Domain.Repositories;

namespace TRC.Application.Services;

public class ImportService : IImportService
{
    private readonly IImportRepository _imports;
    private readonly ITaxCalculationService _tax;
    private readonly IRiskEngine _risk;
    private readonly IUnitOfWork _uow;

    public ImportService(IImportRepository imports, ITaxCalculationService tax, IRiskEngine risk, IUnitOfWork uow)
    {
        _imports = imports;
        _tax = tax;
        _risk = risk;
        _uow = uow;
    }

    public async Task<ImportDto> CreateAsync(CreateImportRequest r, Guid? userId, CancellationToken ct = default)
    {
        var calc = _tax.Calculate(r.InvoiceValueUsd, r.ExchangeRate, r.OtherCosts);

        var import = new Import
        {
            UserId = userId,
            Date = r.Date ?? DateTime.UtcNow,
            Consignee = r.Consignee,
            HSCode = r.HSCode,
            Description = r.Description,
            InvoiceValueUsd = r.InvoiceValueUsd,
            ExchangeRate = r.ExchangeRate,
            CfrValue = Math.Round(r.InvoiceValueUsd * r.ExchangeRate, 2, MidpointRounding.AwayFromZero),
            OtherCosts = r.OtherCosts,
            AssessableValue = calc.AssessableValue,
            TotalTax = calc.TotalTax,
            Status = ImportStatus.Submitted,
            HasCommercialInvoice = r.HasCommercialInvoice,
            HasBillOfLading = r.HasBillOfLading,
            HasMushak = r.HasMushak,
        };

        foreach (var line in calc.Lines)
            import.TaxBreakdowns.Add(new TaxBreakdown
            {
                TaxType = line.TaxType,
                BaseAmount = line.BaseAmount,
                Rate = line.Rate,
                Amount = line.Amount,
            });

        await _imports.AddAsync(import, ct);
        await _uow.SaveChangesAsync(ct);
        return ToDto(import, calc.Lines);
    }

    public async Task<ImportDto?> GetAsync(Guid id, CancellationToken ct = default)
    {
        var import = await _imports.GetWithDetailsAsync(id, ct);
        return import is null ? null : ToDto(import, LinesOf(import));
    }

    public async Task<IReadOnlyList<ImportDto>> ListAsync(CancellationToken ct = default)
    {
        var all = await _imports.ListAsync(ct);
        return all.Select(i => ToDto(i, LinesOf(i))).ToList();
    }

    public async Task<RiskAssessmentResult?> AssessAsync(Guid importId, Guid? assessedBy, CancellationToken ct = default)
    {
        var import = await _imports.GetWithDetailsAsync(importId, ct);
        if (import is null) return null;

        var result = await _risk.AssessAsync(import, ct);

        import.RiskAssessments.Add(new RiskAssessment
        {
            ImportId = import.Id,
            RiskScore = result.Score,
            RiskLevel = result.Level,
            TriggeredRules = JsonSerializer.Serialize(result.Triggered),
            NotEvaluable = JsonSerializer.Serialize(result.NotEvaluable),
            AssessedByUserId = assessedBy,
        });
        import.Status = ImportStatus.Assessed;

        _imports.Update(import);
        await _uow.SaveChangesAsync(ct);
        return result;
    }

    private static IReadOnlyList<TaxLineDto> LinesOf(Import i) =>
        i.TaxBreakdowns.Select(t => new TaxLineDto(t.TaxType, t.BaseAmount, t.Rate, t.Amount)).ToList();

    private static ImportDto ToDto(Import i, IReadOnlyList<TaxLineDto> lines) => new(
        i.Id, i.Consignee, i.HSCode, i.Description, i.InvoiceValueUsd, i.ExchangeRate,
        i.CfrValue, i.OtherCosts, i.AssessableValue, i.TotalTax, i.Status, i.Date, lines);
}
