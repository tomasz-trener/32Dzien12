using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace P03WeatherForecastWPF.Client.Converters;

/// <summary>
/// null lub pusty tekst → Collapsed, cokolwiek innego → Visible.
/// Używany do pokazywania komunikatu o błędzie tylko wtedy, gdy jakiś jest.
/// </summary>
public class NullToVisibilityConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        value is null || value is string { Length: 0 } ? Visibility.Collapsed : Visibility.Visible;

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        throw new NotSupportedException();
}
