using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Options;

namespace ElectronApp.Features.Settings.Store;

/// <summary>
/// JSON file-backed settings store with atomic writes and corruption recovery
/// </summary>
public sealed partial class JsonAppSettingsStore(
    IOptions<AppSettingsStoreOptions> options,
    ILogger<JsonAppSettingsStore> logger
) : IAppSettingsStore, IDisposable
{
    private static readonly JsonSerializerOptions _serializerOptions = new()
    {
        WriteIndented = true,
        Converters = { new JsonStringEnumConverter() },
    };

    private readonly AppSettingsStoreOptions _options = options.Value;
    private readonly SemaphoreSlim _gate = new(1, 1);

    public async Task<UserSettings> LoadAsync(CancellationToken cancellationToken = default)
    {
        await _gate.WaitAsync(cancellationToken);
        try
        {
            var path = GetSettingsPath();
            if (!File.Exists(path))
                return UserSettings.Default;

            await using var stream = File.OpenRead(path);
            var settings = await JsonSerializer.DeserializeAsync<UserSettings>(
                stream,
                _serializerOptions,
                cancellationToken
            );
            return settings ?? UserSettings.Default;
        }
        catch (JsonException ex)
        {
            BackupCorruptedSettingsFile();
            LogInvalidSettingsJsonRecovered(logger, ex);
            return UserSettings.Default;
        }
        finally
        {
            _gate.Release();
        }
    }

    public async Task SaveAsync(
        UserSettings settings,
        CancellationToken cancellationToken = default
    )
    {
        await _gate.WaitAsync(cancellationToken);
        try
        {
            Directory.CreateDirectory(_options.DirectoryPath);
            var destinationPath = GetSettingsPath();
            var tempPath = Path.Combine(
                _options.DirectoryPath,
                $"{_options.FileName}.{Guid.NewGuid():N}.tmp"
            );

            var json = JsonSerializer.Serialize(settings, _serializerOptions);
            await File.WriteAllTextAsync(tempPath, json, cancellationToken);

            try
            {
                File.Move(tempPath, destinationPath, true);
            }
            catch
            {
                TryDeleteFile(tempPath);
                throw;
            }
        }
        finally
        {
            _gate.Release();
        }
    }

    private string GetSettingsPath() => Path.Combine(_options.DirectoryPath, _options.FileName);

    private static void TryDeleteFile(string path)
    {
        try
        {
            File.Delete(path);
        }
        catch
        {
            // ignored
        }
    }

    private void BackupCorruptedSettingsFile()
    {
        var sourcePath = GetSettingsPath();
        if (!File.Exists(sourcePath))
            return;

        var backupPath = Path.Combine(
            _options.DirectoryPath,
            $"{_options.FileName}.corrupted.{DateTimeOffset.UtcNow:yyyyMMddHHmmss}"
        );
        File.Move(sourcePath, backupPath, true);
    }

    public void Dispose() => _gate.Dispose();

    [LoggerMessage(
        Level = LogLevel.Warning,
        Message = "Settings file was invalid JSON. A backup has been created and defaults loaded."
    )]
    private static partial void LogInvalidSettingsJsonRecovered(
        ILogger logger,
        Exception exception
    );
}
