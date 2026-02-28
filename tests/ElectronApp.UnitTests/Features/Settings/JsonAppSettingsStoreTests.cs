using ElectronApp.Features.Settings.Store;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

namespace ElectronApp.UnitTests.Features.Settings;

public sealed class JsonAppSettingsStoreTests : IDisposable
{
    private readonly string _testDirectory = Path.Combine(
        Path.GetTempPath(),
        "electron-app-tests",
        Guid.NewGuid().ToString("N")
    );

    [Fact]
    public async Task LoadAsync_WhenSettingsFileDoesNotExist_ShouldReturnDefault()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var sut = BuildSut();

        var settings = await sut.LoadAsync(cancellationToken);

        Assert.Equal(UserSettings.Default, settings);
    }

    [Fact]
    public async Task SaveAsync_ThenLoadAsync_ShouldRoundTripSettings()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var sut = BuildSut();
        var expected = UserSettings.Default with { Theme = AppTheme.Light };

        await sut.SaveAsync(expected, cancellationToken);
        var loaded = await sut.LoadAsync(cancellationToken);

        Assert.Equal(expected, loaded);
    }

    [Fact]
    public async Task LoadAsync_WithCorruptedJson_ShouldRecoverAndReturnDefaults()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        Directory.CreateDirectory(_testDirectory);
        var settingsPath = Path.Combine(_testDirectory, "settings.json");
        await File.WriteAllTextAsync(settingsPath, "{ not-json }", cancellationToken);
        var sut = BuildSut();

        var settings = await sut.LoadAsync(cancellationToken);

        Assert.Equal(UserSettings.Default, settings);
        Assert.NotEmpty(
            Directory.GetFiles(
                _testDirectory,
                "settings.json.corrupted.*",
                SearchOption.TopDirectoryOnly
            )
        );
    }

    public void Dispose()
    {
        if (Directory.Exists(_testDirectory))
            Directory.Delete(_testDirectory, recursive: true);
    }

    private JsonAppSettingsStore BuildSut()
    {
        var options = Options.Create(
            new AppSettingsStoreOptions
            {
                DirectoryPath = _testDirectory,
                FileName = "settings.json",
            }
        );

        return new JsonAppSettingsStore(options, NullLogger<JsonAppSettingsStore>.Instance);
    }
}
