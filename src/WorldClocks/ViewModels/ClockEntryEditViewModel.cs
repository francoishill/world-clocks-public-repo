using System.ComponentModel;
using System.Windows.Data;
using CommunityToolkit.Mvvm.ComponentModel;
using WorldClocks.Models;

namespace WorldClocks.ViewModels;

/// <summary>Editable state for one row in the Settings clock list.</summary>
public sealed partial class ClockEntryEditViewModel : ObservableObject
{
    private readonly ICollectionView _timeZones;

    public ClockEntryEditViewModel(ClockEntry? source, IReadOnlyList<TimeZoneInfo> availableTimeZones, TimeZoneInfo defaultTimeZone)
    {
        Id = source?.Id ?? Guid.NewGuid();
        _title = source?.Title ?? string.Empty;
        _subtitle = source?.Subtitle ?? string.Empty;

        TimeZoneInfo? resolved = null;
        if (source is not null)
        {
            try
            {
                resolved = TimeZoneInfo.FindSystemTimeZoneById(source.TimeZoneId);
            }
            catch (Exception ex) when (ex is TimeZoneNotFoundException or InvalidTimeZoneException)
            {
                resolved = null;
            }
        }

        _selectedTimeZone = resolved ?? defaultTimeZone;

        _timeZones = new ListCollectionView(availableTimeZones.ToList()) { Filter = MatchesSearchText };
    }

    public Guid Id { get; }

    /// <summary>Filtered by TimeZoneSearchText; bind the ComboBox's ItemsSource to this, not the raw list.</summary>
    public ICollectionView TimeZones => _timeZones;

    [ObservableProperty]
    private string _title;

    [ObservableProperty]
    private string _subtitle;

    [ObservableProperty]
    private TimeZoneInfo _selectedTimeZone;

    /// <summary>What the user has typed into the editable timezone ComboBox. Matches against the
    /// full display text (e.g. "(UTC-08:00) Pacific Time (US &amp; Canada)"), so both the offset and
    /// the place name are searchable from one box.</summary>
    [ObservableProperty]
    private string _timeZoneSearchText = string.Empty;

    partial void OnTimeZoneSearchTextChanged(string value) => _timeZones.Refresh();

    partial void OnSelectedTimeZoneChanged(TimeZoneInfo value) => _timeZones.Refresh();

    private bool MatchesSearchText(object candidate)
    {
        var timeZone = (TimeZoneInfo)candidate;

        // The current selection must never be filtered out: WPF's Selector clears SelectedItem
        // to null the instant it disappears from a bound, filtered ItemsSource, which would wipe
        // out the row's timezone the moment the user typed something that didn't match it.
        // Compared by Id, not reference: SelectedTimeZone starts out as TimeZoneInfo.Local, a
        // distinct instance from the TimeZoneInfo.GetSystemTimeZones() entries in this list, even
        // when it's the same zone.
        if (timeZone.Id == SelectedTimeZone.Id)
        {
            return true;
        }

        if (string.IsNullOrWhiteSpace(TimeZoneSearchText))
        {
            return true;
        }

        return timeZone.DisplayName.Contains(TimeZoneSearchText, StringComparison.OrdinalIgnoreCase);
    }

    public ClockEntry ToClockEntry() => new()
    {
        Id = Id,
        TimeZoneId = SelectedTimeZone.Id,
        Title = string.IsNullOrWhiteSpace(Title) ? null : Title.Trim(),
        Subtitle = string.IsNullOrWhiteSpace(Subtitle) ? null : Subtitle.Trim(),
    };
}
