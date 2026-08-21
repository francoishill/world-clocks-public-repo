namespace WorldClocks.Models;

/// <summary>A single configured world clock: a timezone plus optional display labels.</summary>
public sealed class ClockEntry
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public string TimeZoneId { get; set; } = TimeZoneInfo.Local.Id;

    /// <summary>Optional area name, e.g. "Los Angeles". Falls back to the timezone name when blank.</summary>
    public string? Title { get; set; }

    /// <summary>Optional free text, e.g. colleague names who work in this zone.</summary>
    public string? Subtitle { get; set; }
}
