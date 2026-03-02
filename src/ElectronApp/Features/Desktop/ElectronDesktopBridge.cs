using System.Runtime.InteropServices;
using ElectronNET.API;

namespace ElectronApp.Features.Desktop;

public sealed partial class ElectronDesktopBridge(ILogger<ElectronDesktopBridge> logger)
    : IDesktopBridge
{
    public Task<DesktopEnvironmentInfo> GetEnvironmentInfoAsync(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var info = new DesktopEnvironmentInfo(
            HybridSupport.IsElectronActive,
            RuntimeInformation.OSDescription,
            Environment.Version.ToString()
        );

        return Task.FromResult(info);
    }

    public async Task<bool> OpenExternalAsync(Uri uri, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (!HybridSupport.IsElectronActive)
        {
            LogElectronRuntimeInactiveForExternalOpen(logger, uri);
            return false;
        }

        await Electron.Shell.OpenExternalAsync(uri.ToString());
        return true;
    }

    public Task MinimizeWindowAsync(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (!HybridSupport.IsElectronActive)
            return Task.CompletedTask;

        Electron.WindowManager.BrowserWindows.First().Minimize();
        return Task.CompletedTask;
    }

    public async Task MaximizeOrRestoreWindowAsync(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (!HybridSupport.IsElectronActive)
            return;

        var window = Electron.WindowManager.BrowserWindows.First();

        if (await window.IsMaximizedAsync())
            window.Unmaximize();
        else
            window.Maximize();
    }

    public Task CloseWindowAsync(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (!HybridSupport.IsElectronActive)
            return Task.CompletedTask;

        Electron.WindowManager.BrowserWindows.First().Close();
        return Task.CompletedTask;
    }

    public async Task<bool> IsMaximizedAsync(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (!HybridSupport.IsElectronActive)
            return false;

        return await Electron.WindowManager.BrowserWindows.First().IsMaximizedAsync();
    }

    public event Action<bool>? MaximizedChanged;

    public void RegisterWindowEvents()
    {
        if (!HybridSupport.IsElectronActive)
            return;

        var window = Electron.WindowManager.BrowserWindows.First();
        window.OnMaximize += () => MaximizedChanged?.Invoke(true);
        window.OnUnmaximize += () => MaximizedChanged?.Invoke(false);
    }

    [LoggerMessage(
        Level = LogLevel.Warning,
        Message = "Blocked external open because Electron runtime is inactive: {Uri}"
    )]
    private static partial void LogElectronRuntimeInactiveForExternalOpen(ILogger logger, Uri uri);
}
