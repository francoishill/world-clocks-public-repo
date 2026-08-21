using System.IO;
using System.Text.Json;
using WorldClocks.Models;

namespace WorldClocks.Services;

public sealed class SettingsService : ISettingsService
{
    private static readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = true };

    private readonly string _filePath;

    public SettingsService()
        : this(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "WorldClocks", "settings.json"))
    {
    }

    /// <summary>Allows tests to point at a temp file instead of the real %AppData% location.</summary>
    public SettingsService(string filePath)
    {
        _filePath = filePath;
    }

    public (AppSettings Settings, bool IsFirstRun) Load()
    {
        if (!File.Exists(_filePath))
        {
            return (AppSettings.CreateDefault(), true);
        }

        try
        {
            var json = File.ReadAllText(_filePath);
            var settings = JsonSerializer.Deserialize<AppSettings>(json, JsonOptions);
            return settings is null ? (AppSettings.CreateDefault(), true) : (settings, false);
        }
        catch (Exception ex) when (ex is JsonException or IOException or UnauthorizedAccessException)
        {
            return (AppSettings.CreateDefault(), true);
        }
    }

    public void Save(AppSettings settings)
    {
        var directory = Path.GetDirectoryName(_filePath)!;
        Directory.CreateDirectory(directory);

        // Write-to-temp-then-replace keeps settings.json from being left truncated/corrupt
        // if the process is killed mid-write.
        var tempFilePath = _filePath + ".tmp";
        File.WriteAllText(tempFilePath, JsonSerializer.Serialize(settings, JsonOptions));
        File.Move(tempFilePath, _filePath, overwrite: true);
    }
}
