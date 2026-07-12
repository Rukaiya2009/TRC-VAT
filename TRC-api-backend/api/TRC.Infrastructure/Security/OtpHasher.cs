using System.Security.Cryptography;
using System.Text;
using TRC.Application.Interfaces;

namespace TRC.Infrastructure.Security;

// SHA-256 with a fixed pepper. OTPs are 6 digits and live ~3 minutes, so a slow KDF isn't
// needed — but plaintext storage never is. Fixed-time compare avoids leaking via timing.
public class OtpHasher : IOtpHasher
{
    private const string Pepper = "trc-otp-v1";

    public string Hash(string code)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(Pepper + code));
        return Convert.ToHexString(bytes);
    }

    public bool Verify(string hash, string code) =>
        CryptographicOperations.FixedTimeEquals(
            Encoding.UTF8.GetBytes(hash),
            Encoding.UTF8.GetBytes(Hash(code)));
}
