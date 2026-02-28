using ElectronApp.Features.Settings.Store;

namespace ElectronApp.Features.Settings.State;

public readonly record struct LoadSettingsAction;

public sealed record SettingsLoadedAction(UserSettings Settings);

public sealed record SettingsLoadFailedAction(string ErrorMessage);

public sealed record SetThemeAction(AppTheme Theme);

public sealed record SaveSettingsAction(UserSettings Settings);

public readonly record struct SettingsSavedAction;

public sealed record SettingsSaveFailedAction(string ErrorMessage);
