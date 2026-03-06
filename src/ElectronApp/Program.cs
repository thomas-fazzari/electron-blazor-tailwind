using System.Runtime.InteropServices;
using ElectronApp.Features.Desktop;
using ElectronApp.Features.Settings;
using ElectronApp.Logging;
using ElectronApp.Security;
using ElectronNET.API;
using ElectronNET.API.Entities;
using Fluxor;
using Microsoft.Extensions.Options;
using Serilog;

// Bootstrap logger to capture fatal startup errors before the host is built
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console(formatProvider: System.Globalization.CultureInfo.InvariantCulture)
    .CreateLogger();

BrowserWindow? mainWindow = null;

try
{
    var builder = WebApplication.CreateBuilder(args);

    builder.Host.AddFileLogging();

    if (builder.Environment.IsDevelopment())
    {
        builder.Host.UseDefaultServiceProvider(options =>
        {
            options.ValidateScopes = true;
            options.ValidateOnBuild = true;
        });
    }

    builder.WebHost.UseElectron(args);
    builder.Services.AddElectron();
    builder.Services.AddHttpContextAccessor();
    builder.Services.AddProblemDetails();
    builder.Services.AddRazorComponents().AddInteractiveServerComponents();
    builder.Services.AddFluxor(o => o.ScanAssemblies(typeof(Program).Assembly));

    builder
        .Services.AddOptions<ExternalLinksOptions>()
        .Bind(builder.Configuration.GetSection(ExternalLinksOptions.SectionName))
        .ValidateOnStart();
    builder.Services.AddSingleton<
        IValidateOptions<ExternalLinksOptions>,
        ExternalLinksOptionsValidator
    >();
    builder.Services.AddScoped<IExternalLinkService, ExternalLinkService>();

    builder.Services.AddSettingsFeature(builder.Configuration);
    builder.Services.AddDesktopFeature();

    var app = builder.Build();

    if (!app.Environment.IsDevelopment())
    {
        app.UseExceptionHandler("/error");
        app.UseHsts();
    }

    app.UseAppSecurityHeaders();

    app.UseStaticFiles();
    app.UseAntiforgery();
    app.MapStaticAssets();
    app.MapGet(
        "/error",
        () =>
            Results.Problem(
                title: "An unexpected error occurred.",
                statusCode: StatusCodes.Status500InternalServerError
            )
    );
    app.MapRazorComponents<ElectronApp.Components.App>().AddInteractiveServerRenderMode();

    // Run as a standard web app when Electron is not active
    if (!HybridSupport.IsElectronActive)
    {
        await app.RunAsync();
        return;
    }

    // Replace with the following to restrict the app to Electron only (no browser access):
    //
    // if (!HybridSupport.IsElectronActive)
    //     throw new InvalidOperationException("This application requires Electron to run. Use 'make run' instead.");

    #region Electron bootstrap

    await app.StartAsync();

    var desktopOptions = app.Services.GetRequiredService<IOptions<DesktopBootstrapOptions>>().Value;

    // Prevent multiple instances, focus the existing window when a second launch is attempted
    if (desktopOptions.RequireSingleInstance)
    {
        var hasSingleInstanceLock = await Electron.App.RequestSingleInstanceLockAsync(
            (_, _) =>
            {
                if (mainWindow is not null)
                    _ = BringWindowToFrontAsync(mainWindow);
            }
        );

        if (!hasSingleInstanceLock)
        {
            app.Lifetime.StopApplication();
            return;
        }
    }

    Electron.WindowManager.IsQuitOnWindowAllClosed = true;

    var windowBoundsCoordinator = app.Services.GetRequiredService<IWindowBoundsCoordinator>();

    var windowOptions = new BrowserWindowOptions
    {
        Title = desktopOptions.Title,
        MinWidth = desktopOptions.MinWidth,
        MinHeight = desktopOptions.MinHeight,
        Show = false,
        WebPreferences = new WebPreferences
        {
            DevTools = app.Environment.IsDevelopment(),
            NodeIntegration = false,
            NodeIntegrationInWorker = false,
            ContextIsolation = true,
            Sandbox = true,
            WebSecurity = true,
            AllowRunningInsecureContent = false,
            WebviewTag = false,
            EnableRemoteModule = false,
            Plugins = false,
            ExperimentalFeatures = false,
            Javascript = true,
        },
    };

    await windowBoundsCoordinator.ApplyInitialBoundsAsync(windowOptions, desktopOptions);

    // Custom titlebar OS detection
    windowOptions.TitleBarStyle = RuntimeInformation.IsOSPlatform(OSPlatform.OSX)
        ? TitleBarStyle.hiddenInset
        : TitleBarStyle.hidden;
    windowOptions.Fullscreenable = true;

    mainWindow = await Electron.WindowManager.CreateWindowAsync(windowOptions);

    mainWindow.OnReadyToShow += () => mainWindow.Show();

    // Register window maximize/unmaximize events for titlebar icon
    var desktopBridge = (ElectronDesktopBridge)app.Services.GetRequiredService<IDesktopBridge>();
    desktopBridge.RegisterWindowEvents();

    windowBoundsCoordinator.RegisterPersistence(mainWindow);

    // Explicit Quit() is required for macOS where IsQuitOnWindowAllClosed has no effect
    Electron.App.WindowAllClosed += () =>
    {
        windowBoundsCoordinator.Stop();
        Electron.App.Quit();
        app.Lifetime.StopApplication();
    };

    await app.WaitForShutdownAsync();

    #endregion
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
}
finally
{
    await Log.CloseAndFlushAsync();
}

return;

static async Task BringWindowToFrontAsync(BrowserWindow window)
{
    if (await window.IsMinimizedAsync())
        window.Restore();

    if (!await window.IsVisibleAsync())
        window.Show();

    window.Focus();
}
