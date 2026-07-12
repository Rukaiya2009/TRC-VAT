namespace TRC.Application.Options;

// FR-9.2 rate limits. DevReturnCode=true surfaces the code in the API response so the
// flow is testable before TRC supplies a WhatsApp/email provider (dependency D4).
// MUST be set false in production.
public class OtpOptions
{
    public const string SectionName = "Otp";

    public int CodeLength { get; set; } = 6;
    public int ExpiryMinutes { get; set; } = 3;
    public int MaxAttemptsPerCode { get; set; } = 3;
    public int MaxSendsPerHour { get; set; } = 5;
    public int PhoneTokenMinutes { get; set; } = 30;
    public bool DevReturnCode { get; set; } = true;
}
