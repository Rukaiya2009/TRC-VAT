namespace TRC.Infrastructure.Auth;

public class JwtOptions
{
    public const string SectionName = "Jwt";
    public string Key { get; set; } = "";
    public string Issuer { get; set; } = "TRC";
    public string Audience { get; set; } = "TRC";
    public int AccessTokenMinutes { get; set; } = 60;   // FR-1.3
}
