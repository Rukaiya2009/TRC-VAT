using TRC.Domain.Enums;

namespace TRC.Domain.Entities;

// Append-only send log; supports retry with backoff (FR-13.3).
public class NotificationLog : BaseEntity
{
    public string Recipient { get; set; } = null!;
    public Channel Channel { get; set; }
    public string TemplateKey { get; set; } = null!;
    public Language Language { get; set; }
    public string Status { get; set; } = "Pending";
    public string? ProviderMessageId { get; set; }
    public DateTime? SentAt { get; set; }
    public string? Error { get; set; }
}
