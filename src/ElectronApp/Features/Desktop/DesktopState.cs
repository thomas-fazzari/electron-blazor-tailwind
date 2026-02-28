using Fluxor;

namespace ElectronApp.Features.Desktop;

[FeatureState]
public sealed record DesktopState
{
    public bool IsInitializing { get; init; }
    public bool IsInitialized { get; init; }
    public bool IsElectronActive { get; init; }
    public string Platform { get; init; } = "Unknown";
    public string DotnetVersion { get; init; } = string.Empty;
    public string? ErrorMessage { get; init; }
}
