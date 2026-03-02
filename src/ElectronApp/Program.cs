using System.Runtime.InteropServices;
using ElectronApp.Features.Desktop;
using ElectronApp.Features.Settings;
using ElectronApp.Features.Settings.Store;
using ElectronApp.Security;
using ElectronNET.API;
using ElectronNET.API.Entities;
using Fluxor;
using Microsoft.Extensions.Options;

var builder = WebApplication.CreateBuilder(args);

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
BrowserWindow? mainWindow = null;
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

// Restore saved window bounds or use config defaults
var settingsStore = app.Services.GetRequiredService<IAppSettingsStore>();
var userSettings = await settingsStore.LoadAsync();

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

if (userSettings.WindowBounds is { } savedBounds)
{
    windowOptions.X = savedBounds.X;
    windowOptions.Y = savedBounds.Y;
    windowOptions.Width = savedBounds.Width;
    windowOptions.Height = savedBounds.Height;
}
else
{
    windowOptions.Width = desktopOptions.Width;
    windowOptions.Height = desktopOptions.Height;
    windowOptions.Center = true;
}

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

// Persist window bounds to disk
CancellationTokenSource? boundsDebounce = null;
mainWindow.OnResize += ScheduleSaveBounds;
mainWindow.OnMove += ScheduleSaveBounds;

// Explicit Quit() is required for macOS where IsQuitOnWindowAllClosed has no effect
Electron.App.WindowAllClosed += () =>
{
    boundsDebounce?.Cancel();
    boundsDebounce?.Dispose();
    Electron.App.Quit();
    app.Lifetime.StopApplication();
};

await app.WaitForShutdownAsync();

#endregion

return;

void ScheduleSaveBounds()
{
    boundsDebounce?.Cancel();
    boundsDebounce?.Dispose();
    boundsDebounce = new CancellationTokenSource();
    _ = SaveBoundsAfterDelayAsync(mainWindow!, settingsStore, boundsDebounce.Token);
}

static async Task SaveBoundsAfterDelayAsync(
    BrowserWindow window,
    IAppSettingsStore store,
    CancellationToken cancellationToken
)
{
    try
    {
        await Task.Delay(1000, cancellationToken);
        var rect = await window.GetBoundsAsync();
        var current = await store.LoadAsync(cancellationToken);
        await store.SaveAsync(
            current with
            {
                WindowBounds = new WindowBounds(rect.X, rect.Y, rect.Width, rect.Height),
            },
            cancellationToken
        );
    }
    catch (OperationCanceledException)
    {
        // Debounce canceled
    }
}

static async Task BringWindowToFrontAsync(BrowserWindow window)
{
    if (await window.IsMinimizedAsync())
        window.Restore();

    if (!await window.IsVisibleAsync())
        window.Show();

    window.Focus();
}
