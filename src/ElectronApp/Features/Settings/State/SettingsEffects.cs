using ElectronApp.Features.Settings.Store;
using Fluxor;
using Microsoft.Extensions.Logging;

namespace ElectronApp.Features.Settings.State;

public sealed partial class SettingsEffects(
    IAppSettingsStore settingsStore,
    IState<SettingsState> settingsState,
    ILogger<SettingsEffects> logger
)
{
    [EffectMethod]
    public Task HandleSetThemeAsync(SetThemeAction action, IDispatcher dispatcher)
    {
        dispatcher.Dispatch(new SaveSettingsAction(settingsState.Value.Settings));
        return Task.CompletedTask;
    }

    [EffectMethod]
    public async Task HandleLoadAsync(LoadSettingsAction action, IDispatcher dispatcher)
    {
        try
        {
            var settings = await settingsStore.LoadAsync();
            dispatcher.Dispatch(new SettingsLoadedAction(settings));
        }
        catch (Exception ex)
        {
            LogLoadFailed(logger, ex);
            dispatcher.Dispatch(new SettingsLoadFailedAction("Unable to load settings."));
        }
    }

    [EffectMethod]
    public async Task HandleSaveAsync(SaveSettingsAction action, IDispatcher dispatcher)
    {
        try
        {
            await settingsStore.SaveAsync(action.Settings);
            dispatcher.Dispatch(new SettingsSavedAction());
        }
        catch (Exception ex)
        {
            LogSaveFailed(logger, ex);
            dispatcher.Dispatch(new SettingsSaveFailedAction("Unable to save settings."));
        }
    }

    [LoggerMessage(Level = LogLevel.Error, Message = "Failed to load settings")]
    private static partial void LogLoadFailed(ILogger logger, Exception ex);

    [LoggerMessage(Level = LogLevel.Error, Message = "Failed to save settings")]
    private static partial void LogSaveFailed(ILogger logger, Exception ex);
}
