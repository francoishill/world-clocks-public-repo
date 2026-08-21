using System.Windows;
using System.Windows.Controls;
using WorldClocks.ViewModels;

namespace WorldClocks.Views;

public partial class SettingsWindow : Window
{
    public SettingsWindow(SettingsViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;

        using var icon = IconRenderer.CreateIcon(pixelSize: 32);
        Icon = IconRenderer.ToBitmapSource(icon);
    }

    private void Save_Click(object sender, RoutedEventArgs e) => DialogResult = true;

    private void Cancel_Click(object sender, RoutedEventArgs e) => DialogResult = false;

    /// <summary>The timezone ComboBox is editable so it can be searched; if the user types a
    /// search fragment and clicks away without picking a result, snap the displayed text back to
    /// whatever is actually selected rather than leaving a stale, unselected fragment showing.</summary>
    private void TimeZoneComboBox_LostFocus(object sender, RoutedEventArgs e)
    {
        if (sender is ComboBox { DataContext: ClockEntryEditViewModel row } comboBox)
        {
            comboBox.Text = row.SelectedTimeZone.DisplayName;
        }
    }
}
