using ElectronApp.Features.Settings.Store;
using Microsoft.Extensions.Options;

namespace ElectronApp.Features.Settings;

public static class SettingsServiceExtensions
{
    public static IServiceCollection AddSettingsFeature(
        this IServiceCollection services,
        IConfiguration configuration
    )
    {
        services
            .AddOptions<AppSettingsStoreOptions>()
            .Bind(configuration.GetSection(AppSettingsStoreOptions.SectionName))
            .PostConfigure(options =>
            {
                if (string.IsNullOrWhiteSpace(options.DirectoryPath))
                    options.DirectoryPath = Path.Combine(
                        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                        "ElectronApp"
                    );
            })
            .ValidateOnStart();

        services.AddSingleton<
            IValidateOptions<AppSettingsStoreOptions>,
            AppSettingsStoreOptionsValidator
        >();
        services.AddSingleton<IAppSettingsStore, JsonAppSettingsStore>();

        return services;
    }
}
