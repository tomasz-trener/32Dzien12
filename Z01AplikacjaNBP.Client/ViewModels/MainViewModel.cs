using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Z01AplikacjaNBP.Client.Models;
using Z01AplikacjaNBP.Client.Services;

namespace Z01AplikacjaNBP.Client.ViewModels;

/// <summary>
/// Rozwiązanie zadania: lista kursów NBP w DataGrid + prosty przelicznik.
/// Ten sam schemat co w P03: serwis przez interfejs, komenda async, IsBusy, ErrorMessage.
/// </summary>
public partial class MainViewModel : ObservableObject
{
    private readonly IExchangeRateService _exchangeRateService;

    public MainViewModel(IExchangeRateService exchangeRateService)
    {
        _exchangeRateService = exchangeRateService;
    }

    public ObservableCollection<Rate> Rates { get; } = [];

    [ObservableProperty]
    public partial string TableInfo { get; set; } = "Kliknij „Pobierz kursy”";

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(LoadRatesCommand))]
    public partial bool IsBusy { get; set; }

    [ObservableProperty]
    public partial string? ErrorMessage { get; set; }

    // --- przelicznik: kwota w PLN × wybrany kurs ---

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ConvertedAmount))]
    public partial Rate? SelectedRate { get; set; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ConvertedAmount))]
    public partial decimal AmountPln { get; set; } = 100;

    /// <summary>Właściwość wyliczana – odświeżana przez NotifyPropertyChangedFor powyżej.</summary>
    public string ConvertedAmount =>
        SelectedRate is null
            ? "—"
            : $"{AmountPln:N2} PLN = {AmountPln / SelectedRate.Mid:N2} {SelectedRate.Code}";

    [RelayCommand(CanExecute = nameof(CanLoadRates))]
    private async Task LoadRatesAsync()
    {
        IsBusy = true;
        ErrorMessage = null;

        try
        {
            ExchangeRateTable? table = await _exchangeRateService.GetTableAAsync();

            Rates.Clear();

            if (table is null)
            {
                ErrorMessage = "NBP nie zwrócił żadnej tabeli.";
                return;
            }

            foreach (Rate rate in table.Rates.OrderBy(r => r.Code))
            {
                Rates.Add(rate);
            }

            TableInfo = $"Tabela {table.No} z dnia {table.EffectiveDate:d} ({Rates.Count} walut)";
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

    private bool CanLoadRates() => !IsBusy;
}
