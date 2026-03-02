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

    /// <summary>
    /// Minimizes the main browser window
    /// </summary>
    Task MinimizeWindowAsync(CancellationToken cancellationToken);

    /// <summary>
    /// Toggles the main browser window between maximized and restored states
    /// </summary>
    Task MaximizeOrRestoreWindowAsync(CancellationToken cancellationToken);

    /// <summary>
    /// Closes the main browser window
    /// </summary>
    Task CloseWindowAsync(CancellationToken cancellationToken);

    /// <summary>
    /// Returns whether the main browser window is currently maximized
    /// </summary>
    Task<bool> IsMaximizedAsync(CancellationToken cancellationToken);

    /// <summary>
    /// Raised when the window is maximized or restored
    /// </summary>
    event Action<bool>? MaximizedChanged;
}

public sealed record DesktopEnvironmentInfo(
    bool IsElectronActive,
    string Platform,
    string DotnetVersion
);
