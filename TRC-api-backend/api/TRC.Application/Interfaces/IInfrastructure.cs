using TRC.Domain.Entities;
using TRC.Domain.Enums;

namespace TRC.Application.Interfaces;

// Implemented in Infrastructure — keeps Application free of framework deps.
public interface IJwtTokenGenerator
{
    string GenerateAccessToken(User user);
    (string token, string hash) GenerateRefreshToken();
    string Hash(string value);
}

public interface IPasswordHasher
{
    string Hash(string password);
    bool Verify(string hash, string password);
}

// Provider-abstracted notifications (FR-13.1). WhatsApp + HTTP email.
public interface INotificationService
{
    Task SendAsync(string recipient, Channel channel, string templateKey, Language language, CancellationToken ct = default);
}
