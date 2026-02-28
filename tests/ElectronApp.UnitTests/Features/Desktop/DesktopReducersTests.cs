using ElectronApp.Features.Desktop;

namespace ElectronApp.UnitTests.Features.Desktop;

public sealed class DesktopReducersTests
{
    [Fact]
    public void ReduceInitialize_ShouldSetIsInitializing()
    {
        var initial = new DesktopState();

        var updated = DesktopReducers.ReduceInitialize(initial, new InitializeDesktopAction());

        Assert.True(updated.IsInitializing);
        Assert.Null(updated.ErrorMessage);
    }

    [Fact]
    public void ReduceInitialized_ShouldApplyEnvironmentInfo()
    {
        var initial = new DesktopState { IsInitializing = true };
        var info = new DesktopEnvironmentInfo(true, "macOS", "10.0.0");

        var updated = DesktopReducers.ReduceInitialized(
            initial,
            new DesktopInitializedAction(info)
        );

        Assert.False(updated.IsInitializing);
        Assert.True(updated.IsInitialized);
        Assert.True(updated.IsElectronActive);
        Assert.Equal("macOS", updated.Platform);
        Assert.Equal("10.0.0", updated.DotnetVersion);
        Assert.Null(updated.ErrorMessage);
    }

    [Fact]
    public void ReduceInitializationFailed_ShouldSetErrorMessage()
    {
        var initial = new DesktopState { IsInitializing = true };

        var updated = DesktopReducers.ReduceInitializationFailed(
            initial,
            new DesktopInitializationFailedAction("Failed to initialize")
        );

        Assert.False(updated.IsInitializing);
        Assert.False(updated.IsInitialized);
        Assert.Equal("Failed to initialize", updated.ErrorMessage);
    }
}
