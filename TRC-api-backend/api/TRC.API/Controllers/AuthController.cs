using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TRC.Application.DTOs;
using TRC.Application.Interfaces;
using TRC.Shared.Common;

namespace TRC.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _auth;
    public AuthController(IAuthService auth) => _auth = auth;

    // Public self-registration ALWAYS creates a Prospect (no role in the request), which
    // closes the old self-register-as-Admin hole. Staff go through create-staff below.
    [AllowAnonymous]
    [HttpPost("register")]
    public async Task<IActionResult> Register(RegisterRequest req, CancellationToken ct)
        => Ok(ApiResponse<RegisterResult>.Ok(await _auth.RegisterProspectAsync(req, ct)));

    [AllowAnonymous]
    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginRequest req, CancellationToken ct)
    {
        var result = await _auth.LoginAsync(req, ct);
        return result is null
            ? Unauthorized(ApiResponse<object>.Fail("Invalid email or password."))
            : Ok(ApiResponse<AuthResult>.Ok(result));
    }

    [AllowAnonymous]
    [HttpPost("refresh")]
    public async Task<IActionResult> Refresh(RefreshRequest req, CancellationToken ct)
    {
        var result = await _auth.RefreshAsync(req.RefreshToken, ct);
        return result is null
            ? Unauthorized(ApiResponse<object>.Fail("Invalid or expired refresh token."))
            : Ok(ApiResponse<AuthResult>.Ok(result));
    }

    [AllowAnonymous]
    [HttpPost("confirm-email")]
    public async Task<IActionResult> ConfirmEmail(ConfirmEmailRequest req, CancellationToken ct)
    {
        var ok = await _auth.ConfirmEmailAsync(req, ct);
        return ok
            ? Ok(ApiResponse<object>.Ok(new { message = "Email confirmed." }))
            : BadRequest(ApiResponse<object>.Fail("That confirmation link is invalid or expired."));
    }

    [AllowAnonymous]
    [HttpPost("resend-confirmation")]
    public async Task<IActionResult> Resend(ResendConfirmationRequest req, CancellationToken ct)
    {
        var devToken = await _auth.ResendConfirmationAsync(req.Email, ct);
        return Ok(ApiResponse<object>.Ok(new { message = "If the account exists and is unconfirmed, a new email has been sent.", devToken }));
    }

    [AllowAnonymous]
    [HttpPost("forgot-password")]
    public async Task<IActionResult> Forgot(ForgotPasswordRequest req, CancellationToken ct)
    {
        var devToken = await _auth.ForgotPasswordAsync(req.Email, ct);
        return Ok(ApiResponse<object>.Ok(new { message = "If that email exists, a reset link has been sent.", devToken }));
    }

    [AllowAnonymous]
    [HttpPost("reset-password")]
    public async Task<IActionResult> Reset(ResetPasswordRequest req, CancellationToken ct)
    {
        var ok = await _auth.ResetPasswordAsync(req, ct);
        return ok
            ? Ok(ApiResponse<object>.Ok(new { message = "Password reset. You can now sign in." }))
            : BadRequest(ApiResponse<object>.Fail("That reset link is invalid or expired."));
    }

    // Only an Admin can mint staff (Admin/Auditor) accounts.
    [Authorize(Roles = "Admin")]
    [HttpPost("create-staff")]
    public async Task<IActionResult> CreateStaff(CreateStaffRequest req, CancellationToken ct)
        => Ok(ApiResponse<AuthResult>.Ok(await _auth.CreateStaffAsync(req, ct)));
}
