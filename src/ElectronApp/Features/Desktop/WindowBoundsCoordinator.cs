using ElectronApp.Features.Settings.Store;
using ElectronNET.API;
using ElectronNET.API.Entities;

namespace ElectronApp.Features.Desktop;

public sealed partial class WindowBoundsCoordinator(
    IAppSettingsStore settingsStore,
    ILogger<WindowBoundsCoordinator> logger
) : IWindowBoundsCoordinator, IDisposable
{
    private CancellationTokenSource? _boundsDebounce;
    private BrowserWindow? _window;
    private bool _disposed;

    public async Task ApplyInitialBoundsAsync(
        BrowserWindowOptions windowOptions,
        DesktopBootstrapOptions desktopOptions,
        CancellationToken cancellationToken = default
    )
    {
        var savedBounds = await TryLoadWindowBoundsAsync(cancellationToken);
        if (savedBounds is not null)
        {
            windowOptions.X = savedBounds.X;
            windowOptions.Y = savedBounds.Y;
            windowOptions.Width = savedBounds.Width;
            windowOptions.Height = savedBounds.Height;
            return;
        }

        windowOptions.Width = desktopOptions.Width;
        windowOptions.Height = desktopOptions.Height;
        windowOptions.Center = true;
    }

    public void RegisterPersistence(BrowserWindow window)
    {
        _window = window;
        window.OnResize += ScheduleSaveBounds;
        window.OnMove += ScheduleSaveBounds;
    }

    public void Stop()
    {
        _boundsDebounce?.Cancel();
        _boundsDebounce?.Dispose();
        _boundsDebounce = null;
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    private void Dispose(bool disposing)
    {
        if (_disposed || !disposing)
            return;

        Stop();
        _disposed = true;
    }

    private async Task<WindowBounds?> TryLoadWindowBoundsAsync(CancellationToken cancellationToken)
    {
        try
        {
            var settings = await settingsStore.LoadAsync(cancellationToken);
            return settings.WindowBounds;
        }
        catch (Exception ex)
        {
            LogLoadWindowBoundsFailed(logger, ex);
            return null;
        }
    }

    private void ScheduleSaveBounds()
    {
        if (_window is null)
            return;

        _boundsDebounce?.Cancel();
        _boundsDebounce?.Dispose();
        _boundsDebounce = new CancellationTokenSource();
        _ = SaveBoundsAfterDelayAsync(_window, _boundsDebounce.Token);
    }

    private async Task SaveBoundsAfterDelayAsync(
        BrowserWindow window,
        CancellationToken cancellationToken
    )
    {
        try
        {
            await Task.Delay(1000, cancellationToken);

            var rect = await window.GetBoundsAsync();
            var current = await settingsStore.LoadAsync(cancellationToken);

            await settingsStore.SaveAsync(
                current with
                {
                    WindowBounds = new WindowBounds(rect.X, rect.Y, rect.Width, rect.Height),
                },
                cancellationToken
            );
        }
        catch (OperationCanceledException) { }
        catch (Exception ex)
        {
            LogPersistWindowBoundsFailed(logger, ex);
        }
    }

    [LoggerMessage(Level = LogLevel.Warning, Message = "Failed to load saved window bounds.")]
    private static partial void LogLoadWindowBoundsFailed(ILogger logger, Exception exception);

    [LoggerMessage(Level = LogLevel.Warning, Message = "Failed to persist window bounds.")]
    private static partial void LogPersistWindowBoundsFailed(ILogger logger, Exception exception);
}
