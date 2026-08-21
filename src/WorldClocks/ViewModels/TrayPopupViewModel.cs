using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using WorldClocks.Models;

namespace WorldClocks.ViewModels;

/// <summary>Backs the tray flyout: the live clock list plus the tray context menu commands.</summary>
public sealed partial class TrayPopupViewModel : ObservableObject, IDisposable
{
    private readonly DispatcherTimer _timer;
    private readonly Action _openSettings;
    private readonly Action _exit;
    private bool _use24HourTime;

    public TrayPopupViewModel(Action openSettings, Action exit)
    {
        _openSettings = openSettings;
        _exit = exit;

        _timer = new DispatcherTimer(DispatcherPriority.Background)
        {
            Interval = TimeSpan.FromSeconds(1),
        };
        _timer.Tick += (_, _) => Tick();
        _timer.Start();
    }

    public ObservableCollection<ClockViewModel> Clocks { get; } = new();

    [ObservableProperty]
    private Visibility _emptyStateVisibility = Visibility.Visible;

    [RelayCommand]
    private void OpenSettings() => _openSettings();

    [RelayCommand]
    private void Exit() => _exit();

    /// <summary>Rebuilds the clock list from disk-backed settings. Called at startup and after Settings is saved.
    /// The list is small, so a full rebuild is simpler and cheap enough compared to diffing in place.</summary>
    public void ReloadFromSettings(AppSettings settings)
    {
        _use24HourTime = settings.Use24HourTime;

        Clocks.Clear();
        foreach (var entry in settings.Clocks)
        {
            Clocks.Add(new ClockViewModel(entry));
        }

        EmptyStateVisibility = Clocks.Count == 0 ? Visibility.Visible : Visibility.Collapsed;

        Tick();
    }

    private void Tick()
    {
        var utcNow = DateTimeOffset.UtcNow;
        var observerLocalDate = DateTime.Now.Date;

        foreach (var clock in Clocks)
        {
            clock.UpdateNow(utcNow, observerLocalDate, _use24HourTime);
        }
    }

    public void Dispose() => _timer.Stop();
}
