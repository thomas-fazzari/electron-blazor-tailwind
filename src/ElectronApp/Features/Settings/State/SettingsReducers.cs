using Fluxor;

namespace ElectronApp.Features.Settings.State;

public static class SettingsReducers
{
    [ReducerMethod]
    public static SettingsState ReduceLoadSettings(SettingsState state, LoadSettingsAction _) =>
        state with
        {
            IsLoading = true,
            ErrorMessage = null,
        };

    [ReducerMethod]
    public static SettingsState ReduceSettingsLoaded(
        SettingsState state,
        SettingsLoadedAction action
    ) =>
        state with
        {
            IsLoading = false,
            IsLoaded = true,
            Settings = action.Settings,
            ErrorMessage = null,
        };

    [ReducerMethod]
    public static SettingsState ReduceSettingsLoadFailed(
        SettingsState state,
        SettingsLoadFailedAction action
    ) => state with { IsLoading = false, IsLoaded = false, ErrorMessage = action.ErrorMessage };

    [ReducerMethod]
    public static SettingsState ReduceSetTheme(SettingsState state, SetThemeAction action) =>
        state with
        {
            Settings = state.Settings with { Theme = action.Theme },
            ErrorMessage = null,
        };

    [ReducerMethod]
    public static SettingsState ReduceSaveSettings(SettingsState state, SaveSettingsAction _) =>
        state with
        {
            IsSaving = true,
            ErrorMessage = null,
        };

    [ReducerMethod]
    public static SettingsState ReduceSettingsSaved(SettingsState state, SettingsSavedAction _) =>
        state with
        {
            IsSaving = false,
            ErrorMessage = null,
        };

    [ReducerMethod]
    public static SettingsState ReduceSettingsSaveFailed(
        SettingsState state,
        SettingsSaveFailedAction action
    ) => state with { IsSaving = false, ErrorMessage = action.ErrorMessage };
}
