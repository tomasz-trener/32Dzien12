using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Z01AplikacjaNBP.Client.Services;
using Z01AplikacjaNBP.Client.ViewModels;

namespace Z01AplikacjaNBP.Client;

public partial class App : Application
{
    private readonly IHost _host;

    public App()
    {
        HostApplicationBuilder builder = Host.CreateApplicationBuilder();

        // HttpClient z adresem bazowym – serwis używa adresów względnych
        builder.Services.AddHttpClient<IExchangeRateService, NbpExchangeRateService>(client =>
        {
            client.BaseAddress = new Uri("https://api.nbp.pl/api/");
            client.Timeout = TimeSpan.FromSeconds(10);
        });

        builder.Services.AddSingleton<MainViewModel>();
        builder.Services.AddTransient<MainWindow>();

        _host = builder.Build();
    }

    private void OnStartup(object sender, StartupEventArgs e)
    {
        _host.Start();
        _host.Services.GetRequiredService<MainWindow>().Show();
    }

    private void OnExit(object sender, ExitEventArgs e)
    {
        _host.StopAsync().GetAwaiter().GetResult();
        _host.Dispose();
    }
}
