using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using P03WeatherForecastWPF.Client.Commands;
using P03WeatherForecastWPF.Client.Models;
using P03WeatherForecastWPF.Client.Services;

namespace P03WeatherForecastWPF.Client.ViewModels.Classic;

/// <summary>
/// ViewModel napisany "ręcznie": INotifyPropertyChanged, pola + właściwości,
/// własny RelayCommand. Pokazuje, co dzieje się pod spodem.
/// W aplikacji używamy wersji z CommunityToolkit.Mvvm (ViewModels/MainViewModel.cs),
/// która generuje ten sam kod automatycznie. Ten plik zostaje do porównania.
/// </summary>
public class MainViewModelClassic : INotifyPropertyChanged
{
    private readonly IMeteoService _meteoService;

    private string _cityName = "Warszawa";
    private City[] _cities = [];
    private City? _selectedCity;
    private Weather? _weather;
    private bool _isBusy;
    private string? _errorMessage;

    // Zdarzenie, przez które ViewModel mówi widokowi: "właściwość X się zmieniła, odśwież się"
    public event PropertyChangedEventHandler? PropertyChanged;

    // [CallerMemberName] – kompilator sam wstawia nazwę właściwości, z której wołamy OnPropertyChanged()
    protected void OnPropertyChanged([CallerMemberName] string? propertyName = null) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

    public string CityName
    {
        get => _cityName;
        set
        {
            _cityName = value;
            OnPropertyChanged();
        }
    }

    public City[] Cities
    {
        get => _cities;
        set
        {
            _cities = value;
            OnPropertyChanged();
        }
    }

    public City? SelectedCity
    {
        get => _selectedCity;
        set
        {
            _selectedCity = value;
            OnPropertyChanged();
            _ = LoadWeatherAsync();   // zmiana zaznaczenia uruchamia pobranie pogody
        }
    }

    public Weather? Weather
    {
        get => _weather;
        set
        {
            _weather = value;
            OnPropertyChanged();
        }
    }

    public bool IsBusy
    {
        get => _isBusy;
        set
        {
            _isBusy = value;
            OnPropertyChanged();
        }
    }

    public string? ErrorMessage
    {
        get => _errorMessage;
        set
        {
            _errorMessage = value;
            OnPropertyChanged();
        }
    }

    public ICommand LoadCitiesCommand { get; }

    public MainViewModelClassic(IMeteoService meteoService)
    {
        _meteoService = meteoService;

        // Komenda: co wykonać + kiedy przycisk ma być aktywny
        LoadCitiesCommand = new RelayCommand(
            execute: _ => _ = LoadCitiesAsync(),
            canExecute: _ => !IsBusy && !string.IsNullOrWhiteSpace(CityName));
    }

    private async Task LoadCitiesAsync()
    {
        IsBusy = true;
        ErrorMessage = null;

        try
        {
            Cities = await _meteoService.GetLocationsAsync(CityName);
            if (Cities.Length == 0)
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

    private async Task LoadWeatherAsync()
    {
        if (SelectedCity is null)
        {
            return;
        }

        try
        {
            Weather = await _meteoService.GetCurrentWeatherAsync(SelectedCity.Latitude, SelectedCity.Longitude);
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Błąd połączenia: {ex.Message}";
        }
    }
}
