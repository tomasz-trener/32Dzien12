using System.Text.Json.Serialization;

namespace P03WeatherForecastWPF.Client.Models;

/// <summary>
/// Cała odpowiedź z https://geocoding-api.open-meteo.com/v1/search
/// Gdy nic nie znaleziono, pole "results" w ogóle nie występuje – stąd typ nullable.
/// </summary>
public class GeocodingResponse
{
    [JsonPropertyName("results")]
    public List<City>? Results { get; set; }

    [JsonPropertyName("generationtime_ms")]
    public double GenerationTimeMs { get; set; }
}
