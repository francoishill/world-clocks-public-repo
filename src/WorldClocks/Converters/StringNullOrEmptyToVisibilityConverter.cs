using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace WorldClocks.Converters;

/// <summary>Collapses an element when the bound string is null/empty, e.g. an optional subtitle or badge.</summary>
public sealed class StringNullOrEmptyToVisibilityConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        string.IsNullOrEmpty(value as string) ? Visibility.Collapsed : Visibility.Visible;

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        throw new NotSupportedException();
}
