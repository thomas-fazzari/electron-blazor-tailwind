using Microsoft.Extensions.Options;

namespace ElectronApp.Features.Settings.Store;

public sealed class AppSettingsStoreOptions
{
    public const string SectionName = "SettingsStore";

    public string DirectoryPath { get; set; } = string.Empty;
    public string FileName { get; init; } = "settings.json";
}

public sealed class AppSettingsStoreOptionsValidator : IValidateOptions<AppSettingsStoreOptions>
{
    public ValidateOptionsResult Validate(string? name, AppSettingsStoreOptions options)
    {
        var failures = new List<string>();

        if (string.IsNullOrWhiteSpace(options.DirectoryPath))
            failures.Add("SettingsStore:DirectoryPath must not be empty.");

        if (string.IsNullOrWhiteSpace(options.FileName))
            failures.Add("SettingsStore:FileName must not be empty.");

        if (options.FileName.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0)
            failures.Add("SettingsStore:FileName contains invalid file name characters.");

        return failures.Count > 0
            ? ValidateOptionsResult.Fail(failures)
            : ValidateOptionsResult.Success;
    }
}
