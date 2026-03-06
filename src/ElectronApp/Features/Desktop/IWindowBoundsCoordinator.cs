using ElectronNET.API;
using ElectronNET.API.Entities;

namespace ElectronApp.Features.Desktop;

/// <summary>
/// Coordinates window bounds restoration and persistence for the main Electron window
/// </summary>
public interface IWindowBoundsCoordinator
{
    /// <summary>
    /// Applies saved bounds when available, otherwise applies defaults
    /// </summary>
    Task ApplyInitialBoundsAsync(
        BrowserWindowOptions windowOptions,
        DesktopBootstrapOptions desktopOptions,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Registers window move and resize handlers to persist bounds
    /// </summary>
    void RegisterPersistence(BrowserWindow window);

    /// <summary>
    /// Stops pending persistence operations and releases debounce resources
    /// </summary>
    void Stop();
}
