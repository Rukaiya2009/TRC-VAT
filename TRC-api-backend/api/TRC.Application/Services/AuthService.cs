using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using TRC.Application.DTOs;
using TRC.Application.Interfaces;
using TRC.Application.Options;
using TRC.Domain.Entities;
using TRC.Domain.Enums;

namespace TRC.Application.Services;

// Auth on ASP.NET Core Identity (email pivot, 26 Jul 2026).
//   • Public register -> always Prospect, sends email-confirmation token.
//   • Login uses SignInManager with lockoutOnFailure (5 fails -> 15 min) and blocks
//     unconfirmed emails (RequireConfirmedEmail).
//   • Forgot/reset + confirm use Identity's cryptographic token providers.
//   • JWT issuance stays custom (IJwtTokenGenerator) on successful sign-in.
public class AuthService : IAuthService
{
    private readonly UserManager<User> _users;
    private readonly SignInManager<User> _signin;
    private readonly IJwtTokenGenerator _jwt;
    private readonly INotificationService _notify;
    private readonly bool _devReturnTokens;

    public AuthService(
        UserManager<User> users,
        SignInManager<User> signin,
        IJwtTokenGenerator jwt,
        INotificationService notify,
        IOptions<AuthDevOptions> dev)
    {
        _users = users;
        _signin = signin;
        _jwt = jwt;
        _notify = notify;
        _devReturnTokens = dev.Value.DevReturnTokens;
    }

    public async Task<RegisterResult> RegisterProspectAsync(RegisterRequest r, CancellationToken ct = default)
    {
        var email = r.Email.Trim().ToLowerInvariant();

        var user = new User
        {
            Email = email,
            UserName = email,
            PhoneNumber = r.Phone.Trim(),
            FullName = r.FullName.Trim(),
            Role = UserRole.Prospect,              // forced — never taken from the request
            PreferredLanguage = r.PreferredLanguage,
            IsActive = true,
        };

        var created = await _users.CreateAsync(user, r.Password);
        if (!created.Succeeded)
            throw new InvalidOperationException(string.Join(" ", created.Errors.Select(e => e.Description)));

        await _users.AddToRoleAsync(user, UserRole.Prospect.ToString());

        var token = await _users.GenerateEmailConfirmationTokenAsync(user);
        await _notify.SendAsync(email, Channel.Email, "ConfirmEmail", user.PreferredLanguage, ct);

        var auth = await IssueAsync(user);
        return new RegisterResult(auth, _devReturnTokens ? token : null);
    }

    public async Task<AuthResult?> LoginAsync(LoginRequest r, CancellationToken ct = default)
    {
        var user = await _users.FindByEmailAsync(r.Email.Trim().ToLowerInvariant());
        if (user is null || !user.IsActive) return null;

        var result = await _signin.CheckPasswordSignInAsync(user, r.Password, lockoutOnFailure: true);
        if (result.IsLockedOut)
            throw new InvalidOperationException("Account locked after too many attempts. Try again in 15 minutes.");
        if (result.IsNotAllowed)
            throw new InvalidOperationException("Please confirm your email address before signing in.");
        if (!result.Succeeded) return null;

        user.LastLogin = DateTime.UtcNow;
        await _users.UpdateAsync(user);
        return await IssueAsync(user);
    }

    public async Task<AuthResult?> RefreshAsync(string refreshToken, CancellationToken ct = default)
    {
        var hash = _jwt.Hash(refreshToken);
        var user = _users.Users.FirstOrDefault(u =>
            u.RefreshTokenHash == hash && u.RefreshTokenExpiresAt > DateTime.UtcNow);
        return user is null ? null : await IssueAsync(user);
    }

    public async Task<bool> ConfirmEmailAsync(ConfirmEmailRequest r, CancellationToken ct = default)
    {
        var user = await _users.FindByEmailAsync(r.Email.Trim().ToLowerInvariant());
        if (user is null) return false;
        var result = await _users.ConfirmEmailAsync(user, r.Token);
        return result.Succeeded;
    }

    public async Task<string?> ResendConfirmationAsync(string email, CancellationToken ct = default)
    {
        var user = await _users.FindByEmailAsync(email.Trim().ToLowerInvariant());
        if (user is null || user.EmailConfirmed) return null;
        var token = await _users.GenerateEmailConfirmationTokenAsync(user);
        await _notify.SendAsync(user.Email!, Channel.Email, "ConfirmEmail", user.PreferredLanguage, ct);
        return _devReturnTokens ? token : null;
    }

    public async Task<string?> ForgotPasswordAsync(string email, CancellationToken ct = default)
    {
        var user = await _users.FindByEmailAsync(email.Trim().ToLowerInvariant());
        if (user is null) return null;   // don't reveal whether the email exists
        var token = await _users.GeneratePasswordResetTokenAsync(user);
        await _notify.SendAsync(user.Email!, Channel.Email, "PasswordReset", user.PreferredLanguage, ct);
        return _devReturnTokens ? token : null;
    }

    public async Task<bool> ResetPasswordAsync(ResetPasswordRequest r, CancellationToken ct = default)
    {
        var user = await _users.FindByEmailAsync(r.Email.Trim().ToLowerInvariant());
        if (user is null) return false;
        var result = await _users.ResetPasswordAsync(user, r.Token, r.NewPassword);
        return result.Succeeded;
    }

    public async Task<AuthResult> CreateStaffAsync(CreateStaffRequest r, CancellationToken ct = default)
    {
        var email = r.Email.Trim().ToLowerInvariant();
        var user = new User
        {
            Email = email,
            UserName = email,
            FullName = r.FullName.Trim(),
            Role = r.Role,
            PreferredLanguage = r.PreferredLanguage,
            IsActive = true,
            EmailConfirmed = true,   // an Admin vouches for staff accounts
        };
        var created = await _users.CreateAsync(user, r.Password);
        if (!created.Succeeded)
            throw new InvalidOperationException(string.Join(" ", created.Errors.Select(e => e.Description)));
        await _users.AddToRoleAsync(user, r.Role.ToString());
        return await IssueAsync(user);
    }

    private async Task<AuthResult> IssueAsync(User user)
    {
        var access = _jwt.GenerateAccessToken(user);
        var (refresh, refreshHash) = _jwt.GenerateRefreshToken();
        user.RefreshTokenHash = refreshHash;
        user.RefreshTokenExpiresAt = DateTime.UtcNow.AddDays(7);
        await _users.UpdateAsync(user);
        return new AuthResult(access, refresh, user.FullName, user.Role, user.PreferredLanguage, user.EmailConfirmed);
    }
}
