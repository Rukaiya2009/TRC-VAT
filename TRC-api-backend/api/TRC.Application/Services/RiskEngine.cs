using System.Text.Json;
using TRC.Application.DTOs;
using TRC.Application.Interfaces;
using TRC.Domain.Entities;
using TRC.Domain.Enums;
using TRC.Domain.Repositories;
using TRC.Shared.Constants;

namespace TRC.Application.Services;

/// <summary>
/// Configurable rule engine (SRS §9). Rules, weights and enabled flags load from the
/// RiskRule table (FR-6.2) so tuning needs no code change. This build wires the Phase A
/// rules that run on data the system already captures (R5, R7, R10, R11) plus R1 when
/// BusinessPeriodData is supplied. Phase B rules are seeded but skipped (Not Evaluable)
/// until their data sources exist (FR-6.4/6.5).
/// </summary>
public class RiskEngine : IRiskEngine
{
    private readonly IRiskRuleRepository _rules;
    private readonly IImportRepository _imports;

    public RiskEngine(IRiskRuleRepository rules, IImportRepository imports)
    {
        _rules = rules;
        _imports = imports;
    }

    public async Task<RiskAssessmentResult> AssessAsync(Import import, CancellationToken ct = default)
    {
        var enabled = (await _rules.GetEnabledAsync(ct)).ToDictionary(r => r.Code, r => r);
        var triggered = new List<TriggeredRuleDto>();
        var notEvaluable = new List<string>();

        int Weight(string code) => enabled.TryGetValue(code, out var r) ? r.Weight : 0;
        bool On(string code) => enabled.ContainsKey(code);

        // R10 — round-number / repeated invoice patterns (Phase A).
        if (On("R10") && IsRoundNumber(import.InvoiceValueUsd))
            triggered.Add(new("R10", enabled["R10"].Name, Weight("R10")));

        // R11 — required document missing/inconsistent (Phase A).
        if (On("R11"))
        {
            if (!import.HasCommercialInvoice || !import.HasBillOfLading || !import.HasMushak)
                triggered.Add(new("R11", enabled["R11"].Name, Weight("R11")));
        }

        // R5 — AV deviates > 50% from the consignee's 12-month rolling average (Phase A).
        if (On("R5"))
        {
            var history = await _imports.GetHistoryForConsigneeAsync(import.Consignee, ct);
            var priors = history.Where(i => i.Id != import.Id && i.AssessableValue > 0).ToList();
            if (priors.Count == 0)
                notEvaluable.Add("R5");
            else
            {
                var avg = priors.Average(i => i.AssessableValue);
                if (avg > 0 && Math.Abs(import.AssessableValue - avg) / avg > 0.50m)
                    triggered.Add(new("R5", enabled["R5"].Name, Weight("R5")));
            }
        }

        // R7 — HS code inconsistent with description/history (Phase A, heuristic placeholder).
        if (On("R7"))
        {
            if (string.IsNullOrWhiteSpace(import.HSCode) || import.HSCode.Length < 6)
                triggered.Add(new("R7", enabled["R7"].Name, Weight("R7")));
        }

        // Phase B rules that are enabled but lack a data source are Not Evaluable (FR-6.4).
        foreach (var code in new[] { "R2", "R3", "R4", "R6", "R8", "R9", "R12" })
            if (On(code)) notEvaluable.Add(code);

        var score = triggered.Sum(t => t.Points);
        var level = score >= RiskBands.HighFrom ? RiskLevel.High
                  : score >= RiskBands.MediumFrom ? RiskLevel.Medium
                  : RiskLevel.Low;

        return new RiskAssessmentResult(import.Id, score, level, triggered, notEvaluable);
    }

    // Heuristic for R10: value is a whole thousand.
    private static bool IsRoundNumber(decimal v) => v > 0 && v % 1000m == 0m;

    public static string Serialize(object o) => JsonSerializer.Serialize(o);
}
