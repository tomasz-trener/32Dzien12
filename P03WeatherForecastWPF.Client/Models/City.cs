using System.Text.Json.Serialization;

namespace P03WeatherForecastWPF.Client.Models;

/// <summary>
/// Jedno miasto z odpowiedzi Open-Meteo Geocoding API.
/// Nazwy w JSON są w stylu snake_case (country_code), a w C# PascalCase,
/// dlatego mapujemy je atrybutem [JsonPropertyName].
/// </summary>
public class City
{
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("latitude")]
    public double Latitude { get; set; }

    [JsonPropertyName("longitude")]
    public double Longitude { get; set; }

    [JsonPropertyName("country")]
    public string? Country { get; set; }

    [JsonPropertyName("country_code")]
    public string? CountryCode { get; set; }

    [JsonPropertyName("admin1")]
    public string? Admin1 { get; set; }

    [JsonPropertyName("timezone")]
    public string? Timezone { get; set; }

    [JsonPropertyName("population")]
    public int? Population { get; set; }

    /// <summary>Tekst do wyświetlenia na liście, np. "Warszawa, Województwo mazowieckie (Polska)".</summary>
    [JsonIgnore]
    public string FullName =>
        string.IsNullOrEmpty(Admin1) ? $"{Name} ({Country})" : $"{Name}, {Admin1} ({Country})";
}
