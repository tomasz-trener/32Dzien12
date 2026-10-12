namespace P03WeatherForecastWPF.Client.Configuration;

/// <summary>
/// Klasa odpowiadająca sekcji "AppSettings" w appsettings.json.
/// Wzorzec Options: zamiast czytać configuration["AppSettings:DefaultLanguage"]
/// dostajemy silnie typowany obiekt przez IOptions&lt;AppSettings&gt;.
/// </summary>
public class AppSettings
{
    /// <summary>Nazwa sekcji w pliku konfiguracyjnym.</summary>
    public const string SectionName = "AppSettings";

    /// <summary>Język nazw miast zwracanych przez Open-Meteo (np. "pl", "en").</summary>
    public string DefaultLanguage { get; set; } = "en";
}
