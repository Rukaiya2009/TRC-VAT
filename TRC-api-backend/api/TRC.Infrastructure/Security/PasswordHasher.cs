using Microsoft.AspNetCore.Identity;
using TRC.Application.Interfaces;
using TRC.Domain.Entities;

namespace TRC.Infrastructure.Security;

// Wraps ASP.NET Core Identity's PBKDF2 hasher (FR-1.2).
public class PasswordHasher : IPasswordHasher
{
    private readonly PasswordHasher<User> _inner = new();
    private static readonly User Dummy = new();

    public string Hash(string password) => _inner.HashPassword(Dummy, password);

    public bool Verify(string hash, string password) =>
        _inner.VerifyHashedPassword(Dummy, hash, password) != PasswordVerificationResult.Failed;
}
