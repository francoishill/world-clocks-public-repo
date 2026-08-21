using System.Threading;
using System.Windows;
using System.Windows.Threading;
using H.NotifyIcon;
using WorldClocks.Services;
using WorldClocks.ViewModels;
using WorldClocks.Views;

namespace WorldClocks;

public partial class App : Application
{
    // Fixed GUID: identifies "another copy of this app", not any particular run.
    private const string SingleInstanceMutexName = @"Global\WorldClocks-9F0E5F8E-6E1B-4C9E-9C0A-3C6C9E9F5B21";

    private Mutex? _singleInstanceMutex;
    private TaskbarIcon? _trayIcon;
    private TrayPopupViewModel? _trayPopupViewModel;
    private SettingsWindow? _settingsWindow;
    private ISettingsService? _settingsService;
    private IAutoStartService? _autoStartService;

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        _singleInstanceMutex = new Mutex(initiallyOwned: true, SingleInstanceMutexName, out var createdNew);
        if (!createdNew)
        {
            Shutdown();
            return;
        }

        // No MainWindow: keep the app alive until Exit is chosen from the tray menu.
        ShutdownMode = ShutdownMode.OnExplicitShutdown;
        DispatcherUnhandledException += OnDispatcherUnhandledException;

        _settingsService = new SettingsService();
        _autoStartService = new AutoStartService();

        _trayPopupViewModel = new TrayPopupViewModel(openSettings: OpenSettings, exit: ExitApplication);

        _trayIcon = (TaskbarIcon)FindResource("TrayIcon");
        _trayIcon.DataContext = _trayPopupViewModel;
        _trayIcon.Icon = IconRenderer.CreateIcon(pixelSize: 32);
        _trayIcon.ForceCreate();

        var (settings, isFirstRun) = _settingsService.Load();
        _trayPopupViewModel.ReloadFromSettings(settings);

        if (isFirstRun)
        {
            OpenSettings();
        }
    }

    private void OpenSettings()
    {
        if (_settingsWindow is not null)
        {
            _settingsWindow.Activate();
            return;
        }

        var (settings, _) = _settingsService!.Load();
        settings.AutoStartEnabled = _autoStartService!.IsEnabled();

        var availableTimeZones = TimeZoneInfo.GetSystemTimeZones()
            .OrderBy(tz => tz.BaseUtcOffset)
            .ThenBy(tz => tz.DisplayName)
            .ToList();

        var settingsViewModel = new SettingsViewModel(settings, availableTimeZones);
        _settingsWindow = new SettingsWindow(settingsViewModel);

        var saved = _settingsWindow.ShowDialog();
        _settingsWindow = null;

        if (saved == true)
        {
            var newSettings = settingsViewModel.ToAppSettings();
            _settingsService.Save(newSettings);
            _autoStartService.SetEnabled(newSettings.AutoStartEnabled);
            _trayPopupViewModel!.ReloadFromSettings(newSettings);
        }
    }

    private void ExitApplication() => Shutdown();

    protected override void OnExit(ExitEventArgs e)
    {
        _trayIcon?.Dispose();
        _trayPopupViewModel?.Dispose();

        if (_singleInstanceMutex is not null)
        {
            try
            {
                _singleInstanceMutex.ReleaseMutex();
            }
            catch (ApplicationException)
            {
                // Mutex was never acquired by this thread (e.g. we exited before OnStartup finished) — nothing to release.
            }

            _singleInstanceMutex.Dispose();
        }

        base.OnExit(e);
    }

    private static void OnDispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
    {
        MessageBox.Show(
            $"World Clocks ran into an unexpected error:\n\n{e.Exception.Message}",
            "World Clocks",
            MessageBoxButton.OK,
            MessageBoxImage.Error);
        e.Handled = true;
    }
}
