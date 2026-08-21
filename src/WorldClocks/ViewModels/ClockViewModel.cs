using System.Globalization;
using CommunityToolkit.Mvvm.ComponentModel;
using WorldClocks.Models;

namespace WorldClocks.ViewModels;

/// <summary>Live display state for a single configured clock, refreshed once per second.</summary>
public sealed partial class ClockViewModel : ObservableObject
{
    private readonly TimeZoneInfo? _timeZone;

    public ClockViewModel(ClockEntry entry)
    {
        Id = entry.Id;

        try
        {
            _timeZone = TimeZoneInfo.FindSystemTimeZoneById(entry.TimeZoneId);
        }
        catch (Exception ex) when (ex is TimeZoneNotFoundException or InvalidTimeZoneException)
        {
            _timeZone = null;
        }

        var fallbackTitle = _timeZone?.DisplayName ?? entry.TimeZoneId;
        _title = string.IsNullOrWhiteSpace(entry.Title) ? fallbackTitle : entry.Title;
        _subtitle = entry.Subtitle;

        if (_timeZone is null)
        {
            _isUnavailable = true;
            _timeDisplay = "Unavailable";
        }
    }

    public Guid Id { get; }

    [ObservableProperty]
    private string _title;

    [ObservableProperty]
    private string? _subtitle;

    [ObservableProperty]
    private string _timeDisplay = "--:--:--";

    [ObservableProperty]
    private string? _dateBadge;

    [ObservableProperty]
    private bool _isUnavailable;

    public void UpdateNow(DateTimeOffset utcNow, DateTime observerLocalDate, bool use24HourTime)
    {
        if (_timeZone is null)
        {
            return;
        }

        (TimeDisplay, DateBadge) = ComputeDisplay(_timeZone, utcNow, observerLocalDate, use24HourTime);
    }

    /// <summary>Pure computation, deliberately taking every time input as a parameter (never reading
    /// DateTime.Now/TimeZoneInfo.Local) so it is deterministic and unit-testable.</summary>
    internal static (string TimeText, string? DateBadge) ComputeDisplay(
        TimeZoneInfo timeZone, DateTimeOffset utcNow, DateTime observerLocalDate, bool use24HourTime)
    {
        var zonedNow = TimeZoneInfo.ConvertTime(utcNow, timeZone);
        var timeText = zonedNow.ToString(use24HourTime ? "HH:mm:ss" : "h:mm:ss tt", CultureInfo.CurrentCulture);

        var dayDiff = (zonedNow.Date - observerLocalDate.Date).Days;
        string? dateBadge = dayDiff switch
        {
            < 0 => "Yesterday",
            > 0 => "Tomorrow",
            _ => null,
        };

        return (timeText, dateBadge);
    }
}
