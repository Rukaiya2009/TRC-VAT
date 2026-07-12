using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TRC.Application.DTOs;
using TRC.Application.Interfaces;
using TRC.Shared.Common;

namespace TRC.API.Controllers;

// M9 — phone verification. Public: this is the gate, not something behind the gate.
[ApiController]
[Route("api/otp")]
[AllowAnonymous]
public class OtpController : ControllerBase
{
    private readonly IOtpService _otp;
    public OtpController(IOtpService otp) => _otp = otp;

    // 6-digit code, ~3 min expiry, 5 sends/hour/phone. Blocked numbers are refused.
    [HttpPost("send")]
    public async Task<IActionResult> Send(SendOtpRequest req, CancellationToken ct)
        => Ok(ApiResponse<SendOtpResult>.Ok(await _otp.SendAsync(req, ct)));

    // On success returns a short-lived phone token — the credential for every booking endpoint.
    [HttpPost("verify")]
    public async Task<IActionResult> Verify(VerifyOtpRequest req, CancellationToken ct)
        => Ok(ApiResponse<VerifyOtpResult>.Ok(await _otp.VerifyAsync(req, ct)));
}
