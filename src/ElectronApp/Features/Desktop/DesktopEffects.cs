using Fluxor;

namespace ElectronApp.Features.Desktop;

public sealed partial class DesktopEffects(
    IDesktopBridge desktopBridge,
    ILogger<DesktopEffects> logger
)
{
    [EffectMethod]
    public async Task HandleInitializeAsync(InitializeDesktopAction action, IDispatcher dispatcher)
    {
        try
        {
            var environment = await desktopBridge.GetEnvironmentInfoAsync(CancellationToken.None);
            dispatcher.Dispatch(new DesktopInitializedAction(environment));
        }
        catch (Exception ex)
        {
            LogDesktopInitializationFailed(logger, ex);
            dispatcher.Dispatch(
                new DesktopInitializationFailedAction("Desktop initialization failed.")
            );
        }
    }

    [LoggerMessage(Level = LogLevel.Error, Message = "Desktop initialization failed.")]
    private static partial void LogDesktopInitializationFailed(ILogger logger, Exception exception);
}
