using P03WeatherForecastWPF.Client.Models;

namespace P03WeatherForecastWPF.Client.Services;

/// <summary>
/// Kontrakt serwisu pogodowego. ViewModel zna tylko ten interfejs,
/// więc możemy podmienić implementację (Open-Meteo, dane testowe, inny dostawca)
/// bez zmiany ViewModelu – to jest sedno Dependency Injection.
/// </summary>
public interface IMeteoService
{
    Task<City[]> GetLocationsAsync(string locationName);

    Task<Weather?> GetCurrentWeatherAsync(double latitude, double longitude);
}
