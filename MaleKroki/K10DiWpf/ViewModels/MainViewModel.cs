// K10DiWpf/ViewModels/MainViewModel.cs
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using K10DiWpf.Services;
using Microsoft.Extensions.Options;

namespace K10DiWpf.ViewModels;

/// <summary>
/// ViewModel dostaje swoje zależności konstruktorem: zegar (interfejs)
/// i ustawienia z appsettings.json (IOptions). Nie tworzy ich sam – robi to kontener.
/// </summary>
public partial class MainViewModel : ObservableObject
{
    private readonly IClock _clock;
    private readonly GreetingOptions _options;

    public MainViewModel(IClock clock, IOptions<GreetingOptions> options)
    {
        _clock = clock;
        _options = options.Value;   // IOptions<T>.Value to gotowy obiekt z wartościami z JSON

        Refresh();
    }

    [ObservableProperty]
    public partial string Message { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string ClockInfo { get; set; } = string.Empty;

    [RelayCommand]
    private void Refresh()
    {
        DateTime now = _clock.Now;

        string timeOfDay = _options.Language == "en"
            ? (now.Hour < 12 ? "Good morning" : "Good evening")
            : (now.Hour < 12 ? "Dzień dobry" : "Dobry wieczór");

        Message = $"{timeOfDay}, {_options.UserName}!";
        ClockInfo = $"Zegar: {_clock.GetType().Name}, czas: {now:HH:mm:ss}";
    }
}
