namespace TRC.Application.Options;

// While true, email-confirmation and password-reset tokens are returned in the API response
// so the flow is testable without a live inbox. MUST be false in production.
public class AuthDevOptions
{
    public const string SectionName = "AuthDev";
    public bool DevReturnTokens { get; set; } = false;
}
