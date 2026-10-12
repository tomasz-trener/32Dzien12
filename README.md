# 32Dzien12 – MVVM, Dependency Injection, delegaty

Edycja 32 kursu C#/.NET, dzień 12. Kod końcowy dnia oraz dwie instrukcje krok po kroku.

| Plik | Dla kogo |
|---|---|
| [INSTRUKCJA_PROWADZACY.md](INSTRUKCJA_PROWADZACY.md) | trener: co mówić, co klikać, co pokazywać (🗣️ ⌨️ 👀 💡) |
| [INSTRUKCJA_STUDENT.md](INSTRUKCJA_STUDENT.md) | uczestnik: teoria w pigułce, kroki, punkty kontrolne, zadania |

## Punkt startowy

`P03WeatherForecastWPF.Client` z repozytorium `32Dzien11` (klient Open‑Meteo z logiką w code‑behind).

## Projekty

| Projekt | Typ | Co pokazuje |
|---|---|---|
| `P04Delegaty` | konsola | własne delegaty, `Action`/`Func`/`Predicate`, lambdy, multicast, zdarzenia (`event`) – fundament pod `ICommand` i `INotifyPropertyChanged` |
| `P03WeatherForecastWPF.Client` | WPF | **MVVM**: najpierw ręcznie (`ViewModels/Classic`, własny `RelayCommand`), potem **CommunityToolkit.Mvvm** (`[ObservableProperty]`, `[RelayCommand]`); binding, konwertery; **Dependency Injection** przez Generic Host (`Host.CreateApplicationBuilder`), `IMeteoService` z dwiema implementacjami (Open‑Meteo i Fake), `IOptions<AppSettings>` |
| `Z01AplikacjaNBP.Client` | WPF | rozwiązanie zadania: kursy walut NBP w `DataGrid` + przelicznik, ten sam schemat MVVM + DI |

## Wymagania

- .NET 10 SDK, Visual Studio 2026 z workloadem **.NET desktop development**
- Internet: `open-meteo.com`, `api.nbp.pl`

## Uruchomienie

```powershell
dotnet build 32Dzien12.slnx
dotnet run --project P04Delegaty
dotnet run --project P03WeatherForecastWPF.Client
dotnet run --project Z01AplikacjaNBP.Client
```

## Co dalej

Dzień 13 (`32Dzien13`): własne Web API (`P05Shop.API`), biblioteka wspólna (`P06Shop.Shared`), generator danych (`P07Shop.DataSeeder`) i klient WPF, który z nich korzysta.
