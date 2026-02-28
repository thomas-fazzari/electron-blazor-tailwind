using Microsoft.Extensions.Options;

namespace ElectronApp.Security;

public sealed class ExternalLinksOptionsValidator : IValidateOptions<ExternalLinksOptions>
{
    public ValidateOptionsResult Validate(string? name, ExternalLinksOptions options)
    {
        if (options.AllowedHosts.Length == 0)
        {
            return ValidateOptionsResult.Fail(
                "ExternalLinks:AllowedHosts must include at least one host."
            );
        }

        var invalidHosts = options
            .AllowedHosts.Where(host =>
                string.IsNullOrWhiteSpace(host)
                || Uri.CheckHostName(host) == UriHostNameType.Unknown
            )
            .ToArray();

        return invalidHosts.Length > 0
            ? ValidateOptionsResult.Fail(
                $"ExternalLinks:AllowedHosts has invalid hosts: {string.Join(", ", invalidHosts)}"
            )
            : ValidateOptionsResult.Success;
    }
}
