// K10DiWpf/App.xaml.cs
using System.Windows;
using K10DiWpf.Services;
using K10DiWpf.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace K10DiWpf;

/// <summary>
/// Generic Host = kontener DI + konfiguracja (appsettings.json) + logowanie, w jednym.
/// Dokładnie ten sam mechanizm działa w ASP.NET Core (dzień 13).
/// </summary>
public partial class App : Application
{
    private readonly IHost _host;

    public App()
    {
        HostApplicationBuilder builder = Host.CreateApplicationBuilder();

        // Konfiguracja: sekcja "Greeting" z appsettings.json → IOptions<GreetingOptions>
        builder.Services.Configure<GreetingOptions>(
            builder.Configuration.GetSection(GreetingOptions.SectionName));

        // Serwisy
        builder.Services.AddSingleton<IClock, SystemClock>();
        // Podmiana implementacji – jedna linia (zakomentuj SystemClock, odkomentuj FixedClock):
        // builder.Services.AddSingleton<IClock, FixedClock>();

        // ViewModel i okno
        builder.Services.AddSingleton<MainViewModel>();
        builder.Services.AddTransient<MainWindow>();

        _host = builder.Build();
    }

    private void OnStartup(object sender, StartupEventArgs e)
    {
        _host.Start();

        // Kontener tworzy MainWindow → MainViewModel → IClock + IOptions<GreetingOptions>
        MainWindow mainWindow = _host.Services.GetRequiredService<MainWindow>();
        mainWindow.Show();
    }

    private void OnExit(object sender, ExitEventArgs e)
    {
        _host.StopAsync().GetAwaiter().GetResult();
        _host.Dispose();
    }
}
