using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using TRC.Application.Interfaces;

namespace TRC.Infrastructure.Auth;

// Minted only after a successful OTP verification. Same signing key/issuer/audience as the
// staff token so the existing JwtBearer handler validates it — but it carries NO role claim
// and a scope=phone claim, so it can satisfy the "PhoneVerified" policy and nothing else.
public class PhoneTokenGenerator : IPhoneTokenGenerator
{
    public const string ScopeClaim = "scope";
    public const string ScopeValue = "phone";
    public const string PhoneClaim = "phone";

    private readonly JwtOptions _opts;
    public PhoneTokenGenerator(IOptions<JwtOptions> opts) => _opts = opts.Value;

    public string Generate(Guid phoneProfileId, string phone, int minutes)
    {
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, phoneProfileId.ToString()),
            new(PhoneClaim, phone),
            new(ScopeClaim, ScopeValue),
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_opts.Key));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var token = new JwtSecurityToken(
            issuer: _opts.Issuer,
            audience: _opts.Audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(minutes),
            signingCredentials: creds);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
