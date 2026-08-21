using System.Globalization;

namespace WorldClocks.Models;

/// <summary>The full persisted state of the app.</summary>
public sealed class AppSettings
{
    public List<ClockEntry> Clocks { get; set; } = new();

    public bool AutoStartEnabled { get; set; } = true;

    public bool Use24HourTime { get; set; } = DetermineDefaultUse24HourTime();

    /// <summary>Used when no settings file exists yet, or the existing one can't be read.</summary>
    public static AppSettings CreateDefault() => new();

    private static bool DetermineDefaultUse24HourTime() =>
        CultureInfo.CurrentCulture.DateTimeFormat.ShortTimePattern.Contains('H');
}
