using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using P03WeatherForecastWPF.Client.Configuration;
using P03WeatherForecastWPF.Client.Services;
using P03WeatherForecastWPF.Client.ViewModels;

namespace P03WeatherForecastWPF.Client;

/// <summary>
/// Punkt startowy aplikacji. Zamiast StartupUri budujemy Generic Host:
/// konfiguracja (appsettings.json, User Secrets, zmienne środowiskowe),
/// logowanie i kontener Dependency Injection w jednym miejscu –
/// dokładnie tak samo jak w ASP.NET Core (dzień 13).
/// </summary>
public partial class App : Application
{
    private readonly IHost _host;

    public App()
    {
        HostApplicationBuilder builder = Host.CreateApplicationBuilder();

        // --- Konfiguracja: sekcja AppSettings z appsettings.json → klasa AppSettings (IOptions<AppSettings>)
        builder.Services.Configure<AppSettings>(builder.Configuration.GetSection(AppSettings.SectionName));

        ConfigureServices(builder.Services);

        _host = builder.Build();
    }

    private static void ConfigureServices(IServiceCollection services)
    {
        // --- Serwisy (logika, dostęp do danych)
        // AddHttpClient<TInterface, TImpl>: kontener tworzy HttpClient przez IHttpClientFactory
        // i wstrzykuje go do konstruktora OpenMeteoService. Serwis sam nie tworzy klienta.
        services.AddHttpClient<IMeteoService, OpenMeteoService>(client =>
        {
            client.Timeout = TimeSpan.FromSeconds(10);
        });

        // Wersja bez internetu – odkomentuj, a OpenMeteoService zakomentuj:
        // services.AddSingleton<IMeteoService, FakeMeteoService>();

        // --- ViewModele
        // Singleton: jeden MainViewModel na całą aplikację (stan okna przeżywa jego ponowne otwarcie)
        services.AddSingleton<MainViewModel>();

        // --- Widoki (okna)
        // Transient: nowe okno przy każdym żądaniu
        services.AddTransient<MainWindow>();
    }

    private void OnStartup(object sender, StartupEventArgs e)
    {
        _host.Start();

        // Kontener tworzy MainWindow i wszystko, czego ono potrzebuje
        // (MainViewModel → IMeteoService → HttpClient + IOptions<AppSettings>)
        MainWindow mainWindow = _host.Services.GetRequiredService<MainWindow>();
        mainWindow.Show();
    }

    private void OnExit(object sender, ExitEventArgs e)
    {
        _host.StopAsync().GetAwaiter().GetResult();
        _host.Dispose();
    }
}
