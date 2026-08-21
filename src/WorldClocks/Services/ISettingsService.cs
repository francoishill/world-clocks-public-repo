using WorldClocks.Models;

namespace WorldClocks.Services;

public interface ISettingsService
{
    /// <summary>Loads settings from disk. <c>IsFirstRun</c> is true when there was no usable settings file
    /// (missing, or unreadable/corrupt), in which case sensible defaults are returned instead.</summary>
    (AppSettings Settings, bool IsFirstRun) Load();

    void Save(AppSettings settings);
}
