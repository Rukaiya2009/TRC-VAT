namespace TRC.Shared.Constants;

public static class Roles
{
    public const string Admin = "Admin";
    public const string Auditor = "Auditor";
    public const string Prospect = "Prospect";
    public const string Importer = "Importer";
}

public static class TaxRateKeys
{
    public const string CD = "CD";
    public const string RD = "RD";
    public const string SD = "SD";
    public const string VAT = "VAT";
    public const string AIT = "AIT";
    public const string AT = "AT";
    public const string ATV = "ATV";
}

// Approved bands (FR-6.3): 0-10 Low, 11-25 Medium, 26+ High.
public static class RiskBands
{
    public const int MediumFrom = 11;
    public const int HighFrom = 26;
}
