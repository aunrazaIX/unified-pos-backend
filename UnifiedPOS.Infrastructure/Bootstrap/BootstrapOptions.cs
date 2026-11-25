namespace UnifiedPOS.Infrastructure.Bootstrap;

public sealed class BootstrapOptions
{
    public bool Enabled { get; set; }
    public bool AllowProduction { get; set; }
    public bool AllowDevelopmentShortPassword { get; set; }
    public string OrganizationName { get; set; } = "";
    public string Currency { get; set; } = "";
    public string OwnerUserName { get; set; } = "";
    public string OwnerPassword { get; set; } = "";

    public bool IsValid() => !Enabled ||
        (!string.IsNullOrWhiteSpace(OrganizationName) && OrganizationName.Trim().Length <= 100
        && Currency.Length == 3 && Currency.All(char.IsAsciiLetterUpper)
        && !string.IsNullOrWhiteSpace(OwnerUserName) && OwnerUserName.Trim().Length is >= 3 and <= 100
        && !string.IsNullOrWhiteSpace(OwnerPassword) && OwnerPassword.Length >= (AllowDevelopmentShortPassword ? 9 : 12) && OwnerPassword.Length <= 128);
}
