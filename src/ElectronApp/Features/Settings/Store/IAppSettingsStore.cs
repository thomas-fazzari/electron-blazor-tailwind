namespace ElectronApp.Features.Settings.Store;

/// <summary>
/// Persists and retrieves user preferences
/// </summary>
public interface IAppSettingsStore
{
    Task<UserSettings> LoadAsync(CancellationToken cancellationToken = default);
    Task SaveAsync(UserSettings settings, CancellationToken cancellationToken = default);
}
