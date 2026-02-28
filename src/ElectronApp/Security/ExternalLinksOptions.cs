namespace ElectronApp.Security;

/// <summary>
/// HTTPS enforcement and host allowlist for outbound link navigation
/// </summary>
public sealed class ExternalLinksOptions
{
    public const string SectionName = "ExternalLinks";

    public bool RequireHttps { get; set; } = true;

    public string[] AllowedHosts { get; set; } = [];
}
