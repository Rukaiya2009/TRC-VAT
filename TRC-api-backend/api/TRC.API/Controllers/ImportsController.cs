using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TRC.Application.DTOs;
using TRC.Application.Interfaces;
using TRC.Shared.Common;
using TRC.Shared.Constants;

namespace TRC.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = $"{Roles.Auditor},{Roles.Admin}")]
public class ImportsController : ControllerBase
{
    private readonly IImportService _imports;
    public ImportsController(IImportService imports) => _imports = imports;

    private Guid? CurrentUserId =>
        Guid.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub"), out var id)
            ? id : null;

    [HttpGet]
    public async Task<IActionResult> List(CancellationToken ct)
        => Ok(ApiResponse<IReadOnlyList<ImportDto>>.Ok(await _imports.ListAsync(ct)));

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> Get(Guid id, CancellationToken ct)
    {
        var dto = await _imports.GetAsync(id, ct);
        return dto is null ? NotFound(ApiResponse<object>.Fail("Import not found.")) : Ok(ApiResponse<ImportDto>.Ok(dto));
    }

    // Creating an import triggers the tax calculation (FR-3.2).
    [HttpPost]
    public async Task<IActionResult> Create(CreateImportRequest req, CancellationToken ct)
    {
        var dto = await _imports.CreateAsync(req, CurrentUserId, ct);
        return CreatedAtAction(nameof(Get), new { id = dto.Id }, ApiResponse<ImportDto>.Ok(dto));
    }

    // Run the risk engine (FR-6.1).
    [HttpPost("{id:guid}/assess")]
    public async Task<IActionResult> Assess(Guid id, CancellationToken ct)
    {
        var result = await _imports.AssessAsync(id, CurrentUserId, ct);
        return result is null
            ? NotFound(ApiResponse<object>.Fail("Import not found."))
            : Ok(ApiResponse<RiskAssessmentResult>.Ok(result));
    }
}
