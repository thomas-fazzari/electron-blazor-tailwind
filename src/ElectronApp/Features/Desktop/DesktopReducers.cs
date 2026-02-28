using Fluxor;

namespace ElectronApp.Features.Desktop;

public static class DesktopReducers
{
    [ReducerMethod]
    public static DesktopState ReduceInitialize(DesktopState state, InitializeDesktopAction _) =>
        state with
        {
            IsInitializing = true,
            ErrorMessage = null,
        };

    [ReducerMethod]
    public static DesktopState ReduceInitialized(
        DesktopState state,
        DesktopInitializedAction action
    ) =>
        state with
        {
            IsInitializing = false,
            IsInitialized = true,
            IsElectronActive = action.Environment.IsElectronActive,
            Platform = action.Environment.Platform,
            DotnetVersion = action.Environment.DotnetVersion,
            ErrorMessage = null,
        };

    [ReducerMethod]
    public static DesktopState ReduceInitializationFailed(
        DesktopState state,
        DesktopInitializationFailedAction action
    ) =>
        state with
        {
            IsInitializing = false,
            IsInitialized = false,
            ErrorMessage = action.ErrorMessage,
        };
}
