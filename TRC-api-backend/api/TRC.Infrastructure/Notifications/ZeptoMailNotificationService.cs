using System.Net.Http.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using TRC.Application.Interfaces;
using TRC.Domain.Enums;

namespace TRC.Infrastructure.Notifications;

// Transactional email via ZeptoMail's HTTP send API. SMTP is intentionally avoided —
// Render blocks outbound SMTP ports. If no token is configured it logs instead, so local
// and CI runs work without a key. Swap templateKey -> real subject/body per template.
public class ZeptoMailNotificationService : INotificationService
{
    private readonly ILogger<ZeptoMailNotificationService> _log;
    private readonly string? _token;
    private readonly string _from;
    private readonly string _fromName;
    private readonly string _endpoint;
    private static readonly HttpClient Http = new();

    public ZeptoMailNotificationService(IConfiguration config, ILogger<ZeptoMailNotificationService> log)
    {
        _log = log;
        _token = config["ZeptoMail:Token"];
        _from = config["ZeptoMail:FromAddress"] ?? "noreply@trc.com.bd";
        _fromName = config["ZeptoMail:FromName"] ?? "TRC";
        _endpoint = config["ZeptoMail:Endpoint"] ?? "https://api.zeptomail.com/v1.1/email";
    }

    public async Task SendAsync(string recipient, Channel channel, string templateKey, Language language, CancellationToken ct = default)
    {
        // WhatsApp is delivered one-tap from the admin's own device, not server-side (client
        // accepted the free tier), so anything not Email is only logged here.
        if (channel != Channel.Email || string.IsNullOrWhiteSpace(_token))
        {
            _log.LogInformation("[Notify:{Channel}] {Template} -> {To} ({Lang})", channel, templateKey, recipient, language);
            return;
        }

        var (subject, body) = Render(templateKey, language);
        var payload = new
        {
            from = new { address = _from, name = _fromName },
            to = new[] { new { email_address = new { address = recipient } } },
            subject,
            htmlbody = body,
        };

        using var req = new HttpRequestMessage(HttpMethod.Post, _endpoint)
        {
            Content = JsonContent.Create(payload),
        };
        req.Headers.TryAddWithoutValidation("Authorization", _token); // ZeptoMail expects "Zoho-enczapikey <token>"

        try
        {
            var res = await Http.SendAsync(req, ct);
            if (!res.IsSuccessStatusCode)
                _log.LogError("ZeptoMail send failed {Status} for {Template} -> {To}", res.StatusCode, templateKey, recipient);
        }
        catch (Exception ex)
        {
            _log.LogError(ex, "ZeptoMail send threw for {Template} -> {To}", templateKey, recipient);
        }
    }

    // Minimal templates. Bengali copy pending native review (D6); English shown for both for now.
    private static (string subject, string body) Render(string key, Language lang) => key switch
    {
        "ConfirmEmail"      => ("Confirm your TRC email", "<p>Please confirm your email to finish setting up your TRC account.</p>"),
        "PasswordReset"     => ("Reset your TRC password", "<p>Use the link we provided to reset your password.</p>"),
        "BookingConfirmed"  => ("Your TRC consultation is booked", "<p>Your consultation is confirmed. Details are in the app.</p>"),
        "MeetingLinkUpdated"=> ("Your TRC meeting link", "<p>Your meeting link is ready. See the app for details.</p>"),
        "BookingCancelled"  => ("Your TRC booking was cancelled", "<p>Your booking has been cancelled.</p>"),
        "PhoneBlocked"      => ("TRC booking access paused", "<p>Your booking access has been paused. Please contact TRC.</p>"),
        _                   => ("TRC notification", "<p>You have a new notification from TRC.</p>"),
    };
}
