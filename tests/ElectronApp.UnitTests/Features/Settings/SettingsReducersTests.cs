using ElectronApp.Features.Settings.State;
using ElectronApp.Features.Settings.Store;

namespace ElectronApp.UnitTests.Features.Settings;

public sealed class SettingsReducersTests
{
    [Fact]
    public void ReduceLoadSettings_ShouldSetIsLoading()
    {
        var initial = new SettingsState();

        var updated = SettingsReducers.ReduceLoadSettings(initial, new LoadSettingsAction());

        Assert.True(updated.IsLoading);
        Assert.Null(updated.ErrorMessage);
    }

    [Fact]
    public void ReduceSettingsLoaded_ShouldApplySettings()
    {
        var initial = new SettingsState { IsLoading = true };
        var settings = new UserSettings { Theme = AppTheme.Light };

        var updated = SettingsReducers.ReduceSettingsLoaded(
            initial,
            new SettingsLoadedAction(settings)
        );

        Assert.False(updated.IsLoading);
        Assert.True(updated.IsLoaded);
        Assert.Equal(AppTheme.Light, updated.Settings.Theme);
        Assert.Null(updated.ErrorMessage);
    }

    [Fact]
    public void ReduceSettingsLoadFailed_ShouldSetErrorMessage()
    {
        var initial = new SettingsState { IsLoading = true };

        var updated = SettingsReducers.ReduceSettingsLoadFailed(
            initial,
            new SettingsLoadFailedAction("Load failed")
        );

        Assert.False(updated.IsLoading);
        Assert.False(updated.IsLoaded);
        Assert.Equal("Load failed", updated.ErrorMessage);
    }

    [Fact]
    public void ReduceSetTheme_ShouldUpdateTheme()
    {
        var initial = new SettingsState { Settings = new UserSettings { Theme = AppTheme.Dark } };

        var updated = SettingsReducers.ReduceSetTheme(initial, new SetThemeAction(AppTheme.System));

        Assert.Equal(AppTheme.System, updated.Settings.Theme);
        Assert.Null(updated.ErrorMessage);
    }

    [Fact]
    public void ReduceSaveSettings_ShouldSetIsSaving()
    {
        var initial = new SettingsState();
        var settings = new UserSettings { Theme = AppTheme.Light };

        var updated = SettingsReducers.ReduceSaveSettings(
            initial,
            new SaveSettingsAction(settings)
        );

        Assert.True(updated.IsSaving);
        Assert.Null(updated.ErrorMessage);
    }

    [Fact]
    public void ReduceSettingsSaved_ShouldClearIsSaving()
    {
        var initial = new SettingsState { IsSaving = true };

        var updated = SettingsReducers.ReduceSettingsSaved(initial, new SettingsSavedAction());

        Assert.False(updated.IsSaving);
        Assert.Null(updated.ErrorMessage);
    }

    [Fact]
    public void ReduceSettingsSaveFailed_ShouldSetErrorMessage()
    {
        var initial = new SettingsState { IsSaving = true };

        var updated = SettingsReducers.ReduceSettingsSaveFailed(
            initial,
            new SettingsSaveFailedAction("Save failed")
        );

        Assert.False(updated.IsSaving);
        Assert.Equal("Save failed", updated.ErrorMessage);
    }
}
