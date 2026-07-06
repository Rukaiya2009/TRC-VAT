using Microsoft.AspNetCore.Mvc;
using TRC.Application.DTOs;
using TRC.Application.Interfaces;
using TRC.Shared.Common;
using TRC.Shared.Constants;
using Microsoft.AspNetCore.Authorization;

namespace TRC.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _auth;
    public AuthController(IAuthService auth) => _auth = auth;

    // Registration restricted to Admin per SRS §11 (role assignment is sensitive).
    // Left [AllowAnonymous] here so you can create the first Admin; lock it down after.
    [AllowAnonymous]
    [HttpPost("register")]
    public async Task<IActionResult> Register(RegisterRequest req, CancellationToken ct)
        => Ok(ApiResponse<AuthResult>.Ok(await _auth.RegisterAsync(req, ct)));

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
}
