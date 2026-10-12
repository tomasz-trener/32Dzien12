namespace P04Delegaty;

/// <summary>
/// Własny typ delegata: każda metoda "double → double" pasuje do tego typu.
/// </summary>
public delegate double TemperatureConverter(double celsius);

public static class Converters
{
    public static double CelsiusToFahrenheit(double celsius) => celsius * 1.8 + 32;

    /// <summary>
    /// Metoda wyższego rzędu: przyjmuje delegat jako parametr.
    /// Nie wie, jak przeliczać – dostaje "przepis" z zewnątrz.
    /// </summary>
    public static void PrintConverted(IEnumerable<double> values, TemperatureConverter converter)
    {
        foreach (double value in values)
        {
            Console.WriteLine($"  {value} °C → {converter(value):F1}");
        }
    }
}
