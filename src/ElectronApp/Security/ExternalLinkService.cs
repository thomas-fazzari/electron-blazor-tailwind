using ElectronApp.Features.Desktop;
using Microsoft.Extensions.Options;

namespace ElectronApp.Security;

public sealed partial class ExternalLinkService(
    IOptions<ExternalLinksOptions> options,
    IDesktopBridge desktopBridge,
    ILogger<ExternalLinkService> logger
) : IExternalLinkService
{
    private readonly ExternalLinksOptions _options = options.Value;

    private readonly HashSet<string> _allowedHosts = new(
        options.Value.AllowedHosts,
        StringComparer.OrdinalIgnoreCase
    );

    public async Task<bool> TryOpenAsync(string url, CancellationToken cancellationToken = default)
    {
        if (!Uri.TryCreate(url, UriKind.Absolute, out var uri))
        {
            LogBlockedMalformedExternalUrl(logger, url);
            return false;
        }

        if (_options.RequireHttps && uri.Scheme != Uri.UriSchemeHttps)
        {
            LogBlockedNonHttpsExternalUrl(logger, url);
            return false;
        }

        if (_allowedHosts.Contains(uri.Host))
            return await desktopBridge.OpenExternalAsync(uri, cancellationToken);

        LogBlockedExternalHostNotAllowlisted(logger, uri.Host);
        return false;
    }

    [LoggerMessage(Level = LogLevel.Warning, Message = "Blocked malformed external URL: {Url}")]
    private static partial void LogBlockedMalformedExternalUrl(ILogger logger, string url);

    [LoggerMessage(Level = LogLevel.Warning, Message = "Blocked non-HTTPS external URL: {Url}")]
    private static partial void LogBlockedNonHttpsExternalUrl(ILogger logger, string url);

    [LoggerMessage(
        Level = LogLevel.Warning,
        Message = "Blocked external URL host not in allowlist: {Host}"
    )]
    private static partial void LogBlockedExternalHostNotAllowlisted(ILogger logger, string host);
}
