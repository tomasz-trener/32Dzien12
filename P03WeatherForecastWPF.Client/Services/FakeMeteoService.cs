using P03WeatherForecastWPF.Client.Models;

namespace P03WeatherForecastWPF.Client.Services;

/// <summary>
/// Implementacja "na niby": zwraca stałe dane bez dostępu do internetu.
/// Przydaje się, gdy sieć nie działa, do pracy nad wyglądem okna i do testów.
/// Rejestrujemy ją w App.xaml.cs zamiast OpenMeteoService – ViewModel tego nie zauważy.
/// </summary>
public class FakeMeteoService : IMeteoService
{
    private static readonly City[] _cities =
    [
        new() { Id = 1, Name = "Warszawa", Country = "Polska", Admin1 = "Województwo mazowieckie", Latitude = 52.2297, Longitude = 21.0122 },
        new() { Id = 2, Name = "Kraków", Country = "Polska", Admin1 = "Województwo małopolskie", Latitude = 50.0647, Longitude = 19.9450 },
        new() { Id = 3, Name = "Gdańsk", Country = "Polska", Admin1 = "Województwo pomorskie", Latitude = 54.3520, Longitude = 18.6466 }
    ];

    public async Task<City[]> GetLocationsAsync(string locationName)
    {
        await Task.Delay(500); // symulacja opóźnienia sieci

        return _cities
            .Where(c => c.Name.Contains(locationName, StringComparison.OrdinalIgnoreCase))
            .ToArray();
    }

    public async Task<Weather?> GetCurrentWeatherAsync(double latitude, double longitude)
    {
        await Task.Delay(500);

        return new Weather
        {
            Time = DateTime.Now,
            Temperature = Math.Round(Random.Shared.NextDouble() * 30 - 5, 1),
            WindSpeed = Random.Shared.Next(0, 40),
            WeatherCode = Random.Shared.Next(0, 4)
        };
    }
}
