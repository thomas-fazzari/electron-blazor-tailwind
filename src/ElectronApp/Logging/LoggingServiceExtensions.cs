using System.Globalization;
using Serilog;

namespace ElectronApp.Logging;

internal static class LoggingServiceExtensions
{
    private const string OutputTemplate =
        "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj}{NewLine}{Exception}";

    internal static IHostBuilder AddFileLogging(this IHostBuilder host)
    {
        var appName = typeof(LoggingServiceExtensions).Assembly.GetName().Name ?? "ElectronApp";
        var logPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            appName,
            "logs",
            "log-.txt"
        );

        return host.UseSerilog(
            (context, _, config) =>
                config
                    .ReadFrom.Configuration(context.Configuration)
                    .Enrich.FromLogContext()
                    .Enrich.WithProperty("Application", appName)
                    .Enrich.WithProperty("Environment", context.HostingEnvironment.EnvironmentName)
                    .WriteTo.File(
                        path: logPath,
                        rollingInterval: RollingInterval.Day,
                        retainedFileCountLimit: 7,
                        formatProvider: CultureInfo.InvariantCulture,
                        outputTemplate: OutputTemplate
                    )
        );
    }
}
