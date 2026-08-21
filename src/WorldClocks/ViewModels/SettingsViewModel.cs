using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using WorldClocks.Models;

namespace WorldClocks.ViewModels;

/// <summary>Working copy of <see cref="AppSettings"/> being edited in the Settings window.
/// Nothing here touches disk or the live tray popup until <see cref="ToAppSettings"/> is saved by the caller.</summary>
public sealed partial class SettingsViewModel : ObservableObject
{
    public SettingsViewModel(AppSettings settings, IReadOnlyList<TimeZoneInfo> availableTimeZones)
    {
        AvailableTimeZones = availableTimeZones;

        foreach (var entry in settings.Clocks)
        {
            Rows.Add(new ClockEntryEditViewModel(entry, availableTimeZones, TimeZoneInfo.Local));
        }

        _autoStartEnabled = settings.AutoStartEnabled;
        _use24HourTime = settings.Use24HourTime;
    }

    public ObservableCollection<ClockEntryEditViewModel> Rows { get; } = new();

    public IReadOnlyList<TimeZoneInfo> AvailableTimeZones { get; }

    [ObservableProperty]
    private ClockEntryEditViewModel? _selectedRow;

    [ObservableProperty]
    private bool _autoStartEnabled;

    [ObservableProperty]
    private bool _use24HourTime;

    [RelayCommand]
    private void AddClock()
    {
        var row = new ClockEntryEditViewModel(null, AvailableTimeZones, TimeZoneInfo.Local);
        Rows.Add(row);
        SelectedRow = row;
    }

    [RelayCommand(CanExecute = nameof(HasSelectedRow))]
    private void RemoveClock()
    {
        if (SelectedRow is null)
        {
            return;
        }

        var index = Rows.IndexOf(SelectedRow);
        Rows.Remove(SelectedRow);
        SelectedRow = Rows.Count == 0 ? null : Rows[Math.Min(index, Rows.Count - 1)];
    }

    [RelayCommand(CanExecute = nameof(CanMoveUp))]
    private void MoveUp()
    {
        if (SelectedRow is null)
        {
            return;
        }

        var index = Rows.IndexOf(SelectedRow);
        Rows.Move(index, index - 1);
        RefreshRowCommands();
    }

    [RelayCommand(CanExecute = nameof(CanMoveDown))]
    private void MoveDown()
    {
        if (SelectedRow is null)
        {
            return;
        }

        var index = Rows.IndexOf(SelectedRow);
        Rows.Move(index, index + 1);
        RefreshRowCommands();
    }

    private bool HasSelectedRow => SelectedRow is not null;

    private bool CanMoveUp => SelectedRow is not null && Rows.IndexOf(SelectedRow) > 0;

    private bool CanMoveDown => SelectedRow is not null && Rows.IndexOf(SelectedRow) < Rows.Count - 1;

    partial void OnSelectedRowChanged(ClockEntryEditViewModel? value) => RefreshRowCommands();

    private void RefreshRowCommands()
    {
        RemoveClockCommand.NotifyCanExecuteChanged();
        MoveUpCommand.NotifyCanExecuteChanged();
        MoveDownCommand.NotifyCanExecuteChanged();
    }

    public AppSettings ToAppSettings() => new()
    {
        Clocks = Rows.Select(r => r.ToClockEntry()).ToList(),
        AutoStartEnabled = AutoStartEnabled,
        Use24HourTime = Use24HourTime,
    };
}
