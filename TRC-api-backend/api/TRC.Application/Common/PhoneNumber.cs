using System.Text.RegularExpressions;

namespace TRC.Application.Common;

// Normalised to E.164 so the same person cannot dodge a block by reformatting
// (01799707090 / 8801799707090 / +880 1799-707090 all collapse to +8801799707090).
public static class PhoneNumber
{
    private const string BdCountryCode = "880";

    public static string Normalize(string raw)
    {
        if (string.IsNullOrWhiteSpace(raw))
            throw new ArgumentException("Phone number is required.");

        var digits = Regex.Replace(raw, @"[^\d+]", "");
        digits = digits.StartsWith('+') ? digits[1..] : digits;

        if (digits.StartsWith("00")) digits = digits[2..];
        if (digits.StartsWith('0')) digits = BdCountryCode + digits[1..];       // local 01X… -> 8801X…
        else if (digits.StartsWith("1") && digits.Length == 10) digits = BdCountryCode + digits;

        return "+" + digits;
    }

    public static bool IsPlausible(string normalized) =>
        Regex.IsMatch(normalized, @"^\+\d{10,15}$");
}
