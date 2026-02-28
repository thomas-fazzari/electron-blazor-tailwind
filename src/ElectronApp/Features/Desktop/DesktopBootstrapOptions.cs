using System.ComponentModel.DataAnnotations;
using Microsoft.Extensions.Options;

namespace ElectronApp.Features.Desktop;

/// <summary>
/// Default window dimensions and single-instance settings
/// </summary>
public sealed class DesktopBootstrapOptions
{
    public const string SectionName = "Desktop";

    [Required]
    public string Title { get; set; } = "ElectronApp";

    [Range(1, int.MaxValue)]
    public int Width { get; set; } = 1280;

    [Range(1, int.MaxValue)]
    public int Height { get; set; } = 800;

    [Range(1, int.MaxValue)]
    public int MinWidth { get; set; } = 800;

    [Range(1, int.MaxValue)]
    public int MinHeight { get; set; } = 600;

    public bool RequireSingleInstance { get; set; } = true;
}

public sealed class DesktopBootstrapOptionsValidator : IValidateOptions<DesktopBootstrapOptions>
{
    public ValidateOptionsResult Validate(string? name, DesktopBootstrapOptions options)
    {
        var failures = new List<string>();

        if (options.MinWidth > options.Width)
            failures.Add("MinWidth must be less than or equal to Width.");

        if (options.MinHeight > options.Height)
            failures.Add("MinHeight must be less than or equal to Height.");

        return failures.Count > 0
            ? ValidateOptionsResult.Fail(failures)
            : ValidateOptionsResult.Success;
    }
}
