using Microsoft.Extensions.Logging;
using TRC.Application.Interfaces;
using TRC.Domain.Enums;

namespace TRC.Infrastructure.Notifications;

// Placeholder until the client provides a WhatsApp/email provider account (dependency D4).
// Swap for a real provider behind this same interface (FR-13.1) — no call-site changes.
public class LoggingNotificationService : INotificationService
{
    private readonly ILogger<LoggingNotificationService> _log;
    public LoggingNotificationService(ILogger<LoggingNotificationService> log) => _log = log;

    public Task SendAsync(string recipient, Channel channel, string templateKey, Language language, CancellationToken ct = default)
    {
        _log.LogInformation("[Notification stub] {Channel} -> {Recipient} template={Template} lang={Lang}",
            channel, recipient, templateKey, language);
        return Task.CompletedTask;
    }
}
