using Microsoft.AspNetCore.Mvc;
using TRC.Application.DTOs;
using TRC.Application.Interfaces;
using TRC.Shared.Common;

namespace TRC.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class QuickChecksController : ControllerBase
{
    private readonly IVarianceService _variance;
    public QuickChecksController(IVarianceService variance) => _variance = variance;

    // Public prospect gauge (§8.2). OTP-gating (FR-9.1) is added in Phase 4.
    [HttpPost]
    public IActionResult Evaluate(QuickCheckRequest req)
        => Ok(ApiResponse<QuickCheckResult>.Ok(_variance.Evaluate(req.PaymentValue, req.BoeValue)));
}
