using System.Text.Json.Serialization;

namespace Z01AplikacjaNBP.Client.Models;

/// <summary>
/// Odpowiedź z https://api.nbp.pl/api/exchangerates/tables/A?format=json
/// API zwraca TABLICĘ tabel (zwykle jedną), każda z listą kursów.
/// NBP używa camelCase, więc wystarczy opcja PropertyNameCaseInsensitive
/// albo – jak tu – jawne [JsonPropertyName].
/// </summary>
public class ExchangeRateTable
{
    [JsonPropertyName("table")]
    public string Table { get; set; } = string.Empty;

    [JsonPropertyName("no")]
    public string No { get; set; } = string.Empty;

    [JsonPropertyName("effectiveDate")]
    public DateOnly EffectiveDate { get; set; }

    [JsonPropertyName("rates")]
    public List<Rate> Rates { get; set; } = [];
}

public class Rate
{
    [JsonPropertyName("currency")]
    public string Currency { get; set; } = string.Empty;

    [JsonPropertyName("code")]
    public string Code { get; set; } = string.Empty;

    /// <summary>Kurs średni w PLN za jednostkę waluty.</summary>
    [JsonPropertyName("mid")]
    public decimal Mid { get; set; }
}
