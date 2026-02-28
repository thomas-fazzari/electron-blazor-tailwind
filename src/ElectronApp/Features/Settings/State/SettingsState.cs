using ElectronApp.Features.Settings.Store;
using Fluxor;

namespace ElectronApp.Features.Settings.State;

[FeatureState]
public sealed record SettingsState
{
    public bool IsLoading { get; init; }
    public bool IsLoaded { get; init; }
    public bool IsSaving { get; init; }
    public UserSettings Settings { get; init; } = UserSettings.Default;
    public string? ErrorMessage { get; init; }
}
