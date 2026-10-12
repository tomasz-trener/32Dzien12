using System.Globalization;
using System.Windows.Data;

namespace P03WeatherForecastWPF.Client.Converters;

/// <summary>
/// Konwerter bindingu: liczba (double) → tekst "12,5 °C".
/// ViewModel trzyma czystą liczbę, a formatowanie należy do widoku.
/// </summary>
public class TemperatureToDisplayConverter : IValueConverter
{
    /// <summary>Kierunek ViewModel → widok.</summary>
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is double temperature)
        {
            return $"{temperature.ToString("F1", culture)} °C";
        }

        return "—";
    }

    /// <summary>Kierunek widok → ViewModel. Nie używamy (binding jest jednokierunkowy).</summary>
    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        throw new NotSupportedException();
}
