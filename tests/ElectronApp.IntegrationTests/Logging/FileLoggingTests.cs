using ElectronApp.IntegrationTests.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace ElectronApp.IntegrationTests.Logging;

public sealed class FileLoggingTests(ElectronAppWebApplicationFactory factory)
    : IClassFixture<ElectronAppWebApplicationFactory>
{
    private static readonly Action<ILogger, string, Exception?> _logMarkerMessage =
        LoggerMessage.Define<string>(
            LogLevel.Information,
            new EventId(1000, nameof(Logger_ShouldWriteToConfiguredLocalAppDataFile)),
            "{Marker}"
        );

    [Fact]
    public async Task Logger_ShouldWriteToConfiguredLocalAppDataFile()
    {
        _ = factory.CreateClient();
        var logger = factory.Services.GetRequiredService<ILogger<FileLoggingTests>>();
        var marker = $"integration-log-marker-{Guid.NewGuid():N}";
        var appName = typeof(Program).Assembly.GetName().Name ?? "ElectronApp";
        var logDirectory = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            appName,
            "logs"
        );

        _logMarkerMessage(logger, marker, null);

        var found = await WaitForMarkerAsync(
            logDirectory,
            marker,
            TestContext.Current.CancellationToken
        );

        Assert.True(
            found,
            $"Expected to find marker '{marker}' in a log file under '{logDirectory}'."
        );
    }

    private static async Task<bool> WaitForMarkerAsync(
        string logDirectory,
        string marker,
        CancellationToken cancellationToken
    )
    {
        var deadline = DateTime.UtcNow.AddSeconds(5);

        while (DateTime.UtcNow < deadline)
        {
            if (Directory.Exists(logDirectory))
            {
                foreach (var file in Directory.GetFiles(logDirectory, "log-*.txt"))
                {
                    var content = await File.ReadAllTextAsync(file, cancellationToken);
                    if (content.Contains(marker, StringComparison.Ordinal))
                        return true;
                }
            }

            await Task.Delay(200, cancellationToken);
        }

        return false;
    }
}
