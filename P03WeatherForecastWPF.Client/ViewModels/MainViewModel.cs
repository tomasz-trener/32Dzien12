using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using P03WeatherForecastWPF.Client.Models;
using P03WeatherForecastWPF.Client.Services;

namespace P03WeatherForecastWPF.Client.ViewModels;

/// <summary>
/// ViewModel okna głównego w wersji z CommunityToolkit.Mvvm.
/// - ObservableObject daje INotifyPropertyChanged.
/// - [ObservableProperty] na właściwości partial generuje setter z powiadomieniem.
/// - [RelayCommand] na metodzie generuje właściwość XxxCommand (ICommand).
/// Klasa musi być partial, bo generator kodu dopisuje do niej drugą część.
/// </summary>
public partial class MainViewModel : ObservableObject
{
    private readonly IMeteoService _meteoService;

    public MainViewModel(IMeteoService meteoService)
    {
        _meteoService = meteoService;
    }

    /// <summary>Tekst z pola "Miasto". Zmiana odświeża CanExecute komendy LoadCities.</summary>
    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(LoadCitiesCommand))]
    public partial string CityName { get; set; } = "Warszawa";

    /// <summary>
    /// ObservableCollection powiadamia listę o dodaniu/usunięciu elementów.
    /// Tablica też by działała (podmieniamy całą), ale kolekcja jest standardem w MVVM.
    /// </summary>
    public ObservableCollection<City> Cities { get; } = [];

    [ObservableProperty]
    public partial City? SelectedCity { get; set; }

    [ObservableProperty]
    public partial Weather? Weather { get; set; }

    /// <summary>True podczas pobierania danych – blokuje przycisk i pokazuje pasek.</summary>
    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(LoadCitiesCommand))]
    public partial bool IsBusy { get; set; }

    [ObservableProperty]
    public partial string? ErrorMessage { get; set; }

    /// <summary>
    /// Generator tworzy LoadCitiesCommand typu IAsyncRelayCommand.
    /// CanExecute wskazuje metodę, która mówi, kiedy przycisk jest aktywny.
    /// </summary>
    [RelayCommand(CanExecute = nameof(CanLoadCities))]
    private async Task LoadCitiesAsync()
    {
        IsBusy = true;
        ErrorMessage = null;

        try
        {
            City[] cities = await _meteoService.GetLocationsAsync(CityName);

            Cities.Clear();
            foreach (City city in cities)
            {
                Cities.Add(city);
            }

            if (cities.Length == 0)
            {
                ErrorMessage = "Nie znaleziono miasta o takiej nazwie.";
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Błąd połączenia: {ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }

    private bool CanLoadCities() => !IsBusy && !string.IsNullOrWhiteSpace(CityName);

    /// <summary>
    /// Generator wywołuje tę metodę po każdej zmianie SelectedCity (hook "partial void On...Changed").
    /// Dzięki temu wybór miasta na liście od razu pobiera pogodę.
    /// </summary>
    partial void OnSelectedCityChanged(City? value)
    {
        _ = LoadWeatherAsync(value);
    }

    private async Task LoadWeatherAsync(City? city)
    {
        if (city is null)
        {
            Weather = null;
            return;
        }

        ErrorMessage = null;

        try
        {
            Weather = await _meteoService.GetCurrentWeatherAsync(city.Latitude, city.Longitude);

            if (Weather is null)
            {
                ErrorMessage = "Brak danych pogodowych dla tej lokalizacji.";
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Błąd połączenia: {ex.Message}";
        }
    }
}
