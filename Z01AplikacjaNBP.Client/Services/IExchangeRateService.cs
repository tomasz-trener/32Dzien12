using Z01AplikacjaNBP.Client.Models;

namespace Z01AplikacjaNBP.Client.Services;

public interface IExchangeRateService
{
    /// <summary>Pobiera aktualną tabelę A (kursy średnie) z NBP.</summary>
    Task<ExchangeRateTable?> GetTableAAsync();
}
