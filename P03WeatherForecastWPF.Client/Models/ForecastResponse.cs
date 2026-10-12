using System.Text.Json.Serialization;

namespace P03WeatherForecastWPF.Client.Models;

/// <summary>
/// Odpowiedź z https://api.open-meteo.com/v1/forecast?current=temperature_2m,...
/// Mapujemy tylko pola, których używamy – reszta JSON-a jest ignorowana.
/// </summary>
public class ForecastResponse
{
    [JsonPropertyName("latitude")]
    public double Latitude { get; set; }

    [JsonPropertyName("longitude")]
    public double Longitude { get; set; }

    [JsonPropertyName("timezone")]
    public string? Timezone { get; set; }

    [JsonPropertyName("current_units")]
    public CurrentUnits? CurrentUnits { get; set; }

    [JsonPropertyName("current")]
    public CurrentWeather? Current { get; set; }
}

public class CurrentUnits
{
    [JsonPropertyName("temperature_2m")]
    public string? Temperature2m { get; set; }

    [JsonPropertyName("wind_speed_10m")]
    public string? WindSpeed10m { get; set; }
}

public class CurrentWeather
{
    [JsonPropertyName("time")]
    public string? Time { get; set; }

    [JsonPropertyName("temperature_2m")]
    public double Temperature2m { get; set; }

    [JsonPropertyName("wind_speed_10m")]
    public double WindSpeed10m { get; set; }

    /// <summary>Kod pogody WMO (0 = bezchmurnie, 61 = deszcz itd.).</summary>
    [JsonPropertyName("weather_code")]
    public int WeatherCode { get; set; }
}
