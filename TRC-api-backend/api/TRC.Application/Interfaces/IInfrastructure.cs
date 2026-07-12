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

// OTP codes are never stored in plaintext.
public interface IOtpHasher
{
    string Hash(string code);
    bool Verify(string hash, string code);
}

// Short-lived, phone-scoped token minted after successful OTP verification.
// Carries no role claim, so it can never reach staff/admin endpoints.
public interface IPhoneTokenGenerator
{
    string Generate(Guid phoneProfileId, string phone, int minutes);
}

// Today: returns the admin-configured fallback link (or null, for an admin to paste in later).
// Later: a GoogleCalendarMeetingLinkProvider drops in behind this same interface — no call-site
// changes — once TRC supplies a Workspace service account with domain-wide delegation.
public interface IMeetingLinkProvider
{
    Task<string?> CreateAsync(DateTime startUtc, DateTime endUtc, string subject, CancellationToken ct = default);
}
