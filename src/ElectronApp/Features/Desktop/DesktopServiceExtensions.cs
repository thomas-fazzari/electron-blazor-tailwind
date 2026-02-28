using Microsoft.Extensions.Options;

namespace ElectronApp.Features.Desktop;

public static class DesktopServiceExtensions
{
    public static IServiceCollection AddDesktopFeature(this IServiceCollection services)
    {
        services
            .AddOptions<DesktopBootstrapOptions>()
            .BindConfiguration(DesktopBootstrapOptions.SectionName)
            .ValidateDataAnnotations()
            .ValidateOnStart();

        services.AddSingleton<
            IValidateOptions<DesktopBootstrapOptions>,
            DesktopBootstrapOptionsValidator
        >();
        services.AddSingleton<IDesktopBridge, ElectronDesktopBridge>();

        return services;
    }
}
