using System.Net.Http;
using System.Net.Http.Json;
using Z01AplikacjaNBP.Client.Models;

namespace Z01AplikacjaNBP.Client.Services;

/// <summary>
/// Klient API Narodowego Banku Polskiego. Dokumentacja: https://api.nbp.pl
/// BaseAddress (https://api.nbp.pl/api/) ustawiamy w App.xaml.cs przy AddHttpClient.
/// </summary>
public class NbpExchangeRateService : IExchangeRateService
{
    private readonly HttpClient _httpClient;

    public NbpExchangeRateService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<ExchangeRateTable?> GetTableAAsync()
    {
        // Adres względny – doklejany do BaseAddress
        ExchangeRateTable[]? tables = await _httpClient.GetFromJsonAsync<ExchangeRateTable[]>("exchangerates/tables/A?format=json");

        return tables?.FirstOrDefault();
    }
}
