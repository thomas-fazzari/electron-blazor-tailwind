namespace ElectronApp.Features.Desktop;

/// <summary>
/// Abstracts Electron desktop APIs so components can be tested without a live runtime
/// </summary>
public interface IDesktopBridge
{
    /// <summary>
    /// Returns runtime environment details (platform, .NET version, Electron availability)
    /// </summary>
    Task<DesktopEnvironmentInfo> GetEnvironmentInfoAsync(CancellationToken cancellationToken);

    /// <summary>
    /// Opens a URI in the user's default browser via Electron shell
    /// </summary>
    Task<bool> OpenExternalAsync(Uri uri, CancellationToken cancellationToken);
}

public sealed record DesktopEnvironmentInfo(
    bool IsElectronActive,
    string Platform,
    string DotnetVersion
);
