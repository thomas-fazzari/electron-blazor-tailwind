namespace ElectronApp.Features.Desktop;

public readonly record struct InitializeDesktopAction;

public sealed record DesktopInitializedAction(DesktopEnvironmentInfo Environment);

public sealed record DesktopInitializationFailedAction(string ErrorMessage);
