using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TRC.Application.DTOs;
using TRC.Domain.Repositories;
using TRC.Shared.Common;
using TRC.Shared.Constants;

namespace TRC.API.Controllers;

// Admin tunes the 12 rules without a code change (FR-6.2).
[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = Roles.Admin)]
public class RiskRulesController : ControllerBase
{
    private readonly IRiskRuleRepository _rules;
    private readonly IUnitOfWork _uow;
    public RiskRulesController(IRiskRuleRepository rules, IUnitOfWork uow) { _rules = rules; _uow = uow; }

    [HttpGet]
    public async Task<IActionResult> List(CancellationToken ct)
    {
        var rules = await _rules.ListAsync(ct);
        var dto = rules.OrderBy(r => r.Code)
            .Select(r => new RiskRuleDto(r.Id, r.Code, r.Name, r.Weight, r.Enabled, r.PhaseTag, r.Rationale))
            .ToList();
        return Ok(ApiResponse<IReadOnlyList<RiskRuleDto>>.Ok(dto));
    }

    [HttpPut("{code}")]
    public async Task<IActionResult> Update(string code, UpdateRiskRuleRequest req, CancellationToken ct)
    {
        var rule = await _rules.GetByCodeAsync(code, ct);
        if (rule is null) return NotFound(ApiResponse<object>.Fail("Rule not found."));
        rule.Weight = req.Weight;
        rule.Enabled = req.Enabled;
        rule.LastReviewed = DateTime.UtcNow;
        _rules.Update(rule);
        await _uow.SaveChangesAsync(ct);
        return Ok(ApiResponse<object>.Ok(new { rule.Code, rule.Weight, rule.Enabled }));
    }
}
