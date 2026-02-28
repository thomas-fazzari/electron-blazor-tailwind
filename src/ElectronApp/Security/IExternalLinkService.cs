namespace ElectronApp.Security;

/// <summary>
/// Validates and opens external URLs through the allowlisted host policy
/// </summary>
public interface IExternalLinkService
{
    /// <summary>
    /// Opens <paramref name="url"/> in the default browser if it passes HTTPS and host allowlist checks
    /// </summary>
    Task<bool> TryOpenAsync(string url, CancellationToken cancellationToken = default);
}
