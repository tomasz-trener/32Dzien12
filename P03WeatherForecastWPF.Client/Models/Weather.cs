namespace P03WeatherForecastWPF.Client.Models;

/// <summary>
/// Model "domenowy" pogody – to, co pokazujemy użytkownikowi.
/// Jest niezależny od formatu JSON konkretnego dostawcy (Open-Meteo),
/// dzięki czemu zmiana dostawcy nie zmienia interfejsu.
/// </summary>
public class Weather
{
    public DateTime Time { get; set; }

    public double Temperature { get; set; }

    public double WindSpeed { get; set; }

    public int WeatherCode { get; set; }

    /// <summary>Opis słowny na podstawie kodu WMO.</summary>
    public string Description => WeatherCode switch
    {
        0 => "bezchmurnie",
        1 or 2 or 3 => "częściowe zachmurzenie",
        45 or 48 => "mgła",
        51 or 53 or 55 => "mżawka",
        61 or 63 or 65 => "deszcz",
        71 or 73 or 75 => "śnieg",
        80 or 81 or 82 => "przelotne opady",
        95 or 96 or 99 => "burza",
        _ => $"kod WMO {WeatherCode}"
    };
}
