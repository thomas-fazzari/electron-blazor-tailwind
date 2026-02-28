namespace ElectronApp.Features.Settings.Store;

/// <summary>
/// Serializable user preferences persisted between sessions
/// </summary>
public sealed record UserSettings
{
    public static UserSettings Default { get; } = new();

    public AppTheme Theme { get; init; } = AppTheme.Dark;
    public WindowBounds? WindowBounds { get; init; }
}

public sealed record WindowBounds(int X, int Y, int Width, int Height);
