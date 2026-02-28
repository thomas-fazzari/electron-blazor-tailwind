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

    [LoggerMessage(
        Level = LogLevel.Warning,
        Message = "Blocked external open because Electron runtime is inactive: {Uri}"
    )]
    private static partial void LogElectronRuntimeInactiveForExternalOpen(ILogger logger, Uri uri);
}
