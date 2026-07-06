using TRC.Application.DTOs;
using TRC.Application.Interfaces;
using TRC.Domain.Entities;
using TRC.Domain.Repositories;

namespace TRC.Application.Services;

public class AuthService : IAuthService
{
    private readonly IUserRepository _users;
    private readonly IPasswordHasher _hasher;
    private readonly IJwtTokenGenerator _jwt;
    private readonly IUnitOfWork _uow;

    public AuthService(IUserRepository users, IPasswordHasher hasher, IJwtTokenGenerator jwt, IUnitOfWork uow)
    {
        _users = users;
        _hasher = hasher;
        _jwt = jwt;
        _uow = uow;
    }

    public async Task<AuthResult> RegisterAsync(RegisterRequest r, CancellationToken ct = default)
    {
        var existing = await _users.GetByEmailAsync(r.Email, ct);
        if (existing is not null)
            throw new InvalidOperationException("A user with this email already exists.");

        var user = new User
        {
            Email = r.Email.Trim().ToLowerInvariant(),
            PasswordHash = _hasher.Hash(r.Password),
            FullName = r.FullName,
            Role = r.Role,
            PreferredLanguage = r.PreferredLanguage,
        };
        await _users.AddAsync(user, ct);
        await _uow.SaveChangesAsync(ct);
        return await IssueAsync(user, ct);
    }

    public async Task<AuthResult?> LoginAsync(LoginRequest r, CancellationToken ct = default)
    {
        var user = await _users.GetByEmailAsync(r.Email.Trim().ToLowerInvariant(), ct);
        if (user is null || !user.IsActive || !_hasher.Verify(user.PasswordHash, r.Password))
            return null;

        user.LastLogin = DateTime.UtcNow;
        var result = await IssueAsync(user, ct);
        _users.Update(user);
        await _uow.SaveChangesAsync(ct);
        return result;
    }

    public async Task<AuthResult?> RefreshAsync(string refreshToken, CancellationToken ct = default)
    {
        var hash = _jwt.Hash(refreshToken);
        var users = await _users.ListAsync(ct);
        var user = users.FirstOrDefault(u => u.RefreshTokenHash == hash && u.RefreshTokenExpiresAt > DateTime.UtcNow);
        if (user is null) return null;
        return await IssueAsync(user, ct);
    }

    private async Task<AuthResult> IssueAsync(User user, CancellationToken ct)
    {
        var access = _jwt.GenerateAccessToken(user);
        var (refresh, refreshHash) = _jwt.GenerateRefreshToken();
        user.RefreshTokenHash = refreshHash;
        user.RefreshTokenExpiresAt = DateTime.UtcNow.AddDays(7);
        _users.Update(user);
        await _uow.SaveChangesAsync(ct);
        return new AuthResult(access, refresh, user.FullName, user.Role, user.PreferredLanguage);
    }
}
