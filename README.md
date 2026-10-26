# 32Dzien12 – MVVM, Dependency Injection, delegaty

Edycja 32 kursu C#/.NET, dzień 12. Kod końcowy dnia oraz instrukcja krok po kroku.

| Plik | Opis |
|---|---|
| [Instrukcja.md](Instrukcja.md) | instrukcja dnia – teoria, kroki, kod, punkty kontrolne |
| [Instrukcja-male-kroki.md](Instrukcja-male-kroki.md) | MVVM i DI w 10 małych krokach: osobne mini-projekty (`MaleKroki/`), po jednym temacie na projekt, z eksperymentami |

## Punkt startowy

`P03WeatherForecastWPF.Client` z repozytorium `32Dzien11` (klient Open‑Meteo z logiką w code‑behind).

## Projekty

Folder `MaleKroki/` ma własną solucję `MaleKroki.slnx` z dziesięcioma mini-projektami (K01–K10) do instrukcji „małe kroki”. Nie są częścią `32Dzien12.slnx`.


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
