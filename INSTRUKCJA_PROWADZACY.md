# Dzień 12: scenariusz prowadzenia (krok po kroku)

> **Technologia:** .NET 10 (LTS), C# 14, Visual Studio 2026, solucja `32Dzien12.slnx`.
> **Punkt startowy:** projekt `P03WeatherForecastWPF.Client` z dnia 11 (klient Open‑Meteo z logiką w code‑behind). Pozostałe projekty z dnia 11 (P01, P02) zostają w repozytorium `32Dzien11`; dziś ich nie ruszamy.
> **Cel dnia:** ta sama aplikacja pogodowa, ale zbudowana „jak w firmie”: logika w **ViewModelu** (wzorzec MVVM), okno podpięte **bindingiem**, obiekty tworzone przez **kontener Dependency Injection** (Generic Host). Po drodze projekt konsolowy **P04Delegaty**, bo bez delegatów i zdarzeń nie da się zrozumieć `ICommand` ani `INotifyPropertyChanged`. Na koniec zadanie **Z01AplikacjaNBP.Client** (kursy walut NBP w `DataGrid`), które uczestnicy robią sami tym samym schematem.
> Od dnia 13 do P03 dojdzie własne Web API; ViewModel i serwisy przez interfejs zostają bez zmian.

## Jak czytać ten scenariusz

| Znacznik | Znaczenie |
|---|---|
| 🗣️ **Mówię** | Tekst do powiedzenia uczestnikom, prawie dosłownie |
| ⌨️ **Robię** | Dokładne kliknięcia, nazwy i polecenia do wpisania |
| 👀 **Pokazuję** | Na co zwrócić uwagę na ekranie |
| 💡 **Wyjaśniam** | Szersze omówienie, co i dlaczego się dzieje |
| 🔧 **Ulepszenie** | Tu robimy **inaczej niż w edycjach 26/31**. Tamto działało, to jest poprawka lub nowsza praktyka |
| ✅ **Efekt** | Punkt kontrolny: co ma działać, zanim pójdziesz dalej |
| 🔗 **Przejście** | Jak połączyć ten temat z następnym |
| 🟩 **Co dopisujesz** | Podgląd zmian w istniejącym pliku: linie `+` dopisujesz, `-` usuwasz, `⋯` to pominięty kod. Pod spodem zawsze jest pełny plik |

Kod w blokach jest **gotowy do skopiowania w całości**. Nad blokiem jest ścieżka pliku: zastąp całą zawartość tego pliku. Znaków `+`/`-` z bloków 🟩 nie kopiuj. Pliki oznaczone „wersja tymczasowa” są etapem pośrednim: nadpiszesz je w późniejszej sekcji, ale dzięki nim projekt kompiluje się po każdym bloku.

## Plan dnia (orientacyjnie)

| Blok | Temat | Czas |
|---|---|---|
| 0 | Wstęp: po co MVVM, po co Dependency Injection, Generic Host | 25 min |
| 1 | P04Delegaty: delegaty, `Action`/`Func`/`Predicate`, lambdy, multicast, zdarzenia | 40 min |
| 2 | P03: MVVM „ręcznie” – `INotifyPropertyChanged`, własny `RelayCommand`, `MainViewModelClassic` | 50 min |
| 3 | P03: binding w XAML, `Command` zamiast `Click` | 40 min |
| 4 | P03: interfejs `IMeteoService`, `FakeMeteoService`, wzorzec Options (`AppSettings`) | 30 min |
| 5 | P03: CommunityToolkit.Mvvm – `ObservableObject`, `[ObservableProperty]`, `[RelayCommand]` | 40 min |
| 6 | P03: konwertery i zasoby w `App.xaml` | 20 min |
| 7 | P03: Dependency Injection przez Generic Host w `App.xaml.cs` | 50 min |
| 8 | Zadanie Z01AplikacjaNBP.Client: praca własna + omówienie rozwiązania | 90 min |
| 9 | Podsumowanie, pytania, commit | 15 min |

Przerwy po blokach 2, 4 i 7. Blok 8 zaczyna się od 45 minut pracy własnej.

## Dane testowe (używaj zawsze tych samych)

| Gdzie | Wartość |
|---|---|
| P04Delegaty | bez wejścia – program wypisuje 5 sekcji |
| P03, miasto | `Warszawa` (Open‑Meteo zwraca 2 wyniki, pierwszy to Warszawa, Województwo mazowieckie), `Kraków` (pokazuje też Krakow w Wisconsin), `xyzxyz` (komunikat „Nie znaleziono miasta”), pole puste (przycisk **Szukaj** wyłączony) |
| P03 z `FakeMeteoService` | `Warszawa` → 1 wynik, `a` → 3 wyniki (Warszawa, Kraków, Gdańsk), `xyzxyz` → brak |
| Z01, przelicznik | kwota `430` PLN, waluta `EUR` → `430,00 PLN = 98,23 EUR` (kurs 4,3775 z tabeli z 2026‑10‑09; u Ciebie będzie aktualny kurs) |

---

## 0. Przygotowanie (u siebie, przed startem)

⌨️ **Robię:**

1. Sprawdzam SDK w **Developer PowerShell** (Visual Studio: **View → Terminal**): `dotnet --version` ma pokazać `10.0.x`.

2. Sprawdzam w przeglądarce usługi, z których dziś korzystamy:
   - `https://geocoding-api.open-meteo.com/v1/search?name=Warszawa&count=10&language=pl&format=json` → JSON z listą miast
   - `https://api.nbp.pl/api/exchangerates/tables/A?format=json` → JSON: tablica z jedną tabelą kursów

3. Przygotowuję folder `32Dzien12` z kopią projektu `P03WeatherForecastWPF.Client` z dnia 11 (bez folderów `bin` i `obj`). Repozytorium GitHub `tomasz-trainer/32Dzien12` ma gotowy kod, ale na zajęciach piszemy go od zera.

4. Sprawdzam, że pakiety NuGet są osiągalne: `CommunityToolkit.Mvvm 8.4.2`, `Microsoft.Extensions.Hosting 10.0.12`, `Microsoft.Extensions.Http 10.0.12`. Jeśli sala ma słaby internet, przywracam je u siebie wcześniej (`dotnet restore`) i mam folder `%USERPROFILE%\.nuget\packages` do skopiowania.

5. Mam otwarte okno **Output** (View → Output, źródło **Debug**): będę w nim pokazywać błędy bindingu.

**Punkt startowy (stan P03 po dniu 11):**

| Plik | Co zawiera | Co się z nim dziś dzieje |
|---|---|---|
| `Models/City.cs`, `GeocodingResponse.cs`, `ForecastResponse.cs`, `Weather.cs` | DTO Open‑Meteo i model domenowy | **bez zmian** |
| `Services/OpenMeteoService.cs` | geocoding + pogoda, `HttpClient` i język przez konstruktor | implementuje `IMeteoService`, język z `IOptions<AppSettings>` (blok 4) |
| `MainWindow.xaml` | `x:Name`, `Click`, `SelectionChanged` | binding i `Command` (bloki 3, 5, 6) |
| `MainWindow.xaml.cs` | 91 linii logiki w code‑behind | 18 linii: `DataContext = viewModel` (bloki 2, 7) |
| `App.xaml`, `App.xaml.cs` | `StartupUri`, pusta klasa | konwertery, style, Generic Host (bloki 6, 7) |
| `appsettings.json` | `AppSettings:DefaultLanguage = pl` | **bez zmian**, czyta go Generic Host |
| `P03WeatherForecastWPF.Client.csproj` | `Microsoft.Extensions.Configuration.Json` | trzy nowe pakiety (blok 2) |

### 0.1 Wstęp dla uczestników (teoria, 15–20 min)

🗣️ **Mówię:**
„Wczoraj skończyliśmy aplikację pogodową, która działa. Otwórzcie `MainWindow.xaml.cs` z dnia 11 i popatrzcie: 91 linii, a w nich pobieranie z internetu, obsługa błędów, czytanie konfiguracji **i** ustawianie `lblTemperature.Content`. Wszystko w jednej klasie, która dziedziczy po `Window`. Trzy problemy:

1. **Testowalność.** Żeby sprawdzić, czy dla `xyzxyz` pojawia się komunikat, muszę uruchomić okno i kliknąć. Nie da się napisać testu automatycznego, bo logika żyje w metodzie `btnSearch_Click`, która potrzebuje kontrolek.
2. **Duplikacja.** W dniu 18 ta sama aplikacja ma działać na Androidzie (MAUI). Z code‑behind musiałbym skopiować całą logikę do drugiego okna i utrzymywać dwie kopie.
3. **Sprzężenie z kontrolkami.** Jeśli grafik zamieni `Label` na `TextBlock`, kod przestaje się kompilować (`Content` → `Text`). Logika wie za dużo o wyglądzie.

Rozwiązaniem jest wzorzec **MVVM** (Model–View–ViewModel), stworzony przez Microsoft właśnie dla WPF”.

💡 **Wyjaśniam** (rysuję na tablicy trzy prostokąty i strzałki):

| Element | Co to jest | U nas |
|---|---|---|
| **Model** | dane i reguły domenowe, nie wie o UI | `City`, `Weather`, `OpenMeteoService` |
| **View** (widok) | XAML, wygląd; code‑behind prawie pusty | `MainWindow.xaml` |
| **ViewModel** | stan okna jako właściwości (`CityName`, `Cities`, `Weather`) i akcje jako komendy (`LoadCitiesCommand`); zwykła klasa C#, nie zna kontrolek | `MainViewModel` |
| **Binding** (wiązanie) | mechanizm WPF, który łączy właściwość kontrolki z właściwością ViewModelu w obie strony | `Text="{Binding CityName}"` |
| **Komenda** (`ICommand`) | obiekt „wykonaj” + „czy można wykonać”, zamiast obsługi `Click` | `Command="{Binding LoadCitiesCommand}"` |
| **Powiadomienie** (`INotifyPropertyChanged`) | zdarzenie, przez które ViewModel mówi widokowi „zmieniłem się, odśwież” | `OnPropertyChanged()` |

„ViewModel nie ma referencji do okna. Okno ma referencję do ViewModelu przez `DataContext`. Kierunek zależności jest jeden: widok → ViewModel → model. Dlatego ViewModel można testować w konsoli i użyć ponownie w MAUI”.

🗣️ **Mówię** (zalety, wady, alternatywy):

| Podejście | Zalety | Wady |
|---|---|---|
| **Code‑behind** (dzień 11) | najszybsze na start, zero dodatkowych klas | nietestowalne, nie do ponownego użycia, rośnie bez kontroli |
| **MVP** (Model–View–Presenter, Windows Forms) | testowalny presenter | widok musi implementować interfejs, presenter ręcznie ustawia kontrolki |
| **MVVM** (WPF, MAUI, Avalonia, WinUI) | binding robi „przepisywanie” za nas, testowalny ViewModel, jeden ViewModel na wiele platform | więcej plików, trochę kodu powtarzalnego (dziś zobaczymy, jak CommunityToolkit go generuje), błędy bindingu widać dopiero w czasie działania |
| **MVU** (ReactiveUI, Comet, Elm) | stan niezmienny, przewidywalny | inny sposób myślenia, mniejszy ekosystem w .NET |
| **Komponenty** (Blazor, dzień 17) | kod i znaczniki w jednym pliku `.razor` | tylko web; w dniu 17 zobaczycie, że i tam serwisy wstrzykujemy tak samo |

„Druga rzecz na dziś: **Dependency Injection** (DI, wstrzykiwanie zależności). Popatrzcie na konstruktor okna z dnia 11: `new OpenMeteoService(new HttpClient(), language)`. Okno samo tworzy serwis, więc:

- nie da się podmienić serwisu na wersję testową bez zmiany okna,
- gdy serwis dostanie trzeci parametr, trzeba poprawić każde miejsce, gdzie jest `new`,
- `HttpClient` tworzymy ręcznie i musimy pamiętać, że ma być jeden na aplikację.

Zasada **odwrócenia zależności** (Dependency Inversion): klasa nie tworzy swoich zależności, tylko dostaje je **konstruktorem**, najlepiej jako **interfejs**. `MainViewModel(IMeteoService service)` nie wie, czy dostał Open‑Meteo, dane testowe czy jutro nasze własne API. Kto tworzy obiekty? **Kontener DI**: rejestrujemy w nim »dla `IMeteoService` użyj `OpenMeteoService`«, a on sam buduje cały łańcuch: okno → ViewModel → serwis → `HttpClient` + konfiguracja”.

💡 **Wyjaśniam** cykle życia (zapisuję na tablicy, bo wrócą w dniu 13 i 14):

| Rejestracja | Ile obiektów | Kiedy używać |
|---|---|---|
| `AddSingleton<T>()` | jeden na całą aplikację | serwisy bez stanu użytkownika, cache, ViewModel okna głównego |
| `AddTransient<T>()` | nowy przy każdym pobraniu | lekkie obiekty, okna (`MainWindow`) |
| `AddScoped<T>()` | jeden na „zakres” (w ASP.NET Core: jedno żądanie HTTP) | serwisy z `DbContext` w API (dzień 13–14); w WPF zakres trzeba tworzyć ręcznie, więc dziś go nie używamy |

„Pułapka: singleton nie może zależeć od obiektu o krótszym życiu (przechowałby go na zawsze). Dlatego `MainViewModel` (singleton) dostaje serwis zarejestrowany też jako singleton albo przez fabrykę `HttpClient` (`AddHttpClient`), która sama dba o odświeżanie połączeń.

Kontener uruchomimy przez **Generic Host** (`Host.CreateApplicationBuilder`). To ten sam obiekt, który jutro zobaczycie w `Program.cs` projektu Web API: `builder.Configuration` (czyta `appsettings.json`), `builder.Logging`, `builder.Services`. Uczymy się raz, używamy w WPF, API, Blazorze i MAUI”.

🔗 **Przejście:** „Zanim napiszemy ViewModel, musimy rozumieć dwa mechanizmy, na których stoi: komenda to **delegat** opakowany w obiekt, a powiadomienie o zmianie to **zdarzenie**. Krótki projekt konsolowy”.

---

## 1. P04Delegaty: delegaty, lambdy, zdarzenia

**Cel:** uczestnicy rozumieją, że delegat to „zmienna, w której trzymamy metodę”, znają `Action`/`Func`/`Predicate`, wiedzą, czym `event` różni się od zwykłego delegata, i widzą związek z MVVM.

⌨️ **Robię** w terminalu, w folderze `32Dzien12` (jest w nim już skopiowany folder `P03WeatherForecastWPF.Client`):

```powershell
dotnet new sln -n 32Dzien12 --format slnx
dotnet sln 32Dzien12.slnx add P03WeatherForecastWPF.Client/P03WeatherForecastWPF.Client.csproj
dotnet new console -n P04Delegaty -o P04Delegaty -f net10.0
dotnet sln 32Dzien12.slnx add P04Delegaty/P04Delegaty.csproj
dotnet new gitignore
```

Otwieram `32Dzien12.slnx` w Visual Studio. Plik projektu jest standardowy:

```xml
<!-- P04Delegaty/P04Delegaty.csproj -->
<Project Sdk="Microsoft.NET.Sdk">

  <PropertyGroup>
    <OutputType>Exe</OutputType>
    <TargetFramework>net10.0</TargetFramework>
    <ImplicitUsings>enable</ImplicitUsings>
    <Nullable>enable</Nullable>
  </PropertyGroup>

</Project>
```

### 1.1 Własny delegat i metoda wyższego rzędu

⌨️ **Robię:** dodaję plik `Converters.cs`:

```csharp
// P04Delegaty/Converters.cs
namespace P04Delegaty;

/// <summary>
/// Własny typ delegata: każda metoda "double → double" pasuje do tego typu.
/// </summary>
public delegate double TemperatureConverter(double celsius);

public static class Converters
{
    public static double CelsiusToFahrenheit(double celsius) => celsius * 1.8 + 32;

    /// <summary>
    /// Metoda wyższego rzędu: przyjmuje delegat jako parametr.
    /// Nie wie, jak przeliczać – dostaje "przepis" z zewnątrz.
    /// </summary>
    public static void PrintConverted(IEnumerable<double> values, TemperatureConverter converter)
    {
        foreach (double value in values)
        {
            Console.WriteLine($"  {value} °C → {converter(value):F1}");
        }
    }
}
```

🗣️ **Mówię:** „`delegate double TemperatureConverter(double celsius)` to **typ**. Zmienna tego typu może trzymać każdą metodę, która bierze `double` i zwraca `double`: nazwaną, anonimową, lambdę. `PrintConverted` nie wie, jak przeliczać: dostaje przepis z zewnątrz. Tak samo `RelayCommand` nie wie, co ma zrobić po kliknięciu, dostanie to z ViewModelu”.

### 1.2 Zdarzenia

⌨️ **Robię:** dodaję plik `TemperatureSensor.cs`:

```csharp
// P04Delegaty/TemperatureSensor.cs
namespace P04Delegaty;

/// <summary>
/// Dane przekazywane subskrybentom zdarzenia. Konwencja .NET: klasa dziedziczy po EventArgs.
/// </summary>
public class TemperatureChangedEventArgs(double oldValue, double newValue) : EventArgs
{
    public double OldValue { get; } = oldValue;
    public double NewValue { get; } = newValue;
}

/// <summary>
/// Obiekt, który "mówi" o zmianach przez zdarzenie – dokładnie tak,
/// jak ViewModel mówi widokowi o zmianie właściwości przez PropertyChanged.
/// </summary>
public class TemperatureSensor
{
    private double _temperature;

    // event = delegat multicast, który z zewnątrz można tylko subskrybować (+=/-=),
    // a wywołać może tylko klasa, w której jest zadeklarowany.
    // EventHandler<T> to standardowy delegat: (object? sender, T e)
    public event EventHandler<TemperatureChangedEventArgs>? TemperatureChanged;

    public double Temperature
    {
        get => _temperature;
        set
        {
            if (_temperature == value)
            {
                return;   // bez zmiany – nie powiadamiamy
            }

            double old = _temperature;
            _temperature = value;

            // ?.Invoke – jeśli nikt nie subskrybuje, delegat jest null i nic się nie dzieje
            TemperatureChanged?.Invoke(this, new TemperatureChangedEventArgs(old, value));
        }
    }
}

public static class Alarm
{
    public static void OnTemperatureChanged(object? sender, TemperatureChangedEventArgs e)
    {
        if (e.NewValue > 30)
        {
            Console.WriteLine($"  !!! ALARM: {e.NewValue} °C – za gorąco!");
        }
    }
}
```

💡 **Wyjaśniam:**

- „`event` to delegat **multicast** z ograniczeniem: z zewnątrz wolno tylko `+=` i `-=`. Wywołać go (`Invoke`) może tylko klasa, w której jest zadeklarowany. Bez słowa `event` ktoś z zewnątrz mógłby zrobić `sensor.TemperatureChanged = null` i wyrzucić wszystkich subskrybentów”.
- „`EventHandler<T>` to standardowy delegat zdarzeń w .NET: `(object? sender, T e)`. `TemperatureChangedEventArgs` dziedziczy po `EventArgs` i ma **konstruktor podstawowy** (primary constructor, C# 12): parametry w nagłówku klasy, przypisane do właściwości”.
- „`TemperatureChanged?.Invoke(...)`: gdy nikt nie subskrybuje, delegat jest `null`. Operator `?.` chroni przed `NullReferenceException`”.
- „Setter sprawdza, czy wartość się zmieniła. Dokładnie taki setter napiszemy za chwilę w ViewModelu, tylko zdarzenie będzie się nazywać `PropertyChanged`”.

### 1.3 `Program.cs`

```csharp
// P04Delegaty/Program.cs
// P04Delegaty – delegaty, lambdy, Action/Func/Predicate, multicast, zdarzenia.
// To fundament pod ICommand (RelayCommand) i INotifyPropertyChanged, których używamy w MVVM.

using P04Delegaty;

Console.OutputEncoding = System.Text.Encoding.UTF8;

// ---------------------------------------------------------------------------
// 1. Własny typ delegata: "wskaźnik na metodę" o określonej sygnaturze
// ---------------------------------------------------------------------------
Console.WriteLine("--- 1. Własny delegat ---");

TemperatureConverter toFahrenheit = Converters.CelsiusToFahrenheit;   // przypisanie metody nazwanej
TemperatureConverter toKelvin = c => c + 273.15;                        // przypisanie lambdy

Console.WriteLine($"20 °C = {toFahrenheit(20)} °F");
Console.WriteLine($"20 °C = {toKelvin(20)} K");

// Delegat jako parametr: metoda nie wie, JAK przeliczać, dostaje przepis z zewnątrz
Converters.PrintConverted([0, 10, 25], toFahrenheit);

// ---------------------------------------------------------------------------
// 2. Gotowe delegaty generyczne: Action (nic nie zwraca), Func (zwraca), Predicate (zwraca bool)
// ---------------------------------------------------------------------------
Console.WriteLine("\n--- 2. Action / Func / Predicate ---");

Action<string> greet = name => Console.WriteLine($"Cześć, {name}!");
greet("Tomasz");

Action<string> print = Console.WriteLine;   // grupa metod – kompilator sam dobiera przeciążenie
print("Action może wskazywać na metodę z biblioteki");

Func<int, int, double> average = (a, b) => (a + b) / 2.0;
Console.WriteLine($"Średnia 3 i 4 = {average(3, 4)}");

Predicate<int> isEven = n => n % 2 == 0;
Console.WriteLine($"Czy 4 jest parzyste? {isEven(4)}");

// Delegaty w LINQ: Where przyjmuje Func<T, bool>, Select przyjmuje Func<T, TResult>
int[] numbers = [1, 2, 3, 4, 5, 6];
var evenSquares = numbers.Where(n => isEven(n)).Select(n => n * n);
Console.WriteLine($"Kwadraty parzystych: {string.Join(", ", evenSquares)}");

// ---------------------------------------------------------------------------
// 3. Multicast: jeden delegat może wskazywać na wiele metod (+= / -=)
// ---------------------------------------------------------------------------
Console.WriteLine("\n--- 3. Multicast ---");

Action<string> notify = msg => Console.WriteLine($"[konsola] {msg}");
notify += msg => Console.WriteLine($"[plik]    {msg}");
notify += msg => Console.WriteLine($"[e-mail]  {msg}");

notify("Zamówienie przyjęte");          // wywołuje wszystkie trzy po kolei

// ---------------------------------------------------------------------------
// 4. Zdarzenia (event): delegat multicast, którego z zewnątrz można tylko subskrybować
// ---------------------------------------------------------------------------
Console.WriteLine("\n--- 4. Zdarzenia ---");

var sensor = new TemperatureSensor();

// Subskrypcja: "gdy zmieni się temperatura, wykonaj ten kod"
sensor.TemperatureChanged += (sender, e) =>
    Console.WriteLine($"Zmiana: {e.OldValue} → {e.NewValue} °C");

sensor.TemperatureChanged += Alarm.OnTemperatureChanged;   // druga subskrypcja: metoda nazwana

sensor.Temperature = 21.5;
sensor.Temperature = 35.0;   // Alarm zareaguje
sensor.Temperature = 35.0;   // bez zmiany – zdarzenie nie wystąpi

sensor.TemperatureChanged -= Alarm.OnTemperatureChanged;   // wypisanie się
sensor.Temperature = 40.0;   // tylko pierwszy subskrybent

// ---------------------------------------------------------------------------
// 5. Gdzie to spotkamy w WPF?
// ---------------------------------------------------------------------------
Console.WriteLine("\n--- 5. Związek z MVVM ---");
Console.WriteLine("INotifyPropertyChanged.PropertyChanged  → event (jak TemperatureChanged)");
Console.WriteLine("ICommand.Execute / CanExecute           → Action<object?> i Predicate<object?> w RelayCommand");
Console.WriteLine("Button.Click=\"...\"                        → delegat RoutedEventHandler");
```

⌨️ **Robię:** `dotnet run --project P04Delegaty`.

👀 **Pokazuję** wyjście i omawiam sekcja po sekcji:

```text
--- 1. Własny delegat ---
20 °C = 68 °F
20 °C = 293.15 K
  0 °C → 32.0
  10 °C → 50.0
  25 °C → 77.0

--- 2. Action / Func / Predicate ---
Cześć, Tomasz!
Action może wskazywać na metodę z biblioteki
Średnia 3 i 4 = 3.5
Czy 4 jest parzyste? True
Kwadraty parzystych: 4, 16, 36

--- 3. Multicast ---
[konsola] Zamówienie przyjęte
[plik]    Zamówienie przyjęte
[e-mail]  Zamówienie przyjęte

--- 4. Zdarzenia ---
Zmiana: 0 → 21.5 °C
Zmiana: 21.5 → 35 °C
  !!! ALARM: 35 °C – za gorąco!
Zmiana: 35 → 40 °C

--- 5. Związek z MVVM ---
INotifyPropertyChanged.PropertyChanged  → event (jak TemperatureChanged)
ICommand.Execute / CanExecute           → Action<object?> i Predicate<object?> w RelayCommand
Button.Click="..."                        → delegat RoutedEventHandler
```

🗣️ **Mówię** (po jednej myśli na sekcję):

1. **Własny delegat.** „`toFahrenheit` wskazuje metodę nazwaną, `toKelvin` lambdę. Wywołanie wygląda jak wywołanie metody. `PrintConverted([0, 10, 25], toFahrenheit)` – przekazujemy *zachowanie* jako argument”.
2. **Action / Func / Predicate.** „Zamiast deklarować własny typ delegata, .NET ma gotowe generyczne: `Action<T>` nic nie zwraca, `Func<T, TResult>` zwraca, `Predicate<T>` zwraca `bool`. `Action<string> print = Console.WriteLine` to **grupa metod**: kompilator sam wybiera przeciążenie z parametrem `string`. LINQ to delegaty w praktyce: `Where` przyjmuje `Func<T, bool>`, `Select` przyjmuje `Func<T, TResult>`”.
3. **Multicast.** „`+=` dokłada kolejną metodę. Jedno wywołanie `notify(...)` uruchamia wszystkie trzy po kolei, w kolejności dodania. Uwaga: jeśli delegat zwraca wartość, dostaniemy tylko wynik ostatniej metody; jeśli któraś rzuci wyjątek, kolejne się nie wykonają”.
4. **Zdarzenia.** „Dwóch subskrybentów: lambda i metoda `Alarm.OnTemperatureChanged`. Trzecie przypisanie `35.0` nie wypisuje nic, bo setter wykrył brak zmiany. Po `-=` alarm milczy przy `40.0`”.
5. **Związek z MVVM.** „To tabela na dziś: `PropertyChanged` jest zdarzeniem, `RelayCommand` trzyma `Action` i `Predicate`, a `Click="..."` z wczoraj to też delegat (`RoutedEventHandler`), tylko WPF podpinał go za nas”.

💡 **Wyjaśniam** (alternatywy, 2 min): „Zamiast delegatu można przekazać **interfejs** z jedną metodą (wzorzec Strategia). Interfejs jest lepszy, gdy zachowanie ma stan albo kilka metod; delegat, gdy to jedna funkcja. W LINQ i w komendach WPF wygrały delegaty, bo lambda jest krótsza niż klasa”.

🔧 **Ulepszenie:** edycja 31 miała w P04 tylko cztery przypisania `Action`/`Func`/`Predicate` bez kontekstu. Tu jest własny typ delegata, metoda wyższego rzędu, LINQ, multicast, pełne zdarzenie z `EventArgs` i wypisanie się, a na końcu mapa „gdzie to spotkamy w WPF”. Dzięki temu `RelayCommand` i `INotifyPropertyChanged` w bloku 2 nie są magią.

✅ **Efekt:** program wypisuje 5 sekcji jak wyżej; w sekcji 4 są dokładnie trzy linie „Zmiana” i jedna linia „ALARM”.

🔗 **Przejście:** „Mamy klocki: delegat, zdarzenie, lambda. Budujemy z nich ViewModel. Najpierw ręcznie, żeby zobaczyć każdą śrubkę, potem biblioteką, która wygeneruje to za nas”.

---

## 2. P03: MVVM „ręcznie”

**Cel:** uczestnicy piszą ViewModel z `INotifyPropertyChanged` i własnym `RelayCommand`, rozumieją, co robi każda linia, i widzą, ile w tym kodu powtarzalnego.

### 2.1 Pakiety NuGet

⌨️ **Robię:** prawy przycisk na P03 → **Manage NuGet Packages**. Usuwam `Microsoft.Extensions.Configuration.Json`, instaluję `CommunityToolkit.Mvvm` (8.4.2), `Microsoft.Extensions.Hosting` (10.0.12), `Microsoft.Extensions.Http` (10.0.12). Z konsoli:

```powershell
dotnet remove P03WeatherForecastWPF.Client package Microsoft.Extensions.Configuration.Json
dotnet add P03WeatherForecastWPF.Client package CommunityToolkit.Mvvm --version 8.4.2
dotnet add P03WeatherForecastWPF.Client package Microsoft.Extensions.Hosting --version 10.0.12
dotnet add P03WeatherForecastWPF.Client package Microsoft.Extensions.Http --version 10.0.12
```

🟩 **Co dopisujesz** w `P03WeatherForecastWPF.Client/P03WeatherForecastWPF.Client.csproj`:

```diff
⋯
 
   <ItemGroup>
-    <PackageReference Include="Microsoft.Extensions.Configuration.Json" Version="10.0.12" />
+    <!-- MVVM: ObservableObject, [ObservableProperty], [RelayCommand] -->
+    <PackageReference Include="CommunityToolkit.Mvvm" Version="8.4.2" />
+    <!-- Generic Host: konfiguracja + logowanie + DI (zawiera Configuration.Json, Options, Http) -->
+    <PackageReference Include="Microsoft.Extensions.Hosting" Version="10.0.12" />
+    <!-- AddHttpClient<TInterface, TImpl> (IHttpClientFactory) -->
+    <PackageReference Include="Microsoft.Extensions.Http" Version="10.0.12" />
   </ItemGroup>
 
```

```xml
<!-- P03WeatherForecastWPF.Client/P03WeatherForecastWPF.Client.csproj -->
<Project Sdk="Microsoft.NET.Sdk">

  <PropertyGroup>
    <OutputType>WinExe</OutputType>
    <TargetFramework>net10.0-windows</TargetFramework>
    <Nullable>enable</Nullable>
    <ImplicitUsings>enable</ImplicitUsings>
    <UseWPF>true</UseWPF>
  </PropertyGroup>

  <ItemGroup>
    <!-- MVVM: ObservableObject, [ObservableProperty], [RelayCommand] -->
    <PackageReference Include="CommunityToolkit.Mvvm" Version="8.4.2" />
    <!-- Generic Host: konfiguracja + logowanie + DI (zawiera Configuration.Json, Options, Http) -->
    <PackageReference Include="Microsoft.Extensions.Hosting" Version="10.0.12" />
    <!-- AddHttpClient<TInterface, TImpl> (IHttpClientFactory) -->
    <PackageReference Include="Microsoft.Extensions.Http" Version="10.0.12" />
  </ItemGroup>

  <ItemGroup>
    <!-- Plik konfiguracyjny kopiowany obok .exe, żeby aplikacja mogła go odczytać -->
    <None Update="appsettings.json">
      <CopyToOutputDirectory>PreserveNewest</CopyToOutputDirectory>
    </None>
  </ItemGroup>

</Project>
```

💡 **Wyjaśniam:** „Instaluję wszystko od razu, żeby nie przerywać później. `Microsoft.Extensions.Hosting` ciągnie za sobą konfigurację (w tym `Configuration.Json`, dlatego go usuwamy), logowanie, `Options` i kontener DI. `Microsoft.Extensions.Http` dokłada `AddHttpClient`. `CommunityToolkit.Mvvm` użyjemy w bloku 5; teraz piszemy ręcznie”.

### 2.2 Własny `RelayCommand`

⌨️ **Robię:** folder `Commands` → klasa `RelayCommand.cs`:

```csharp
// P03WeatherForecastWPF.Client/Commands/RelayCommand.cs
using System.Windows.Input;

namespace P03WeatherForecastWPF.Client.Commands;

/// <summary>
/// Najprostsza implementacja ICommand: opakowuje dwa delegaty –
/// co wykonać (execute) i czy można wykonać (canExecute).
/// Używa jej klasyczny ViewModel (ViewModels/Classic). W wersji docelowej
/// korzystamy z gotowego RelayCommand z CommunityToolkit.Mvvm.
/// </summary>
public class RelayCommand : ICommand
{
    private readonly Action<object?> _execute;
    private readonly Predicate<object?>? _canExecute;

    public RelayCommand(Action<object?> execute, Predicate<object?>? canExecute = null)
    {
        _execute = execute;
        _canExecute = canExecute;
    }

    /// <summary>
    /// WPF pyta "czy można wykonać?" przy każdym CommandManager.RequerySuggested
    /// (np. po kliknięciu, zmianie fokusu). Podpinamy się pod to zdarzenie,
    /// żeby przyciski same się włączały i wyłączały.
    /// </summary>
    public event EventHandler? CanExecuteChanged
    {
        add => CommandManager.RequerySuggested += value;
        remove => CommandManager.RequerySuggested -= value;
    }

    public bool CanExecute(object? parameter) => _canExecute is null || _canExecute(parameter);

    public void Execute(object? parameter) => _execute(parameter);
}
```

💡 **Wyjaśniam:**

- „`ICommand` ma trzy składowe: `Execute` (co zrobić), `CanExecute` (czy wolno) i zdarzenie `CanExecuteChanged` (powiedz przyciskowi, żeby zapytał ponownie). Nasza klasa opakowuje dwa delegaty z bloku 1: `Action<object?>` i `Predicate<object?>`”.
- „`CanExecuteChanged` nie trzyma własnej listy subskrybentów: przekazuje `add`/`remove` do `CommandManager.RequerySuggested`. WPF podnosi to zdarzenie po każdej interakcji (kliknięcie, zmiana fokusu, naciśnięcie klawisza). Dzięki temu przycisk sam się wyłącza, gdy `CanExecute` zwróci `false`, bez jednej linii w oknie. Koszt: WPF pyta często, więc `CanExecute` ma być tanie”.
- „`object? parameter`: komenda może dostać parametr z XAML (`CommandParameter`). Dziś go nie używamy, stąd `_` w lambdach”.

### 2.3 Interfejs serwisu i wersja testowa

🗣️ **Mówię:** „ViewModel potrzebuje serwisu pogodowego. Gdybym wpisał `OpenMeteoService`, związałbym ViewModel z jedną implementacją. Wprowadzam **interfejs** `IMeteoService` i od razu drugą implementację, która nie potrzebuje internetu. Dzięki niej etap »ręczny« uruchomimy bez `HttpClient` i bez konfiguracji; prawdziwy serwis podepniemy pod interfejs w bloku 4”.

⌨️ **Robię:** w folderze `Services` dodaję dwa pliki:

```csharp
// P03WeatherForecastWPF.Client/Services/IMeteoService.cs
using P03WeatherForecastWPF.Client.Models;

namespace P03WeatherForecastWPF.Client.Services;

/// <summary>
/// Kontrakt serwisu pogodowego. ViewModel zna tylko ten interfejs,
/// więc możemy podmienić implementację (Open-Meteo, dane testowe, inny dostawca)
/// bez zmiany ViewModelu – to jest sedno Dependency Injection.
/// </summary>
public interface IMeteoService
{
    Task<City[]> GetLocationsAsync(string locationName);

    Task<Weather?> GetCurrentWeatherAsync(double latitude, double longitude);
}
```

```csharp
// P03WeatherForecastWPF.Client/Services/FakeMeteoService.cs
using P03WeatherForecastWPF.Client.Models;

namespace P03WeatherForecastWPF.Client.Services;

/// <summary>
/// Implementacja "na niby": zwraca stałe dane bez dostępu do internetu.
/// Przydaje się, gdy sieć nie działa, do pracy nad wyglądem okna i do testów.
/// Rejestrujemy ją w App.xaml.cs zamiast OpenMeteoService – ViewModel tego nie zauważy.
/// </summary>
public class FakeMeteoService : IMeteoService
{
    private static readonly City[] _cities =
    [
        new() { Id = 1, Name = "Warszawa", Country = "Polska", Admin1 = "Województwo mazowieckie", Latitude = 52.2297, Longitude = 21.0122 },
        new() { Id = 2, Name = "Kraków", Country = "Polska", Admin1 = "Województwo małopolskie", Latitude = 50.0647, Longitude = 19.9450 },
        new() { Id = 3, Name = "Gdańsk", Country = "Polska", Admin1 = "Województwo pomorskie", Latitude = 54.3520, Longitude = 18.6466 }
    ];

    public async Task<City[]> GetLocationsAsync(string locationName)
    {
        await Task.Delay(500); // symulacja opóźnienia sieci

        return _cities
            .Where(c => c.Name.Contains(locationName, StringComparison.OrdinalIgnoreCase))
            .ToArray();
    }

    public async Task<Weather?> GetCurrentWeatherAsync(double latitude, double longitude)
    {
        await Task.Delay(500);

        return new Weather
        {
            Time = DateTime.Now,
            Temperature = Math.Round(Random.Shared.NextDouble() * 30 - 5, 1),
            WindSpeed = Random.Shared.Next(0, 40),
            WeatherCode = Random.Shared.Next(0, 4)
        };
    }
}
```

👀 **Pokazuję:** sygnatury w interfejsie są **identyczne** z metodami `OpenMeteoService` z dnia 11 (Visual Studio: prawy przycisk na klasie → **Quick Actions → Extract Interface** zrobiłby to samo). `FakeMeteoService` ma `Task.Delay(500)`, żeby pasek postępu było widać.

### 2.4 `MainViewModelClassic`

⌨️ **Robię:** folder `ViewModels`, w nim podfolder `Classic`, klasa `MainViewModelClassic.cs`:

```csharp
// P03WeatherForecastWPF.Client/ViewModels/Classic/MainViewModelClassic.cs
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using P03WeatherForecastWPF.Client.Commands;
using P03WeatherForecastWPF.Client.Models;
using P03WeatherForecastWPF.Client.Services;

namespace P03WeatherForecastWPF.Client.ViewModels.Classic;

/// <summary>
/// ViewModel napisany "ręcznie": INotifyPropertyChanged, pola + właściwości,
/// własny RelayCommand. Pokazuje, co dzieje się pod spodem.
/// W aplikacji używamy wersji z CommunityToolkit.Mvvm (ViewModels/MainViewModel.cs),
/// która generuje ten sam kod automatycznie. Ten plik zostaje do porównania.
/// </summary>
public class MainViewModelClassic : INotifyPropertyChanged
{
    private readonly IMeteoService _meteoService;

    private string _cityName = "Warszawa";
    private City[] _cities = [];
    private City? _selectedCity;
    private Weather? _weather;
    private bool _isBusy;
    private string? _errorMessage;

    // Zdarzenie, przez które ViewModel mówi widokowi: "właściwość X się zmieniła, odśwież się"
    public event PropertyChangedEventHandler? PropertyChanged;

    // [CallerMemberName] – kompilator sam wstawia nazwę właściwości, z której wołamy OnPropertyChanged()
    protected void OnPropertyChanged([CallerMemberName] string? propertyName = null) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

    public string CityName
    {
        get => _cityName;
        set
        {
            _cityName = value;
            OnPropertyChanged();
        }
    }

    public City[] Cities
    {
        get => _cities;
        set
        {
            _cities = value;
            OnPropertyChanged();
        }
    }

    public City? SelectedCity
    {
        get => _selectedCity;
        set
        {
            _selectedCity = value;
            OnPropertyChanged();
            _ = LoadWeatherAsync();   // zmiana zaznaczenia uruchamia pobranie pogody
        }
    }

    public Weather? Weather
    {
        get => _weather;
        set
        {
            _weather = value;
            OnPropertyChanged();
        }
    }

    public bool IsBusy
    {
        get => _isBusy;
        set
        {
            _isBusy = value;
            OnPropertyChanged();
        }
    }

    public string? ErrorMessage
    {
        get => _errorMessage;
        set
        {
            _errorMessage = value;
            OnPropertyChanged();
        }
    }

    public ICommand LoadCitiesCommand { get; }

    public MainViewModelClassic(IMeteoService meteoService)
    {
        _meteoService = meteoService;

        // Komenda: co wykonać + kiedy przycisk ma być aktywny
        LoadCitiesCommand = new RelayCommand(
            execute: _ => _ = LoadCitiesAsync(),
            canExecute: _ => !IsBusy && !string.IsNullOrWhiteSpace(CityName));
    }

    private async Task LoadCitiesAsync()
    {
        IsBusy = true;
        ErrorMessage = null;

        try
        {
            Cities = await _meteoService.GetLocationsAsync(CityName);
            if (Cities.Length == 0)
            {
                ErrorMessage = "Nie znaleziono miasta o takiej nazwie.";
            }
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

    private async Task LoadWeatherAsync()
    {
        if (SelectedCity is null)
        {
            return;
        }

        try
        {
            Weather = await _meteoService.GetCurrentWeatherAsync(SelectedCity.Latitude, SelectedCity.Longitude);
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Błąd połączenia: {ex.Message}";
        }
    }
}
```

💡 **Wyjaśniam** (pokazuję kursorem, od góry):

1. **`INotifyPropertyChanged`**: „Jeden interfejs, jedno zdarzenie `PropertyChanged`. Binding WPF subskrybuje je przy starcie i po każdym `OnPropertyChanged("Weather")` odczytuje `Weather` na nowo. Bez tego binding pokaże wartość początkową i nigdy jej nie odświeży”.
2. **`[CallerMemberName]`**: „Atrybut z `System.Runtime.CompilerServices`. Kompilator wstawia w miejsce parametru nazwę składowej, z której wołamy metodę. Wołam `OnPropertyChanged()` w setterze `CityName`, a do zdarzenia trafia `"CityName"`. Zero literówek w napisach”.
3. **Pole + właściwość**: „Każda właściwość to 8 linii: pole, getter, setter z powiadomieniem. Sześć właściwości = 50 linii, które wyglądają tak samo. Zapamiętajcie ten ból, w bloku 5 zniknie”.
4. **`SelectedCity`**: „Setter robi coś więcej: po powiadomieniu uruchamia `LoadWeatherAsync()`. Wybór na liście = pobranie pogody; wczoraj to była obsługa `SelectionChanged`. `_ = ...` to **discard**: mówimy kompilatorowi »wiem, że nie czekam na ten `Task`«. Alternatywa `async void` jest gorsza: wyjątek z `async void` zabija aplikację, a z `Task` możemy go złapać w środku metody”.
5. **Komenda**: „`LoadCitiesCommand` dostaje dwie lambdy: `execute` uruchamia pobieranie, `canExecute` zwraca `false`, gdy trwa pobieranie albo pole jest puste. Przycisk w oknie sam to odczyta”.
6. **`IsBusy` i `ErrorMessage`**: „Wczoraj było `btnSearch.IsEnabled = false` i `tbStatus.Text = ...`. Dziś ViewModel ustawia **stan** (`IsBusy = true`), a jak go pokazać, decyduje widok: wyłączony przycisk, pasek postępu, kursor. ViewModel nie zna żadnej kontrolki, więc da się go przetestować w konsoli”.
7. **`catch (Exception)`**: „Łapiemy szerzej niż wczoraj (`HttpRequestException`), bo serwis za interfejsem może rzucić cokolwiek: błąd sieci, timeout (`TaskCanceledException`), błąd JSON. ViewModel zamienia każdy na komunikat”.

### 2.5 Tymczasowe podpięcie w code‑behind

⌨️ **Robię:** zastępuję `MainWindow.xaml.cs` w całości (zmienia się wszystko, więc bez bloku 🟩):

```csharp
// P03WeatherForecastWPF.Client/MainWindow.xaml.cs  (wersja tymczasowa – pełna w bloku 7)
using System.Windows;
using P03WeatherForecastWPF.Client.Services;
using P03WeatherForecastWPF.Client.ViewModels.Classic;

namespace P03WeatherForecastWPF.Client;

/// <summary>
/// Wersja tymczasowa (etap 2): ViewModel tworzymy ręcznie w code-behind.
/// W etapie 7 zrobi to za nas kontener Dependency Injection.
/// </summary>
public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();

        // FakeMeteoService zwraca stałe dane – nie potrzebuje HttpClient ani konfiguracji,
        // więc etap "ręcznego" MVVM kompiluje się i działa bez internetu.
        DataContext = new MainViewModelClassic(new FakeMeteoService());
    }
}
```

🗣️ **Mówię:** „91 linii zamieniło się w jedną: `DataContext = viewModel`. `DataContext` to obiekt, do którego odnoszą się wszystkie `{Binding}` w tym oknie i jego dzieciach. Celowo wstawiam `FakeMeteoService`: w wersji końcowej `OpenMeteoService` będzie wymagał `IOptions<AppSettings>`, którego jeszcze nie mamy, a nie chcę, żeby etap pośredni się nie kompilował. ViewModel i tak nie widzi różnicy, bo zna tylko interfejs. Na tym polega DI, choć na razie robimy je ręcznie, słowem `new`”.

> ⚠️ Po tym kroku projekt **nie zbuduje się**: `MainWindow.xaml` nadal odwołuje się do `btnSearch_Click` i `lbCities_SelectionChanged`, których już nie ma. Naprawiamy to w bloku 3. Jeśli grupa chce zobaczyć zielony build od razu, pokaż błąd kompilatora (`does not contain a definition for 'btnSearch_Click'`) i powiedz, że to dobrze: kompilator pilnuje, żeby widok nie wisiał na usuniętym kodzie.

🔗 **Przejście:** „Okno ma teraz ViewModel, ale żadna kontrolka jeszcze o nim nie wie. Pora na binding”.

---

## 3. P03: binding w XAML

**Cel:** uczestnicy potrafią zbindować tekst, listę, zaznaczenie, widoczność i komendę; wiedzą, jak znaleźć błąd bindingu.

⌨️ **Robię:** zastępuję `MainWindow.xaml` w całości (wszystkie kontrolki dostają binding, więc bez bloku 🟩):

```xml
<!-- P03WeatherForecastWPF.Client/MainWindow.xaml  (wersja tymczasowa – pełna w bloku 6) -->
<Window x:Class="P03WeatherForecastWPF.Client.MainWindow"
        xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
        xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
        xmlns:d="http://schemas.microsoft.com/expression/blend/2008"
        xmlns:mc="http://schemas.openxmlformats.org/markup-compatibility/2006"
        xmlns:local="clr-namespace:P03WeatherForecastWPF.Client"
        xmlns:vm="clr-namespace:P03WeatherForecastWPF.Client.ViewModels.Classic"
        mc:Ignorable="d"
        d:DataContext="{d:DesignInstance Type=vm:MainViewModelClassic}"
        Title="P03 – Pogoda (MVVM)" Height="460" Width="680">

    <!-- d:DataContext podpowiada IntelliSense nazwy właściwości ViewModelu w XAML (tylko w czasie projektowania) -->

    <Window.Resources>
        <!-- Wbudowany konwerter bool → Visibility. Własne konwertery dodamy w etapie 6. -->
        <BooleanToVisibilityConverter x:Key="BoolToVisibility" />
    </Window.Resources>

    <DockPanel Margin="8">

        <!-- Lewa kolumna: wyszukiwanie miasta -->
        <StackPanel DockPanel.Dock="Left" Width="280" Margin="0,0,8,0">
            <Label Content="Miasto:" />

            <!-- UpdateSourceTrigger=PropertyChanged: ViewModel dostaje tekst po każdej literze,
                 a nie dopiero po opuszczeniu pola (domyślnie LostFocus) -->
            <TextBox Text="{Binding CityName, UpdateSourceTrigger=PropertyChanged}">
                <TextBox.InputBindings>
                    <KeyBinding Key="Enter" Command="{Binding LoadCitiesCommand}" />
                </TextBox.InputBindings>
            </TextBox>

            <!-- Command zamiast Click: przycisk sam się wyłącza, gdy CanExecute zwraca false -->
            <Button Content="Szukaj" Command="{Binding LoadCitiesCommand}" Margin="0,4" />

            <ProgressBar Height="6" IsIndeterminate="True"
                         Visibility="{Binding IsBusy, Converter={StaticResource BoolToVisibility}}" />

            <ListBox Height="260"
                     ItemsSource="{Binding Cities}"
                     SelectedItem="{Binding SelectedCity}"
                     DisplayMemberPath="FullName" />
        </StackPanel>

        <!-- Prawa część: pogoda dla wybranego miasta -->
        <StackPanel>
            <Label Content="Wybrane miasto:" FontWeight="Bold" />
            <Label Content="{Binding SelectedCity.FullName, FallbackValue=—}" />

            <Label Content="Temperatura:" FontWeight="Bold" />
            <Label Content="{Binding Weather.Temperature, StringFormat={}{0:F1} °C, FallbackValue=—}" FontSize="28" />

            <Label Content="Wiatr:" FontWeight="Bold" />
            <Label Content="{Binding Weather.WindSpeed, StringFormat={}{0:F0} km/h, FallbackValue=—}" />

            <Label Content="Opis:" FontWeight="Bold" />
            <Label Content="{Binding Weather.Description, FallbackValue=—}" />

            <Label Content="Czas pomiaru:" FontWeight="Bold" />
            <Label Content="{Binding Weather.Time, StringFormat=g, FallbackValue=—}" />

            <!-- Komunikat o błędzie: pusty tekst, gdy ErrorMessage jest null -->
            <TextBlock Text="{Binding ErrorMessage}" Foreground="DarkRed" TextWrapping="Wrap" Margin="4" />
        </StackPanel>
    </DockPanel>
</Window>
```

👀 **Pokazuję:** zniknęły wszystkie `x:Name`, `Click` i `SelectionChanged`. Każda kontrolka mówi tylko, **z czym** jest związana, nie **co robić**.

💡 **Wyjaśniam** po kolei (tabela na tablicy):

| Składnia | Co robi | Komentarz |
|---|---|---|
| `{Binding CityName}` | łączy właściwość kontrolki z właściwością `CityName` obiektu z `DataContext` | `DataContext` dziedziczy się w dół drzewa: ustawiony na oknie, działa w każdym dziecku |
| `Mode` (nie piszemy) | kierunek: `OneWay` (VM → widok), `TwoWay` (oba), `OneTime` | `TextBox.Text`, `SelectedItem` są domyślnie `TwoWay`; `Label.Content` jest `OneWay` |
| `UpdateSourceTrigger=PropertyChanged` | kiedy widok zapisuje do VM: po każdej literze zamiast po opuszczeniu pola (`LostFocus`) | bez tego `CanExecute` zobaczy pusty tekst dopiero po kliknięciu gdzie indziej |
| `ItemsSource="{Binding Cities}"` + `SelectedItem="{Binding SelectedCity}"` | lista bierze elementy z kolekcji, a zaznaczony element zapisuje do VM | to zastępuje `lbCities.ItemsSource = cities` i `SelectionChanged` |
| `SelectedCity.FullName` | ścieżka przez kilka właściwości | gdy `SelectedCity` jest `null`, binding nie rzuca wyjątku, tylko loguje błąd i pokazuje `FallbackValue` |
| `StringFormat={}{0:F1} °C` | formatowanie jak w `string.Format` | `{}` na początku to „escape”: bez niego XAML uznałby `{0:F1}` za rozszerzenie znaczników. `StringFormat=g` dla daty nie potrzebuje `{}` |
| `FallbackValue=—` | tekst, gdy ścieżka bindingu nie działa (np. `Weather` jest `null`) | wczoraj `—` było wpisane w `Content` |
| `Command="{Binding LoadCitiesCommand}"` | przycisk woła `Execute`, a `IsEnabled` bierze z `CanExecute` | nie ma `Click`; przycisk wyłączony = `CanExecute` zwrócił `false` |
| `KeyBinding Key="Enter"` | ta sama komenda z klawiatury | w code‑behind byłoby `KeyDown` z `if (e.Key == Key.Enter)` |
| `Converter={StaticResource BoolToVisibility}` | `bool` → `Visibility`, bo `ProgressBar.Visibility` nie przyjmie `true` | wbudowany `BooleanToVisibilityConverter`; własne piszemy w bloku 6 |
| `d:DataContext="{d:DesignInstance Type=vm:MainViewModelClassic}"` | tylko dla edytora: IntelliSense podpowiada nazwy właściwości i podkreśla literówki | prefiks `d:` jest ignorowany w czasie działania (`mc:Ignorable="d"`) |

⌨️ **Robię:** F5. Wpisuję `Warszawa` → **Szukaj** (albo Enter). 👀 **Pokazuję:** pasek postępu przez pół sekundy, lista z jednym wynikiem (dane z `FakeMeteoService`), po kliknięciu losowa temperatura. Kasuję tekst w polu: przycisk **Szukaj** gaśnie. Wpisuję `a`: trzy miasta.

⌨️ **Robię** (błąd bindingu, 3 min): zmieniam w XAML `{Binding CityName}` na `{Binding CityNam}`, F5, otwieram okno **Output**.

👀 **Pokazuję:** linia `System.Windows.Data Error: 40 : BindingExpression path error: 'CityNam' property not found on 'object' ''MainViewModelClassic'`. 🗣️ **Mówię:** „Binding nie rzuca wyjątku, tylko loguje. Aplikacja działa, ale pole jest martwe. Gdy coś »nie odświeża się«, pierwsze miejsce do sprawdzenia to okno Output. Druga ochrona to `d:DataContext`: z nim edytor podkreśli `CityNam` na niebiesko”. Przywracam `CityName`.

🔧 **Ulepszenie:** w edycjach 26/31 `CityName` w ViewModelu nie wywoływał `OnPropertyChanged`, `TextBox` był bez `UpdateSourceTrigger`, nie było `FallbackValue`, `KeyBinding` ani `d:DataContext`, a `Button` nie miał `CanExecute` (klik z pustym polem wysyłał zapytanie o pusty napis). Tutaj okno reaguje na każdą literę, a przycisk wie, kiedy ma być wyłączony.

✅ **Efekt:** okno działa na danych testowych; `MainWindow.xaml.cs` ma jedną linię logiki; w Output nie ma linii `System.Windows.Data Error`.

🔗 **Przejście:** „Działa na danych na niby. Podłączamy prawdziwy serwis, ale tak, żeby ViewModel tego nie zauważył”.

---

## 4. P03: `OpenMeteoService` za interfejsem i wzorzec Options

**Cel:** prawdziwy serwis implementuje `IMeteoService`; ustawienia wchodzą jako silnie typowany obiekt `AppSettings` zamiast tekstowego klucza.

### 4.1 Klasa ustawień

⌨️ **Robię:** folder `Configuration` → klasa `AppSettings.cs`:

```csharp
// P03WeatherForecastWPF.Client/Configuration/AppSettings.cs
namespace P03WeatherForecastWPF.Client.Configuration;

/// <summary>
/// Klasa odpowiadająca sekcji "AppSettings" w appsettings.json.
/// Wzorzec Options: zamiast czytać configuration["AppSettings:DefaultLanguage"]
/// dostajemy silnie typowany obiekt przez IOptions&lt;AppSettings&gt;.
/// </summary>
public class AppSettings
{
    /// <summary>Nazwa sekcji w pliku konfiguracyjnym.</summary>
    public const string SectionName = "AppSettings";

    /// <summary>Język nazw miast zwracanych przez Open-Meteo (np. "pl", "en").</summary>
    public string DefaultLanguage { get; set; } = "en";
}
```

💡 **Wyjaśniam:** „Wczoraj czytaliśmy `configuration["AppSettings:DefaultLanguage"]`: napis, literówka wychodzi dopiero w czasie działania. **Wzorzec Options**: klasa, której właściwości odpowiadają kluczom w sekcji JSON. Kontener zbinduje sekcję do obiektu i poda go jako `IOptions<AppSettings>`. `SectionName` jako stała, żeby nazwa sekcji była w jednym miejscu. Wartość domyślna `"en"` działa, gdy klucza nie ma w pliku”.

### 4.2 Serwis

🟩 **Co dopisujesz** w `P03WeatherForecastWPF.Client/Services/OpenMeteoService.cs`:

```diff
⋯
 using System.Net.Http;
 using System.Net.Http.Json;
+using Microsoft.Extensions.Options;
+using P03WeatherForecastWPF.Client.Configuration;
 using P03WeatherForecastWPF.Client.Models;
 
⋯
 /// Dokumentacja: https://open-meteo.com/en/docs i https://open-meteo.com/en/docs/geocoding-api
 /// </summary>
-public class OpenMeteoService
+public class OpenMeteoService : IMeteoService
 {
     private const string GeocodingBaseUrl = "https://geocoding-api.open-meteo.com/v1/search";
⋯
     private readonly string _language;
 
-    /// <param name="httpClient">Klient HTTP – przekazujemy z zewnątrz, żeby nie tworzyć go w każdym serwisie.</param>
-    /// <param name="language">Język nazw miast, np. "pl" lub "en".</param>
-    public OpenMeteoService(HttpClient httpClient, string language)
+    /// <param name="httpClient">Klient HTTP dostarczany przez IHttpClientFactory (AddHttpClient w App.xaml.cs).</param>
+    /// <param name="options">Sekcja AppSettings z appsettings.json, zbindowana przez kontener DI.</param>
+    public OpenMeteoService(HttpClient httpClient, IOptions<AppSettings> options)
     {
         _httpClient = httpClient;
-        _language = language;
+        _language = options.Value.DefaultLanguage;
     }
 
```

```csharp
// P03WeatherForecastWPF.Client/Services/OpenMeteoService.cs
using System.Globalization;
using System.Net.Http;
using System.Net.Http.Json;
using Microsoft.Extensions.Options;
using P03WeatherForecastWPF.Client.Configuration;
using P03WeatherForecastWPF.Client.Models;

namespace P03WeatherForecastWPF.Client.Services;

/// <summary>
/// Komunikacja z Open-Meteo: wyszukiwanie miast (geocoding) i aktualna pogoda.
/// Dokumentacja: https://open-meteo.com/en/docs i https://open-meteo.com/en/docs/geocoding-api
/// </summary>
public class OpenMeteoService : IMeteoService
{
    private const string GeocodingBaseUrl = "https://geocoding-api.open-meteo.com/v1/search";
    private const string ForecastBaseUrl = "https://api.open-meteo.com/v1/forecast";

    private readonly HttpClient _httpClient;
    private readonly string _language;

    /// <param name="httpClient">Klient HTTP dostarczany przez IHttpClientFactory (AddHttpClient w App.xaml.cs).</param>
    /// <param name="options">Sekcja AppSettings z appsettings.json, zbindowana przez kontener DI.</param>
    public OpenMeteoService(HttpClient httpClient, IOptions<AppSettings> options)
    {
        _httpClient = httpClient;
        _language = options.Value.DefaultLanguage;
    }

    public async Task<City[]> GetLocationsAsync(string locationName)
    {
        string url = $"{GeocodingBaseUrl}?name={Uri.EscapeDataString(locationName)}&count=10&language={_language}&format=json";

        // GetFromJsonAsync pobiera odpowiedź i od razu deserializuje JSON do obiektu
        GeocodingResponse? response = await _httpClient.GetFromJsonAsync<GeocodingResponse>(url);

        return response?.Results?.ToArray() ?? [];
    }

    public async Task<Weather?> GetCurrentWeatherAsync(double latitude, double longitude)
    {
        // Współrzędne zawsze z kropką dziesiętną, niezależnie od polskich ustawień regionalnych
        string lat = latitude.ToString(CultureInfo.InvariantCulture);
        string lon = longitude.ToString(CultureInfo.InvariantCulture);

        string url = $"{ForecastBaseUrl}?latitude={lat}&longitude={lon}&current=temperature_2m,wind_speed_10m,weather_code&timezone=auto";

        ForecastResponse? response = await _httpClient.GetFromJsonAsync<ForecastResponse>(url);

        if (response?.Current is null)
        {
            return null;
        }

        // Mapowanie z modelu dostawcy (ForecastResponse) na nasz model (Weather)
        return new Weather
        {
            Time = DateTime.TryParse(response.Current.Time, CultureInfo.InvariantCulture, out DateTime time) ? time : DateTime.Now,
            Temperature = response.Current.Temperature2m,
            WindSpeed = response.Current.WindSpeed10m,
            WeatherCode = response.Current.WeatherCode
        };
    }
}
```

🗣️ **Mówię:** „Trzy zmiany: `: IMeteoService`, drugi parametr konstruktora to `IOptions<AppSettings>` zamiast `string`, a język bierzemy z `options.Value.DefaultLanguage`. Reszta bez zmian; logika pobierania jest ta sama, co wczoraj. `IOptions<T>` to opakowanie z jedną właściwością `Value`; istnieją też `IOptionsSnapshot<T>` (odczyt na nowo w każdym zakresie) i `IOptionsMonitor<T>` (powiadamia o zmianie pliku). Do ustawień czytanych raz wystarczy `IOptions`”.

### 4.3 Tymczasowe podpięcie prawdziwego serwisu

⌨️ **Robię:** w `MainWindow.xaml.cs` tworzę prawdziwy serwis ręcznie:

🟩 **Co dopisujesz** w `P03WeatherForecastWPF.Client/MainWindow.xaml.cs`:

```diff
⋯
+using System.Net.Http;
 using System.Windows;
+using Microsoft.Extensions.Options;
+using P03WeatherForecastWPF.Client.Configuration;
 using P03WeatherForecastWPF.Client.Services;
 using P03WeatherForecastWPF.Client.ViewModels.Classic;
⋯
 
 /// <summary>
-/// Wersja tymczasowa (etap 2): ViewModel tworzymy ręcznie w code-behind.
+/// Wersja tymczasowa (etap 4): prawdziwy serwis, ale wszystko nadal tworzymy ręcznie.
 /// W etapie 7 zrobi to za nas kontener Dependency Injection.
 /// </summary>
⋯
         InitializeComponent();
 
-        // FakeMeteoService zwraca stałe dane – nie potrzebuje HttpClient ani konfiguracji,
-        // więc etap "ręcznego" MVVM kompiluje się i działa bez internetu.
-        DataContext = new MainViewModelClassic(new FakeMeteoService());
+        // Options.Create opakowuje obiekt ustawień w IOptions<T> – tak samo zrobi kontener DI,
+        // tylko wartości weźmie z appsettings.json (etap 7). Na razie język na sztywno.
+        IOptions<AppSettings> settings = Options.Create(new AppSettings { DefaultLanguage = "pl" });
+
+        DataContext = new MainViewModelClassic(new OpenMeteoService(new HttpClient(), settings));
+
+        // Wersja bez internetu – ViewModel nie widzi różnicy:
+        // DataContext = new MainViewModelClassic(new FakeMeteoService());
     }
 }
```

```csharp
// P03WeatherForecastWPF.Client/MainWindow.xaml.cs  (wersja tymczasowa – pełna w bloku 7)
using System.Net.Http;
using System.Windows;
using Microsoft.Extensions.Options;
using P03WeatherForecastWPF.Client.Configuration;
using P03WeatherForecastWPF.Client.Services;
using P03WeatherForecastWPF.Client.ViewModels.Classic;

namespace P03WeatherForecastWPF.Client;

/// <summary>
/// Wersja tymczasowa (etap 4): prawdziwy serwis, ale wszystko nadal tworzymy ręcznie.
/// W etapie 7 zrobi to za nas kontener Dependency Injection.
/// </summary>
public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();

        // Options.Create opakowuje obiekt ustawień w IOptions<T> – tak samo zrobi kontener DI,
        // tylko wartości weźmie z appsettings.json (etap 7). Na razie język na sztywno.
        IOptions<AppSettings> settings = Options.Create(new AppSettings { DefaultLanguage = "pl" });

        DataContext = new MainViewModelClassic(new OpenMeteoService(new HttpClient(), settings));

        // Wersja bez internetu – ViewModel nie widzi różnicy:
        // DataContext = new MainViewModelClassic(new FakeMeteoService());
    }
}
```

💡 **Wyjaśniam:** „`Options.Create(obiekt)` to statyczna metoda z `Microsoft.Extensions.Options`, która opakowuje gotowy obiekt w `IOptions<T>`. Przydaje się w testach i właśnie tu: jeszcze nie mamy kontenera, a serwis chce `IOptions`. Zwróćcie uwagę, ile »ręcznego« tworzenia obiektów jest w tych trzech liniach: `HttpClient`, ustawienia, serwis, ViewModel. W bloku 7 wszystko to zniknie”.

⌨️ **Robię:** F5. `Warszawa` → 2 wyniki, klikam pierwszy: prawdziwa temperatura. `Kraków` → lista z Krakowem w Wisconsin. `xyzxyz` → „Nie znaleziono miasta o takiej nazwie”. Potem zamieniam komentarze (wersja z `FakeMeteoService`) i pokazuję, że okno działa tak samo.

✅ **Efekt:** prawdziwa pogoda w oknie; podmiana serwisu to zmiana jednej linii w konstruktorze okna, ViewModel i XAML nietknięte.

🔗 **Przejście:** „`MainViewModelClassic` ma 146 linii, z czego 60 to powtarzalne settery. Biblioteka CommunityToolkit.Mvvm wygeneruje je za nas w czasie kompilacji”.

---

## 5. P03: CommunityToolkit.Mvvm

**Cel:** ten sam ViewModel napisany z generatorem kodu; uczestnicy rozumieją, co generator dopisuje i gdzie to podejrzeć.

⌨️ **Robię:** w folderze `ViewModels` dodaję `MainViewModel.cs` (klasyczna wersja zostaje w `Classic` do porównania):

```csharp
// P03WeatherForecastWPF.Client/ViewModels/MainViewModel.cs
using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using P03WeatherForecastWPF.Client.Models;
using P03WeatherForecastWPF.Client.Services;

namespace P03WeatherForecastWPF.Client.ViewModels;

/// <summary>
/// ViewModel okna głównego w wersji z CommunityToolkit.Mvvm.
/// - ObservableObject daje INotifyPropertyChanged.
/// - [ObservableProperty] na właściwości partial generuje setter z powiadomieniem.
/// - [RelayCommand] na metodzie generuje właściwość XxxCommand (ICommand).
/// Klasa musi być partial, bo generator kodu dopisuje do niej drugą część.
/// </summary>
public partial class MainViewModel : ObservableObject
{
    private readonly IMeteoService _meteoService;

    public MainViewModel(IMeteoService meteoService)
    {
        _meteoService = meteoService;
    }

    /// <summary>Tekst z pola "Miasto". Zmiana odświeża CanExecute komendy LoadCities.</summary>
    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(LoadCitiesCommand))]
    public partial string CityName { get; set; } = "Warszawa";

    /// <summary>
    /// ObservableCollection powiadamia listę o dodaniu/usunięciu elementów.
    /// Tablica też by działała (podmieniamy całą), ale kolekcja jest standardem w MVVM.
    /// </summary>
    public ObservableCollection<City> Cities { get; } = [];

    [ObservableProperty]
    public partial City? SelectedCity { get; set; }

    [ObservableProperty]
    public partial Weather? Weather { get; set; }

    /// <summary>True podczas pobierania danych – blokuje przycisk i pokazuje pasek.</summary>
    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(LoadCitiesCommand))]
    public partial bool IsBusy { get; set; }

    [ObservableProperty]
    public partial string? ErrorMessage { get; set; }

    /// <summary>
    /// Generator tworzy LoadCitiesCommand typu IAsyncRelayCommand.
    /// CanExecute wskazuje metodę, która mówi, kiedy przycisk jest aktywny.
    /// </summary>
    [RelayCommand(CanExecute = nameof(CanLoadCities))]
    private async Task LoadCitiesAsync()
    {
        IsBusy = true;
        ErrorMessage = null;

        try
        {
            City[] cities = await _meteoService.GetLocationsAsync(CityName);

            Cities.Clear();
            foreach (City city in cities)
            {
                Cities.Add(city);
            }

            if (cities.Length == 0)
            {
                ErrorMessage = "Nie znaleziono miasta o takiej nazwie.";
            }
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

    private bool CanLoadCities() => !IsBusy && !string.IsNullOrWhiteSpace(CityName);

    /// <summary>
    /// Generator wywołuje tę metodę po każdej zmianie SelectedCity (hook "partial void On...Changed").
    /// Dzięki temu wybór miasta na liście od razu pobiera pogodę.
    /// </summary>
    partial void OnSelectedCityChanged(City? value)
    {
        _ = LoadWeatherAsync(value);
    }

    private async Task LoadWeatherAsync(City? city)
    {
        if (city is null)
        {
            Weather = null;
            return;
        }

        ErrorMessage = null;

        try
        {
            Weather = await _meteoService.GetCurrentWeatherAsync(city.Latitude, city.Longitude);

            if (Weather is null)
            {
                ErrorMessage = "Brak danych pogodowych dla tej lokalizacji.";
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Błąd połączenia: {ex.Message}";
        }
    }
}
```

💡 **Wyjaśniam** element po elemencie:

| Element | Co robi | Odpowiednik w `MainViewModelClassic` |
|---|---|---|
| `: ObservableObject` | gotowa implementacja `INotifyPropertyChanged` z `OnPropertyChanged`, `SetProperty` | zdarzenie + metoda `OnPropertyChanged` |
| `partial class` | generator kodu (source generator) dopisuje drugą część klasy w czasie kompilacji | – |
| `[ObservableProperty] public partial string CityName { get; set; }` | **właściwość częściowa** (partial property, C# 13, toolkit 8.4): my deklarujemy nagłówek, generator dopisuje pole, getter i setter z powiadomieniem | 8 linii: pole `_cityName`, getter, setter z `OnPropertyChanged()` |
| `[NotifyCanExecuteChangedFor(nameof(LoadCitiesCommand))]` | po zmianie tej właściwości komenda podnosi `CanExecuteChanged` | `CommandManager.RequerySuggested` (WPF pytał „na wszelki wypadek” przy każdym kliknięciu) |
| `[RelayCommand(CanExecute = nameof(CanLoadCities))]` na `LoadCitiesAsync` | generator tworzy właściwość `LoadCitiesCommand` typu `IAsyncRelayCommand`; końcówka `Async` jest obcinana | `new RelayCommand(execute, canExecute)` w konstruktorze |
| `partial void OnSelectedCityChanged(City? value)` | „hak” wołany przez wygenerowany setter po zmianie wartości | kod w setterze `SelectedCity` po `OnPropertyChanged()` |
| `ObservableCollection<City> Cities { get; } = []` | kolekcja, która sama powiadamia listę o `Add`/`Clear`; właściwość tylko do odczytu, bo podmieniamy zawartość, nie obiekt | `City[]` z powiadomieniem przy podmianie tablicy |

🗣️ **Mówię:** „`[ObservableProperty]` na właściwości `partial` to nowa składnia. Do wersji 8.3 toolkitu atrybut stawiało się na **polu** z małej litery: `[ObservableProperty] private string cityName;`, a generator tworzył właściwość `CityName`. Działało, ale myliło: w kodzie pisało się `cityName` (pole, bez powiadomienia) albo `CityName` (właściwość) i łatwo było o błąd. Tak było w edycjach 26 i 31. Od C# 13 język ma właściwości częściowe i toolkit 8.4 z nich korzysta: deklarujemy `public partial string CityName { get; set; }`, a generator dopisuje implementację. Jedna nazwa, zero pola. Wymaga .NET 9+ (my mamy 10)”.

👀 **Pokazuję** wygenerowany kod: Solution Explorer → P03 → **Dependencies → Analyzers → CommunityToolkit.Mvvm.SourceGenerators** → `P03WeatherForecastWPF.Client.ViewModels.MainViewModel.g.cs` (osobne pliki dla właściwości i komend). „To jest dokładnie to, co napisaliśmy ręcznie w bloku 2: setter z `OnPropertyChanging`, `OnPropertyChanged`, wywołanie `OnSelectedCityChanged`, `LoadCitiesCommand` jako `AsyncRelayCommand`. Nic magicznego, tylko mniej pisania”.

💡 **Wyjaśniam** (porównanie, tabela na tablicy): `MainViewModelClassic` + `RelayCommand` = 146 + 36 linii; `MainViewModel` = 120 linii **z komentarzami** (bez nich ok. 80). „Różnica rośnie z każdą właściwością. Ważniejsze: w wersji toolkit nie da się zapomnieć `OnPropertyChanged`”.

🗣️ **Mówię** (wady, alternatywy): „Generator to zależność od biblioteki i od wersji C#. Błędy w atrybutach dostaniecie jako diagnostyki `MVVMTK0xxx`, trzeba je umieć czytać. Alternatywy: Prism, ReactiveUI (bardziej »reaktywne«, trudniejsze), DevExpress MVVM. CommunityToolkit jest od Microsoftu, działa w WPF, MAUI, WinUI, Avalonii i Blazorze, dlatego jest naszym wyborem na resztę kursu”.

### 5.1 Przełączenie okna na nowy ViewModel

⌨️ **Robię:** w `MainWindow.xaml.cs` zamieniam `MainViewModelClassic` na `MainViewModel`:

🟩 **Co dopisujesz** w `P03WeatherForecastWPF.Client/MainWindow.xaml.cs`:

```diff
⋯
 using P03WeatherForecastWPF.Client.Configuration;
 using P03WeatherForecastWPF.Client.Services;
-using P03WeatherForecastWPF.Client.ViewModels.Classic;
+using P03WeatherForecastWPF.Client.ViewModels;
 
 namespace P03WeatherForecastWPF.Client;
 
 /// <summary>
-/// Wersja tymczasowa (etap 4): prawdziwy serwis, ale wszystko nadal tworzymy ręcznie.
+/// Wersja tymczasowa (etap 5): ViewModel z CommunityToolkit.Mvvm, nadal tworzony ręcznie.
 /// W etapie 7 zrobi to za nas kontener Dependency Injection.
 /// </summary>
⋯
         InitializeComponent();
 
-        // Options.Create opakowuje obiekt ustawień w IOptions<T> – tak samo zrobi kontener DI,
-        // tylko wartości weźmie z appsettings.json (etap 7). Na razie język na sztywno.
         IOptions<AppSettings> settings = Options.Create(new AppSettings { DefaultLanguage = "pl" });
 
-        DataContext = new MainViewModelClassic(new OpenMeteoService(new HttpClient(), settings));
+        DataContext = new MainViewModel(new OpenMeteoService(new HttpClient(), settings));
 
         // Wersja bez internetu – ViewModel nie widzi różnicy:
-        // DataContext = new MainViewModelClassic(new FakeMeteoService());
+        // DataContext = new MainViewModel(new FakeMeteoService());
     }
 }
```

```csharp
// P03WeatherForecastWPF.Client/MainWindow.xaml.cs  (wersja tymczasowa – pełna w bloku 7)
using System.Net.Http;
using System.Windows;
using Microsoft.Extensions.Options;
using P03WeatherForecastWPF.Client.Configuration;
using P03WeatherForecastWPF.Client.Services;
using P03WeatherForecastWPF.Client.ViewModels;

namespace P03WeatherForecastWPF.Client;

/// <summary>
/// Wersja tymczasowa (etap 5): ViewModel z CommunityToolkit.Mvvm, nadal tworzony ręcznie.
/// W etapie 7 zrobi to za nas kontener Dependency Injection.
/// </summary>
public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();

        IOptions<AppSettings> settings = Options.Create(new AppSettings { DefaultLanguage = "pl" });

        DataContext = new MainViewModel(new OpenMeteoService(new HttpClient(), settings));

        // Wersja bez internetu – ViewModel nie widzi różnicy:
        // DataContext = new MainViewModel(new FakeMeteoService());
    }
}
```

W `MainWindow.xaml` zmieniam tylko przestrzeń nazw dla `d:DataContext`:

🟩 **Co dopisujesz** w `P03WeatherForecastWPF.Client/MainWindow.xaml`:

```diff
⋯
         xmlns:mc="http://schemas.openxmlformats.org/markup-compatibility/2006"
         xmlns:local="clr-namespace:P03WeatherForecastWPF.Client"
-        xmlns:vm="clr-namespace:P03WeatherForecastWPF.Client.ViewModels.Classic"
+        xmlns:vm="clr-namespace:P03WeatherForecastWPF.Client.ViewModels"
         mc:Ignorable="d"
-        d:DataContext="{d:DesignInstance Type=vm:MainViewModelClassic}"
+        d:DataContext="{d:DesignInstance Type=vm:MainViewModel}"
         Title="P03 – Pogoda (MVVM)" Height="460" Width="680">
 
```

```xml
<!-- P03WeatherForecastWPF.Client/MainWindow.xaml  (wersja tymczasowa – pełna w bloku 6) -->
<Window x:Class="P03WeatherForecastWPF.Client.MainWindow"
        xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
        xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
        xmlns:d="http://schemas.microsoft.com/expression/blend/2008"
        xmlns:mc="http://schemas.openxmlformats.org/markup-compatibility/2006"
        xmlns:local="clr-namespace:P03WeatherForecastWPF.Client"
        xmlns:vm="clr-namespace:P03WeatherForecastWPF.Client.ViewModels"
        mc:Ignorable="d"
        d:DataContext="{d:DesignInstance Type=vm:MainViewModel}"
        Title="P03 – Pogoda (MVVM)" Height="460" Width="680">

    <!-- d:DataContext podpowiada IntelliSense nazwy właściwości ViewModelu w XAML (tylko w czasie projektowania) -->

    <Window.Resources>
        <!-- Wbudowany konwerter bool → Visibility. Własne konwertery dodamy w etapie 6. -->
        <BooleanToVisibilityConverter x:Key="BoolToVisibility" />
    </Window.Resources>

    <DockPanel Margin="8">

        <!-- Lewa kolumna: wyszukiwanie miasta -->
        <StackPanel DockPanel.Dock="Left" Width="280" Margin="0,0,8,0">
            <Label Content="Miasto:" />

            <!-- UpdateSourceTrigger=PropertyChanged: ViewModel dostaje tekst po każdej literze,
                 a nie dopiero po opuszczeniu pola (domyślnie LostFocus) -->
            <TextBox Text="{Binding CityName, UpdateSourceTrigger=PropertyChanged}">
                <TextBox.InputBindings>
                    <KeyBinding Key="Enter" Command="{Binding LoadCitiesCommand}" />
                </TextBox.InputBindings>
            </TextBox>

            <!-- Command zamiast Click: przycisk sam się wyłącza, gdy CanExecute zwraca false -->
            <Button Content="Szukaj" Command="{Binding LoadCitiesCommand}" Margin="0,4" />

            <ProgressBar Height="6" IsIndeterminate="True"
                         Visibility="{Binding IsBusy, Converter={StaticResource BoolToVisibility}}" />

            <ListBox Height="260"
                     ItemsSource="{Binding Cities}"
                     SelectedItem="{Binding SelectedCity}"
                     DisplayMemberPath="FullName" />
        </StackPanel>

        <!-- Prawa część: pogoda dla wybranego miasta -->
        <StackPanel>
            <Label Content="Wybrane miasto:" FontWeight="Bold" />
            <Label Content="{Binding SelectedCity.FullName, FallbackValue=—}" />

            <Label Content="Temperatura:" FontWeight="Bold" />
            <Label Content="{Binding Weather.Temperature, StringFormat={}{0:F1} °C, FallbackValue=—}" FontSize="28" />

            <Label Content="Wiatr:" FontWeight="Bold" />
            <Label Content="{Binding Weather.WindSpeed, StringFormat={}{0:F0} km/h, FallbackValue=—}" />

            <Label Content="Opis:" FontWeight="Bold" />
            <Label Content="{Binding Weather.Description, FallbackValue=—}" />

            <Label Content="Czas pomiaru:" FontWeight="Bold" />
            <Label Content="{Binding Weather.Time, StringFormat=g, FallbackValue=—}" />

            <!-- Komunikat o błędzie: pusty tekst, gdy ErrorMessage jest null -->
            <TextBlock Text="{Binding ErrorMessage}" Foreground="DarkRed" TextWrapping="Wrap" Margin="4" />
        </StackPanel>
    </DockPanel>
</Window>
```

⌨️ **Robię:** F5. Te same testy: `Warszawa`, `Kraków`, `xyzxyz`, puste pole.

👀 **Pokazuję:** XAML nie zmienił ani jednego bindingu, bo nazwy właściwości i komendy są takie same. Lista działa z `ObservableCollection` tak samo jak z tablicą.

🔧 **Ulepszenie:** edycje 26/31 miały `[RelayCommand] public async void LoadCities()` (wyjątek z `async void` wywraca aplikację, brak `CanExecute`), `[ObservableProperty]` na polach z małej litery, a `Cities` raz jako `ObservableCollection` podmienianą w całości, raz z `Clear`/`Add`. Tu: `private async Task` + `CanExecute`, właściwości częściowe, kolekcja tylko do odczytu, `IsBusy` i `ErrorMessage` z `try/catch/finally`.

✅ **Efekt:** okno działa identycznie jak w bloku 4, ViewModel jest krótszy, a klasyczna wersja zostaje w `ViewModels/Classic` jako materiał porównawczy (nie jest nigdzie używana, ale się kompiluje).

🔗 **Przejście:** „Temperatura w XAML ma `StringFormat`. A co, gdy formatowanie jest bardziej złożone, np. `null` ma ukryć kontrolkę? Konwertery”.

---

## 6. P03: konwertery i zasoby aplikacji

**Cel:** `IValueConverter` jako most między typem w ViewModelu a typem, którego oczekuje kontrolka; zasoby na poziomie aplikacji.

⌨️ **Robię:** folder `Converters`, dwa pliki:

```csharp
// P03WeatherForecastWPF.Client/Converters/TemperatureToDisplayConverter.cs
using System.Globalization;
using System.Windows.Data;

namespace P03WeatherForecastWPF.Client.Converters;

/// <summary>
/// Konwerter bindingu: liczba (double) → tekst "12,5 °C".
/// ViewModel trzyma czystą liczbę, a formatowanie należy do widoku.
/// </summary>
public class TemperatureToDisplayConverter : IValueConverter
{
    /// <summary>Kierunek ViewModel → widok.</summary>
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is double temperature)
        {
            return $"{temperature.ToString("F1", culture)} °C";
        }

        return "—";
    }

    /// <summary>Kierunek widok → ViewModel. Nie używamy (binding jest jednokierunkowy).</summary>
    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        throw new NotSupportedException();
}
```

```csharp
// P03WeatherForecastWPF.Client/Converters/NullToVisibilityConverter.cs
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace P03WeatherForecastWPF.Client.Converters;

/// <summary>
/// null lub pusty tekst → Collapsed, cokolwiek innego → Visible.
/// Używany do pokazywania komunikatu o błędzie tylko wtedy, gdy jakiś jest.
/// </summary>
public class NullToVisibilityConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        value is null || value is string { Length: 0 } ? Visibility.Collapsed : Visibility.Visible;

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        throw new NotSupportedException();
}
```

💡 **Wyjaśniam:**

- „`IValueConverter` ma dwie metody: `Convert` (ViewModel → widok) i `ConvertBack` (widok → ViewModel). Nasze bindingi są jednokierunkowe, więc `ConvertBack` rzuca `NotSupportedException`: lepiej głośno, niż cicho zwrócić coś dziwnego”.
- „`culture` to kultura z bindingu; `ToString("F1", culture)` da `12,5` na polskim Windowsie i `12.5` na angielskim. Formatowanie dla **człowieka** może zależeć od kultury; parsowanie danych z API (wczoraj) nie”.
- „`value is string { Length: 0 }` to wzorzec właściwości (property pattern, C# 8): dopasuj, gdy `value` jest napisem o długości zero”.
- „Kiedy konwerter, a kiedy właściwość w ViewModelu? Jeśli to czysta prezentacja (format, kolor, widoczność) – konwerter, bo ViewModel ma zostać niezależny od WPF. Jeśli to logika (np. »czy pokazać ostrzeżenie o mrozie«) – właściwość `bool` w ViewModelu, a w widoku tylko `BoolToVisibility`. Trzecia droga to `DataTrigger` w stylu; działa bez kodu C#, ale jest rozwlekła”.

⌨️ **Robię:** rejestruję konwertery jako zasoby aplikacji i dodaję dwa proste style:

```xml
<!-- P03WeatherForecastWPF.Client/App.xaml  (wersja tymczasowa – pełna w bloku 7) -->
<Application x:Class="P03WeatherForecastWPF.Client.App"
             xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
             xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
             xmlns:local="clr-namespace:P03WeatherForecastWPF.Client"
             xmlns:converters="clr-namespace:P03WeatherForecastWPF.Client.Converters"
             StartupUri="MainWindow.xaml">
    <Application.Resources>

        <!-- Konwertery jako zasoby aplikacji: jedna instancja, dostępna w każdym oknie -->
        <converters:TemperatureToDisplayConverter x:Key="TemperatureToDisplay" />
        <converters:NullToVisibilityConverter x:Key="NullToVisibility" />
        <BooleanToVisibilityConverter x:Key="BoolToVisibility" />

        <Style TargetType="Button">
            <Setter Property="Padding" Value="8,4" />
            <Setter Property="Margin" Value="0,4" />
        </Style>

        <Style TargetType="Label">
            <Setter Property="Padding" Value="4,2" />
        </Style>

    </Application.Resources>
</Application>
```

🗣️ **Mówię:** „`xmlns:converters` wskazuje przestrzeń nazw C#. Konwerter jako zasób to **jedna instancja** na aplikację, dostępna w każdym oknie przez `StaticResource`. Wczoraj zasoby były w `Window.Resources`; dziś przenosimy je wyżej, bo w dniu 13 dojdą kolejne okna. Style `Button` i `Label` bez klucza (niejawne) zastępują `Margin="0,4"` wpisywane przy każdym przycisku”.

⌨️ **Robię:** końcowa wersja okna – podmieniam `StringFormat` temperatury na konwerter, chowam komunikat, gdy jest pusty, i usuwam zasób z okna (jest w aplikacji):

🟩 **Co dopisujesz** w `P03WeatherForecastWPF.Client/MainWindow.xaml`:

```diff
⋯
         mc:Ignorable="d"
         d:DataContext="{d:DesignInstance Type=vm:MainViewModel}"
-        Title="P03 – Pogoda (MVVM)" Height="460" Width="680">
+        Title="P03 – Pogoda (MVVM + DI)" Height="460" Width="680">
 
     <!-- d:DataContext podpowiada IntelliSense nazwy właściwości ViewModelu w XAML (tylko w czasie projektowania) -->
-
-    <Window.Resources>
-        <!-- Wbudowany konwerter bool → Visibility. Własne konwertery dodamy w etapie 6. -->
-        <BooleanToVisibilityConverter x:Key="BoolToVisibility" />
-    </Window.Resources>
 
     <DockPanel Margin="8">
⋯
 
             <!-- Command zamiast Click: przycisk sam się wyłącza, gdy CanExecute zwraca false -->
-            <Button Content="Szukaj" Command="{Binding LoadCitiesCommand}" Margin="0,4" />
+            <Button Content="Szukaj" Command="{Binding LoadCitiesCommand}" />
 
             <ProgressBar Height="6" IsIndeterminate="True"
⋯
 
             <Label Content="Temperatura:" FontWeight="Bold" />
-            <Label Content="{Binding Weather.Temperature, StringFormat={}{0:F1} °C, FallbackValue=—}" FontSize="28" />
+            <Label Content="{Binding Weather.Temperature, Converter={StaticResource TemperatureToDisplay}}" FontSize="28" />
 
             <Label Content="Wiatr:" FontWeight="Bold" />
⋯
             <Label Content="{Binding Weather.Time, StringFormat=g, FallbackValue=—}" />
 
-            <!-- Komunikat o błędzie: pusty tekst, gdy ErrorMessage jest null -->
-            <TextBlock Text="{Binding ErrorMessage}" Foreground="DarkRed" TextWrapping="Wrap" Margin="4" />
+            <!-- Komunikat o błędzie widoczny tylko, gdy ErrorMessage nie jest pusty -->
+            <TextBlock Text="{Binding ErrorMessage}" Foreground="DarkRed" TextWrapping="Wrap" Margin="4"
+                       Visibility="{Binding ErrorMessage, Converter={StaticResource NullToVisibility}}" />
         </StackPanel>
     </DockPanel>
```

```xml
<!-- P03WeatherForecastWPF.Client/MainWindow.xaml -->
<Window x:Class="P03WeatherForecastWPF.Client.MainWindow"
        xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
        xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
        xmlns:d="http://schemas.microsoft.com/expression/blend/2008"
        xmlns:mc="http://schemas.openxmlformats.org/markup-compatibility/2006"
        xmlns:local="clr-namespace:P03WeatherForecastWPF.Client"
        xmlns:vm="clr-namespace:P03WeatherForecastWPF.Client.ViewModels"
        mc:Ignorable="d"
        d:DataContext="{d:DesignInstance Type=vm:MainViewModel}"
        Title="P03 – Pogoda (MVVM + DI)" Height="460" Width="680">

    <!-- d:DataContext podpowiada IntelliSense nazwy właściwości ViewModelu w XAML (tylko w czasie projektowania) -->

    <DockPanel Margin="8">

        <!-- Lewa kolumna: wyszukiwanie miasta -->
        <StackPanel DockPanel.Dock="Left" Width="280" Margin="0,0,8,0">
            <Label Content="Miasto:" />

            <!-- UpdateSourceTrigger=PropertyChanged: ViewModel dostaje tekst po każdej literze,
                 a nie dopiero po opuszczeniu pola (domyślnie LostFocus) -->
            <TextBox Text="{Binding CityName, UpdateSourceTrigger=PropertyChanged}">
                <TextBox.InputBindings>
                    <KeyBinding Key="Enter" Command="{Binding LoadCitiesCommand}" />
                </TextBox.InputBindings>
            </TextBox>

            <!-- Command zamiast Click: przycisk sam się wyłącza, gdy CanExecute zwraca false -->
            <Button Content="Szukaj" Command="{Binding LoadCitiesCommand}" />

            <ProgressBar Height="6" IsIndeterminate="True"
                         Visibility="{Binding IsBusy, Converter={StaticResource BoolToVisibility}}" />

            <ListBox Height="260"
                     ItemsSource="{Binding Cities}"
                     SelectedItem="{Binding SelectedCity}"
                     DisplayMemberPath="FullName" />
        </StackPanel>

        <!-- Prawa część: pogoda dla wybranego miasta -->
        <StackPanel>
            <Label Content="Wybrane miasto:" FontWeight="Bold" />
            <Label Content="{Binding SelectedCity.FullName, FallbackValue=—}" />

            <Label Content="Temperatura:" FontWeight="Bold" />
            <Label Content="{Binding Weather.Temperature, Converter={StaticResource TemperatureToDisplay}}" FontSize="28" />

            <Label Content="Wiatr:" FontWeight="Bold" />
            <Label Content="{Binding Weather.WindSpeed, StringFormat={}{0:F0} km/h, FallbackValue=—}" />

            <Label Content="Opis:" FontWeight="Bold" />
            <Label Content="{Binding Weather.Description, FallbackValue=—}" />

            <Label Content="Czas pomiaru:" FontWeight="Bold" />
            <Label Content="{Binding Weather.Time, StringFormat=g, FallbackValue=—}" />

            <!-- Komunikat o błędzie widoczny tylko, gdy ErrorMessage nie jest pusty -->
            <TextBlock Text="{Binding ErrorMessage}" Foreground="DarkRed" TextWrapping="Wrap" Margin="4"
                       Visibility="{Binding ErrorMessage, Converter={StaticResource NullToVisibility}}" />
        </StackPanel>
    </DockPanel>
</Window>
```

⌨️ **Robię:** F5. 👀 **Pokazuję:** temperatura `12,5 °C` (przecinek z kultury), czerwony komunikat pojawia się tylko dla `xyzxyz` i znika po udanym wyszukaniu (bo `ErrorMessage = null` → `Collapsed`).

🔧 **Ulepszenie:** w edycji 26 konwerter nazywał się `TemperatureToDisplayConveter` (literówka), nie używał `culture`, a `ConvertBack` próbował parsować tekst, choć binding był jednokierunkowy. Zasoby siedziały w `Window.Resources`. Tu: poprawna nazwa, kultura, `NotSupportedException`, zasoby w `App.xaml`, dodatkowy `NullToVisibilityConverter` zamiast zawsze widocznego `TextBlock`.

✅ **Efekt:** `MainWindow.xaml` jest w wersji końcowej; temperatura z jednostką, komunikat tylko gdy jest błąd.

🔗 **Przejście:** „Zostało jedno brzydkie miejsce: konstruktor okna z czterema `new`. Zastąpi go kontener DI”.

---

## 7. P03: Dependency Injection przez Generic Host

**Cel:** `App.xaml.cs` buduje hosta, rejestruje serwisy i tworzy okno; `appsettings.json` wchodzi przez `IOptions<AppSettings>`; podmiana implementacji to jedna linia.

### 7.1 `App.xaml` bez `StartupUri`

🟩 **Co dopisujesz** w `P03WeatherForecastWPF.Client/App.xaml`:

```diff
⋯
              xmlns:local="clr-namespace:P03WeatherForecastWPF.Client"
              xmlns:converters="clr-namespace:P03WeatherForecastWPF.Client.Converters"
-             StartupUri="MainWindow.xaml">
+             Startup="OnStartup"
+             Exit="OnExit">
+    <!-- Brak StartupUri: okno tworzy kontener DI w App.xaml.cs (OnStartup) -->
     <Application.Resources>
 
```

```xml
<!-- P03WeatherForecastWPF.Client/App.xaml -->
<Application x:Class="P03WeatherForecastWPF.Client.App"
             xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
             xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
             xmlns:local="clr-namespace:P03WeatherForecastWPF.Client"
             xmlns:converters="clr-namespace:P03WeatherForecastWPF.Client.Converters"
             Startup="OnStartup"
             Exit="OnExit">
    <!-- Brak StartupUri: okno tworzy kontener DI w App.xaml.cs (OnStartup) -->
    <Application.Resources>

        <!-- Konwertery jako zasoby aplikacji: jedna instancja, dostępna w każdym oknie -->
        <converters:TemperatureToDisplayConverter x:Key="TemperatureToDisplay" />
        <converters:NullToVisibilityConverter x:Key="NullToVisibility" />
        <BooleanToVisibilityConverter x:Key="BoolToVisibility" />

        <Style TargetType="Button">
            <Setter Property="Padding" Value="8,4" />
            <Setter Property="Margin" Value="0,4" />
        </Style>

        <Style TargetType="Label">
            <Setter Property="Padding" Value="4,2" />
        </Style>

    </Application.Resources>
</Application>
```

🗣️ **Mówię:** „`StartupUri` tworzyło okno konstruktorem bezparametrowym: `new MainWindow()`. Nasze okno będzie wymagało `MainViewModel` w konstruktorze, więc tworzyć je musi ktoś, kto umie dostarczyć ViewModel: kontener. Zamiast `StartupUri` podpinamy zdarzenia `Startup` i `Exit`”.

### 7.2 `App.xaml.cs` z hostem

```csharp
// P03WeatherForecastWPF.Client/App.xaml.cs
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
```

💡 **Wyjaśniam** linia po linii (to najważniejszy plik dnia):

| Linia | Co robi |
|---|---|
| `Host.CreateApplicationBuilder()` | tworzy budowniczego z domyślną konfiguracją: czyta `appsettings.json` z folderu aplikacji (dlatego `PreserveNewest` w `.csproj`), `appsettings.{Environment}.json`, zmienne środowiskowe, User Secrets (gdy `UserSecretsId` jest w projekcie), włącza logowanie do konsoli/debuggera |
| `builder.Services.Configure<AppSettings>(builder.Configuration.GetSection("AppSettings"))` | binduje sekcję JSON do klasy i rejestruje `IOptions<AppSettings>` |
| `AddHttpClient<IMeteoService, OpenMeteoService>(client => …)` | **typed client**: rejestruje `IMeteoService` → `OpenMeteoService` i dla każdego egzemplarza podaje `HttpClient` z `IHttpClientFactory`. Fabryka współdzieli połączenia (handler) między klientami i odświeża je co 2 minuty, więc nie ma ani wyczerpania gniazd, ani przeterminowanego DNS. Lambda konfiguruje klienta (timeout, jutro `BaseAddress`) |
| `AddSingleton<MainViewModel>()` | jeden ViewModel na aplikację; rejestracja po typie konkretnym, bo nikt nie potrzebuje interfejsu do niego |
| `AddTransient<MainWindow>()` | okno tworzone na żądanie; kontener widzi konstruktor `MainWindow(MainViewModel)` i sam poda ViewModel |
| `_host = builder.Build()` | buduje kontener; od teraz rejestracje są zamrożone |
| `OnStartup` → `_host.Start()` | uruchamia hosta (usługi w tle, logowanie); `GetRequiredService<MainWindow>()` tworzy okno **i cały łańcuch zależności**: `MainWindow` → `MainViewModel` → `IMeteoService` → `HttpClient` + `IOptions<AppSettings>` |
| `OnExit` | zatrzymuje hosta i zwalnia zasoby (`Dispose` wszystkich singletonów implementujących `IDisposable`) |

🗣️ **Mówię:** „`GetRequiredService` rzuca wyjątek, gdy typ nie jest zarejestrowany. `GetService` zwróciłby `null` i błąd wyszedłby później, w mniej oczywistym miejscu. W kodzie aplikacji zawsze `GetRequiredService`. Uwaga: ta jedna linia w `OnStartup` to jedyne miejsce, gdzie **my** pytamy kontener. Reszta obiektów dostaje zależności konstruktorem. Wstrzykiwanie `IServiceProvider` do ViewModelu i wołanie `GetService` w środku to antywzorzec (service locator): ukrywa zależności”.

### 7.3 Okno z konstruktorem

🟩 **Co dopisujesz** w `P03WeatherForecastWPF.Client/MainWindow.xaml.cs`:

```diff
⋯
-using System.Net.Http;
 using System.Windows;
-using Microsoft.Extensions.Options;
-using P03WeatherForecastWPF.Client.Configuration;
-using P03WeatherForecastWPF.Client.Services;
 using P03WeatherForecastWPF.Client.ViewModels;
 
⋯
 
 /// <summary>
-/// Wersja tymczasowa (etap 5): ViewModel z CommunityToolkit.Mvvm, nadal tworzony ręcznie.
-/// W etapie 7 zrobi to za nas kontener Dependency Injection.
+/// Code-behind okna w MVVM jest prawie pusty: jedyne zadanie to
+/// podpięcie ViewModelu jako DataContext. Cała logika jest w MainViewModel.
+/// ViewModel wchodzi konstruktorem – dostarcza go kontener DI (App.xaml.cs).
 /// </summary>
 public partial class MainWindow : Window
 {
-    public MainWindow()
+    public MainWindow(MainViewModel viewModel)
     {
         InitializeComponent();
-
-        IOptions<AppSettings> settings = Options.Create(new AppSettings { DefaultLanguage = "pl" });
-
-        DataContext = new MainViewModel(new OpenMeteoService(new HttpClient(), settings));
-
-        // Wersja bez internetu – ViewModel nie widzi różnicy:
-        // DataContext = new MainViewModel(new FakeMeteoService());
+        DataContext = viewModel;
     }
 }
```

```csharp
// P03WeatherForecastWPF.Client/MainWindow.xaml.cs
using System.Windows;
using P03WeatherForecastWPF.Client.ViewModels;

namespace P03WeatherForecastWPF.Client;

/// <summary>
/// Code-behind okna w MVVM jest prawie pusty: jedyne zadanie to
/// podpięcie ViewModelu jako DataContext. Cała logika jest w MainViewModel.
/// ViewModel wchodzi konstruktorem – dostarcza go kontener DI (App.xaml.cs).
/// </summary>
public partial class MainWindow : Window
{
    public MainWindow(MainViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }
}
```

⌨️ **Robię:** F5. 👀 **Pokazuję:** działa tak samo jak w bloku 6, ale nigdzie w kodzie nie ma `new OpenMeteoService`, `new HttpClient` ani `new MainViewModel`.

⌨️ **Robię** trzy pokazy (po 2 min każdy):

1. **Podmiana implementacji.** W `ConfigureServices` komentuję `AddHttpClient<…>` i odkomentowuję `services.AddSingleton<IMeteoService, FakeMeteoService>();`. F5: dane testowe. „Jedna linia. ViewModel, okno i XAML nietknięte. Tak samo w dniu 13 podmienimy Open‑Meteo na nasze API, a w testach na atrapę”. Przywracam.
2. **Konfiguracja bez kompilacji.** W `bin\Debug\net10.0-windows\appsettings.json` zmieniam `"pl"` na `"en"`, uruchamiam `.exe` z Eksploratora: `Warsaw, Masovian Voivodeship (Poland)`. Przywracam `pl`.
3. **Brak rejestracji.** Komentuję `services.AddSingleton<MainViewModel>();`. F5: `InvalidOperationException: Unable to resolve service for type 'MainViewModel' while attempting to activate 'MainWindow'`. „Komunikat mówi dokładnie, czego brakuje i kto tego chciał. To najczęstszy błąd DI, zobaczycie go jutro w API”. Przywracam.

💡 **Wyjaśniam** (alternatywy): „W edycjach 26/31 robiliśmy `new ServiceCollection()` + `BuildServiceProvider()`. To ten sam kontener, ale bez konfiguracji i logowania; `appsettings.json` trzeba było czytać osobno. Generic Host daje wszystko w jednym i jest identyczny z `WebApplication.CreateBuilder` z ASP.NET Core. Inne kontenery (Autofac, DryIoc) mają więcej możliwości, ale `Microsoft.Extensions.DependencyInjection` wystarcza w 95% projektów i jest standardem w .NET”.

🔧 **Ulepszenie:** edycje 26/31: `ServiceCollection` bez hosta, `OpenMeteoService` zarejestrowany jako singleton tworzący `HttpClient` w `using` przy każdym zapytaniu, konfiguracja czytana w konstruktorze serwisu, pusty interfejs `IMainViewModel`, drugie okno otwierane przez `IServiceProvider` wstrzyknięty do ViewModelu. Edycja 32: Generic Host, `AddHttpClient` typed client, `IOptions<AppSettings>`, brak pustych interfejsów, brak service locatora, `OnExit` zwalnia hosta.

✅ **Efekt:** P03 w wersji końcowej. `dotnet build` bez ostrzeżeń. Trzy testy: `Warszawa` (2 wyniki, pogoda po kliknięciu), `xyzxyz` (komunikat), puste pole (przycisk wyłączony).

🔗 **Przejście:** „Teraz wy. Ten sam schemat: model z JSON, serwis za interfejsem, ViewModel z toolkitu, okno z bindingiem, host w `App.xaml.cs`. Dane: kursy walut NBP”.

---

## 8. Zadanie: Z01AplikacjaNBP.Client

**Cel:** uczestnicy samodzielnie budują aplikację MVVM + DI według schematu z P03, na nowym API i z nową kontrolką (`DataGrid`).

### 8.1 Treść zadania (45 min pracy własnej)

🗣️ **Mówię** i wyświetlam na ekranie:

> **Z01AplikacjaNBP.Client** – aplikacja WPF, która:
> 1. Po kliknięciu **Pobierz kursy** pobiera tabelę A kursów średnich z API NBP: `https://api.nbp.pl/api/exchangerates/tables/A?format=json` i pokazuje ją w `DataGrid` z kolumnami **Kod**, **Waluta**, **Kurs średni (PLN)** (4 miejsca po przecinku), posortowaną po kodzie.
> 2. Nad tabelą pokazuje numer tabeli, datę i liczbę walut, np. `Tabela 196/A/NBP/2026 z dnia 09.10.2026 (32 walut)`.
> 3. Pod tabelą ma przelicznik: pole **Kwota w PLN** (domyślnie 100) i wynik `430,00 PLN = 98,23 EUR` dla waluty zaznaczonej w tabeli. Wynik przelicza się po każdej zmianie kwoty lub zaznaczenia.
> 4. Podczas pobierania pokazuje pasek postępu, a przycisk jest wyłączony. Błąd sieci to komunikat, nie wyjątek.
> 5. Architektura jak w P03: `Models/`, `Services/IExchangeRateService` + `NbpExchangeRateService`, `ViewModels/MainViewModel` (CommunityToolkit.Mvvm), `App.xaml.cs` z Generic Host i `AddHttpClient`, `MainWindow(MainViewModel)`.

**Wskazówki** (podaję od razu, bez nich grupa traci czas na drobiazgi):

- Otwórz adres w przeglądarce. Odpowiedź to **tablica** tabel (`[ { "table": "A", "no": "...", "effectiveDate": "...", "rates": [ { "currency": "...", "code": "...", "mid": 4.3775 } ] } ]`), więc deserializuj do `ExchangeRateTable[]` i weź `FirstOrDefault()`.
- NBP używa camelCase; mapuj `[JsonPropertyName("effectiveDate")]`. Datę trzymaj w `DateOnly`, kurs w `decimal` (pieniądze nigdy w `double`).
- W `AddHttpClient` ustaw `client.BaseAddress = new Uri("https://api.nbp.pl/api/")`, a w serwisie użyj adresu względnego `exchangerates/tables/A?format=json`.
- `DataGrid`: `AutoGenerateColumns="False"`, `IsReadOnly="True"`, `CanUserAddRows="False"`, kolumny `DataGridTextColumn` z `Binding` i `StringFormat=N4`.
- Właściwość wyliczana (`ConvertedAmount`) nie ma settera, więc nie powiadomi sama. Użyj `[NotifyPropertyChangedFor(nameof(ConvertedAmount))]` na `AmountPln` i `SelectedRate`.
- `TextBox` zbindowany do `decimal`: gdy wpiszesz literę, WPF sam pokaże czerwoną ramkę i nie zapisze wartości. To wbudowana walidacja konwersji.
- Pakiety: te same trzy co w P03. Projekt: `dotnet new wpf -n Z01AplikacjaNBP.Client -o Z01AplikacjaNBP.Client -f net10.0` + `dotnet sln add`.

⌨️ **Robię** w czasie pracy własnej: chodzę po sali. Najczęstsze blokady: brak `Startup="OnStartup"` w `App.xaml` (okno się nie pokazuje), `StartupUri` zostawione razem z hostem (dwa okna albo wyjątek o konstruktorze), `mid` jako `double` z `StringFormat` niedziałającym na `Label` bez `{}`, zapomniane `[NotifyPropertyChangedFor]` (wynik nie odświeża się).

### 8.2 Rozwiązanie (omówienie, 30 min)

🗣️ **Mówię:** „Pokazuję moje rozwiązanie plik po pliku. Porównajcie ze swoim: nie chodzi o identyczny kod, tylko o ten sam podział odpowiedzialności”.

```xml
<!-- Z01AplikacjaNBP.Client/Z01AplikacjaNBP.Client.csproj -->
<Project Sdk="Microsoft.NET.Sdk">

  <PropertyGroup>
    <OutputType>WinExe</OutputType>
    <TargetFramework>net10.0-windows</TargetFramework>
    <Nullable>enable</Nullable>
    <ImplicitUsings>enable</ImplicitUsings>
    <UseWPF>true</UseWPF>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include="CommunityToolkit.Mvvm" Version="8.4.2" />
    <PackageReference Include="Microsoft.Extensions.Hosting" Version="10.0.12" />
    <PackageReference Include="Microsoft.Extensions.Http" Version="10.0.12" />
  </ItemGroup>

</Project>
```

```csharp
// Z01AplikacjaNBP.Client/Models/ExchangeRateTable.cs
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
```

💡 **Wyjaśniam:** „`decimal` dla kursu: `0.1 + 0.2` w `double` to `0.30000000000000004`; przy pieniądzach to błąd księgowy. `DateOnly` (od .NET 6) zamiast `DateTime` z godziną 00:00; `System.Text.Json` deserializuje `"2026-10-09"` do `DateOnly` bez dodatkowego kodu. `List<Rate> = []` zamiast `null`, żeby pętla nigdy nie dostała `null`”.

```csharp
// Z01AplikacjaNBP.Client/Services/IExchangeRateService.cs
using Z01AplikacjaNBP.Client.Models;

namespace Z01AplikacjaNBP.Client.Services;

public interface IExchangeRateService
{
    /// <summary>Pobiera aktualną tabelę A (kursy średnie) z NBP.</summary>
    Task<ExchangeRateTable?> GetTableAAsync();
}
```

```csharp
// Z01AplikacjaNBP.Client/Services/NbpExchangeRateService.cs
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
```

💡 **Wyjaśniam:** „Serwis nie zna pełnego adresu: `BaseAddress` jest w rejestracji `AddHttpClient`. Gdy NBP zmieni domenę albo jutro postawimy własne API, zmieniamy jedną linię w `App.xaml.cs` (a w dniu 13 weźmiemy ją z `appsettings.json`). `FirstOrDefault()` zwraca `null` dla pustej tablicy; ViewModel to obsłuży”.

```csharp
// Z01AplikacjaNBP.Client/ViewModels/MainViewModel.cs
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
```

💡 **Wyjaśniam:**

- „`ConvertedAmount` to właściwość **wyliczana**: nie ma pola ani settera, więc nikt nie wywoła dla niej `OnPropertyChanged`. `[NotifyPropertyChangedFor(nameof(ConvertedAmount))]` na `AmountPln` i `SelectedRate` mówi generatorowi: po zmianie tej właściwości powiadom też o tamtej. W edycji 26 robiliśmy to ręcznie: `OnPropertyChanged(nameof(CurrentTemperature))` w setterze”.
- „`AmountPln / SelectedRate.Mid`: kurs to cena jednej jednostki waluty w PLN, więc dzielimy. `430 / 4,3775 = 98,23`”.
- „`OrderBy(r => r.Code)`: sortowanie należy do ViewModelu (logika prezentacji), nie do serwisu (dostęp do danych) ani do `DataGrid` (użytkownik może sortować po kliknięciu w nagłówek, ale stan początkowy ustalamy my)”.
- „`return` w `try` z `finally`: `IsBusy = false` wykona się mimo wczesnego wyjścia”.

```xml
<!-- Z01AplikacjaNBP.Client/App.xaml -->
<Application x:Class="Z01AplikacjaNBP.Client.App"
             xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
             xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
             Startup="OnStartup"
             Exit="OnExit">
    <Application.Resources>
        <BooleanToVisibilityConverter x:Key="BoolToVisibility" />
    </Application.Resources>
</Application>
```

```csharp
// Z01AplikacjaNBP.Client/App.xaml.cs
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
```

```xml
<!-- Z01AplikacjaNBP.Client/MainWindow.xaml -->
<Window x:Class="Z01AplikacjaNBP.Client.MainWindow"
        xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
        xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
        xmlns:d="http://schemas.microsoft.com/expression/blend/2008"
        xmlns:mc="http://schemas.openxmlformats.org/markup-compatibility/2006"
        xmlns:vm="clr-namespace:Z01AplikacjaNBP.Client.ViewModels"
        mc:Ignorable="d"
        d:DataContext="{d:DesignInstance Type=vm:MainViewModel}"
        Title="Z01 – Kursy walut NBP" Height="520" Width="760">

    <Grid Margin="8">
        <Grid.RowDefinitions>
            <RowDefinition Height="Auto" />
            <RowDefinition Height="Auto" />
            <RowDefinition Height="*" />
            <RowDefinition Height="Auto" />
        </Grid.RowDefinitions>

        <StackPanel Grid.Row="0" Orientation="Horizontal">
            <Button Content="Pobierz kursy" Command="{Binding LoadRatesCommand}" Padding="12,4" />
            <TextBlock Text="{Binding TableInfo}" VerticalAlignment="Center" Margin="12,0" />
        </StackPanel>

        <ProgressBar Grid.Row="1" Height="6" IsIndeterminate="True" Margin="0,6"
                     Visibility="{Binding IsBusy, Converter={StaticResource BoolToVisibility}}" />

        <!-- DataGrid: kolumny definiujemy ręcznie, żeby mieć polskie nagłówki i formatowanie -->
        <DataGrid Grid.Row="2"
                  ItemsSource="{Binding Rates}"
                  SelectedItem="{Binding SelectedRate}"
                  AutoGenerateColumns="False" IsReadOnly="True"
                  CanUserAddRows="False">
            <DataGrid.Columns>
                <DataGridTextColumn Header="Kod" Binding="{Binding Code}" Width="80" />
                <DataGridTextColumn Header="Waluta" Binding="{Binding Currency}" Width="*" />
                <DataGridTextColumn Header="Kurs średni (PLN)" Binding="{Binding Mid, StringFormat=N4}" Width="160" />
            </DataGrid.Columns>
        </DataGrid>

        <StackPanel Grid.Row="3" Orientation="Horizontal" Margin="0,8,0,0">
            <Label Content="Kwota w PLN:" />
            <TextBox Width="120" Text="{Binding AmountPln, UpdateSourceTrigger=PropertyChanged}" />
            <Label Content="{Binding ConvertedAmount}" FontWeight="Bold" />
            <TextBlock Text="{Binding ErrorMessage}" Foreground="DarkRed" VerticalAlignment="Center" Margin="12,0" />
        </StackPanel>
    </Grid>
</Window>
```

💡 **Wyjaśniam** `DataGrid`: „`AutoGenerateColumns="False"`, bo automatyczne kolumny miałyby nagłówki `Currency`, `Code`, `Mid` i domyślny format. `IsReadOnly` i `CanUserAddRows="False"` usuwają pusty wiersz do edycji na końcu. `SelectedItem="{Binding SelectedRate}"` działa tak samo jak w `ListBox`. `StringFormat=N4` w kolumnie nie potrzebuje `{}`, bo nie zaczyna się od nawiasu”.

```csharp
// Z01AplikacjaNBP.Client/MainWindow.xaml.cs
using System.Windows;
using Z01AplikacjaNBP.Client.ViewModels;

namespace Z01AplikacjaNBP.Client;

public partial class MainWindow : Window
{
    public MainWindow(MainViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }
}
```

⌨️ **Robię:** `dotnet sln 32Dzien12.slnx add Z01AplikacjaNBP.Client/Z01AplikacjaNBP.Client.csproj`, ustawiam Z01 jako startowy, F5. **Pobierz kursy** → tabela, zaznaczam `EUR`, wpisuję `430`. 👀 **Pokazuję:** `430,00 PLN = 98,23 EUR` (u Ciebie aktualny kurs). Wpisuję `abc`: czerwona ramka, wynik się nie zmienia. Wyłączam Wi‑Fi, klikam **Pobierz kursy**: „Błąd połączenia: …”.

🔧 **Ulepszenie:** w edycji 31 (`P01AplikacjaNBP_WPF.Client`) serwis tworzył `HttpClient` w `using` przy każdym zapytaniu i używał Newtonsoft.Json, `Mid` był `double`, okno robiło `DataContext = new MainViewModel(new NbpWebApiService())`, nie było `IsBusy`, obsługi błędów ani przelicznika. Tu: `AddHttpClient` z `BaseAddress`, `System.Text.Json` z `decimal` i `DateOnly`, Generic Host, przelicznik z właściwością wyliczaną, komunikat o błędzie.

✅ **Efekt:** tabela z 30+ walutami posortowana po kodzie, informacja o tabeli nad nią, przelicznik reaguje na zmianę kwoty i zaznaczenia.

---

## 9. Podsumowanie dnia, pytania i commit

🗣️ **Mówię** (podsumowanie, 5 min):
„Dziś: (1) delegat to metoda w zmiennej, zdarzenie to delegat z ograniczeniem dostępu; (2) MVVM: ViewModel trzyma stan i komendy, widok tylko binduje, code‑behind ma jedną linię; (3) `INotifyPropertyChanged` i `ICommand` napisane ręcznie, a potem wygenerowane przez CommunityToolkit.Mvvm; (4) binding: `UpdateSourceTrigger`, `ItemsSource`/`SelectedItem`, `StringFormat`, `FallbackValue`, konwertery; (5) Dependency Injection: interfejs + konstruktor + kontener, cykle życia, Generic Host z konfiguracją `IOptions`. Jutro postawimy własne Web API w tym samym Generic Hoście: `builder.Services`, `AddScoped`, `appsettings.json` będą wyglądały znajomo. `MainViewModel` i `IMeteoService` zostają; dojdzie `IProductService` z `P06Shop.Shared`”.

**Pytania kontrolne** (zadaj grupie):

1. Czym różni się `event` od zwykłego pola typu delegata? Co może zrobić kod spoza klasy z jednym, a czego nie może z drugim?
2. Co się stanie, gdy setter właściwości w ViewModelu nie wywoła `OnPropertyChanged`? Czy aplikacja się wywróci?
3. Po co `UpdateSourceTrigger=PropertyChanged` na `TextBox` z nazwą miasta? Co by się działo bez niego z przyciskiem **Szukaj**?
4. Dlaczego `MainViewModel` przyjmuje `IMeteoService`, a nie `OpenMeteoService`? Podaj dwie korzyści.
5. Czym różni się `AddSingleton` od `AddTransient`? Co jest singletonem w P03, a co transientem i dlaczego?
6. Co generuje `[ObservableProperty]` na właściwości `partial`? Dlaczego klasa musi być `partial`?
7. Dlaczego `StringFormat={}{0:F1} °C` ma `{}` na początku, a `StringFormat=N4` nie?
8. Gdzie w kodzie dnia 12 jest jedyne wywołanie `GetRequiredService` i dlaczego nie wołamy go w ViewModelu?

⌨️ **Robię:** solucja końcowa (Visual Studio dodało `README.md` i instrukcje do folderu **Solution Items**):

```xml
<!-- 32Dzien12.slnx -->
<Solution>
  <Folder Name="/Solution Items/">
    <File Path="README.md" />
    <File Path="INSTRUKCJA_PROWADZACY.md" />
    <File Path="INSTRUKCJA_STUDENT.md" />
  </Folder>
  <Project Path="P03WeatherForecastWPF.Client/P03WeatherForecastWPF.Client.csproj" />
  <Project Path="P04Delegaty/P04Delegaty.csproj" />
  <Project Path="Z01AplikacjaNBP.Client/Z01AplikacjaNBP.Client.csproj" />
</Solution>
```

Commit i push:

```powershell
git init
git add .
git commit -m "Dzien 12: delegaty, MVVM (klasyczny + CommunityToolkit), binding, DI przez Generic Host, zadanie NBP"
git branch -M master
git remote add origin https://github.com/tomasz-trainer/32Dzien12.git
git push -u origin master
```

**Zadanie domowe** (opcjonalne): w P03 dodaj komendę `ClearCommand` (czyści listę, zaznaczenie i pogodę; `CanExecute` tylko, gdy lista nie jest pusta) i przycisk **Wyczyść** z `KeyBinding` na `Escape`. Druga opcja: w Z01 dodaj `ComboBox` z wyborem tabeli `A`/`B` (`exchangerates/tables/B`) zbindowany do właściwości `TableName`.

---

## Dodatek A: ulepszenia względem edycji 26 i 31

| Gdzie | Edycje 26/31 | Edycja 32 | Dlaczego |
|---|---|---|---|
| Platforma, pakiety | 26: .NET 8, toolkit 8.2.2, `Microsoft.Extensions.DependencyInjection` 8.0; 31: .NET 10, toolkit 8.4.2, DI 10.0.12, Newtonsoft 13 | .NET 10, `CommunityToolkit.Mvvm 8.4.2`, `Microsoft.Extensions.Hosting 10.0.12`, `Microsoft.Extensions.Http 10.0.12`; usunięty `Configuration.Json` (zawarty w Hosting), bez Newtonsoft | mniej pakietów, jeden host |
| P04Delegaty | 31: cztery przypisania `Action`/`Func`/`Predicate` w `Main` | własny typ delegata, metoda wyższego rzędu, lambdy, LINQ, multicast, `event` z `EventArgs` i `-=`, mapa „gdzie to w MVVM” | uczestnik rozumie `RelayCommand` i `PropertyChanged` |
| ViewModel klasyczny | `BaseViewModel` + pusty `IMainViewModel`; `CityName` bez `OnPropertyChanged`; `async void loadCities()` bez obsługi błędów | `ViewModels/Classic/MainViewModelClassic.cs` w całości w jednym pliku (widać każdy element), wszystkie właściwości powiadamiają, `async Task` + discard, `IsBusy`, `ErrorMessage`, `try/catch/finally`, `CanExecute` | kompletny wzorzec; pusty interfejs nic nie dawał |
| `RelayCommand` | `internal`, parametry bez `?` (ostrzeżenia nullable) | `public`, `Action<object?>`, `Predicate<object?>?`, komentarz o `RequerySuggested` | czysty build z `Nullable=enable` |
| Toolkit | `[ObservableProperty] private Weather weather;` (pole z małej litery), `[RelayCommand] public async void LoadCities()`, `Cities` raz podmieniana w całości, raz `Clear/Add`; V2 i V3 jako kolejne pliki | `[ObservableProperty] public partial …` (C# 13 / toolkit 8.4), `[NotifyCanExecuteChangedFor]`, `[RelayCommand(CanExecute=…)] private async Task`, `partial void OnSelectedCityChanged`, `ObservableCollection { get; }` | nowa składnia bez pola‑cienia; `async void` wywracał aplikację |
| Binding | `TextBox` bez `UpdateSourceTrigger`, brak `FallbackValue`, `d:DataContext`, `KeyBinding` | `UpdateSourceTrigger=PropertyChanged`, `FallbackValue=—`, `KeyBinding Enter`, `d:DataContext` z `DesignInstance`, `ProgressBar` z `IsBusy`, komunikat z `NullToVisibility` | przycisk reaguje na każdą literę; IntelliSense w XAML |
| Konwertery | 26: `TemperatureToDisplayConveter` (literówka), bez `culture`, `ConvertBack` parsuje; zasoby w `Window.Resources` | poprawna nazwa, `culture`, `NotSupportedException`, `NullToVisibilityConverter`, zasoby i style w `App.xaml` | zasoby wspólne dla kolejnych okien |
| Serwis | 31: `OpenMeteoService` singleton tworzy `HttpClient` w `using` przy każdym zapytaniu i sam czyta konfigurację; 26: AccuWeather z kluczem API | `OpenMeteoService(HttpClient, IOptions<AppSettings>)` przez `AddHttpClient<IMeteoService, OpenMeteoService>` | `IHttpClientFactory`, brak wyczerpania gniazd, bez klucza |
| `FakeMeteoService` | stałe dane, bez opóźnienia | filtrowanie po nazwie, `Task.Delay(500)`, losowa pogoda | widać pasek postępu i brak wyników |
| DI | `new ServiceCollection()` + `BuildServiceProvider()`, `GetService` (może zwrócić `null`), brak zwalniania | `Host.CreateApplicationBuilder`, `Configure<AppSettings>`, `GetRequiredService`, `OnExit` zatrzymuje hosta | ten sam model co ASP.NET Core (dzień 13); konfiguracja i logowanie za darmo |
| Drugie okno | `SecondWindow` otwierane przez `IServiceProvider` wstrzyknięty do ViewModelu | usunięte | service locator w ViewModelu to antywzorzec; nawigacja i dialogi wracają w dniu 14 przez `IMessageDialogService` |
| Konfiguracja | `UserSecretsId` i `AddUserSecrets` bez żadnego sekretu, `Copy always` | `IOptions<AppSettings>` z `Configure<T>`, `PreserveNewest` | User Secrets dopiero przy sekretach Azure (dzień 15) |
| Zadanie NBP | 31: `P01AplikacjaNBP_WPF.Client`: Newtonsoft, `HttpClient` w `using`, `Mid` jako `double`, `DataContext = new MainViewModel(new NbpWebApiService())`, bez błędów i `IsBusy`; brak numeracji zgodnej z resztą | `Z01AplikacjaNBP.Client`: `System.Text.Json`, `decimal`, `DateOnly`, `AddHttpClient` z `BaseAddress`, Generic Host, przelicznik z `[NotifyPropertyChangedFor]`, `IsBusy`, `ErrorMessage`, sortowanie | ten sam schemat co P03; nazwa `Z01` = zadanie |
| Instrukcja | `Podsumowanie.txt` z 7 punktami | dwie instrukcje z etapami pośrednimi, które się kompilują (`FakeMeteoService`, `Options.Create`) | uczestnik nigdy nie ma czerwonego projektu dłużej niż jeden blok |

## Dodatek B: jak zweryfikowano scenariusz

- Cały kod końcowy (`P03WeatherForecastWPF.Client`, `P04Delegaty`, `Z01AplikacjaNBP.Client`) zbudowano `dotnet build 32Dzien12.slnx` z .NET SDK 10.0.401 bez błędów i ostrzeżeń. Dodatkowo zbudowano każdy etap pośredni z tej instrukcji (stan po blokach 3, 4, 5, 6, 7) z plikami „wersja tymczasowa” dokładnie w tej treści, jaka jest w blokach.
- `P04Delegaty` uruchomiono z konsoli; wyjście w bloku 1.3 jest skopiowane z tego uruchomienia.
- Logikę `MainViewModel` (P03) i `MainViewModel` (Z01) przetestowano w programie konsolowym z tym samym kontenerem DI (`Host.CreateApplicationBuilder`, te same rejestracje co w `App.xaml.cs`): `LoadCitiesCommand` dla `Warszawa` zwraca 2 miasta, ustawienie `SelectedCity` pobiera pogodę (`Weather` z temperaturą i opisem), `xyzxyz` daje `ErrorMessage = "Nie znaleziono miasta o takiej nazwie."`, pusta nazwa wyłącza komendę (`CanExecute` zwraca `false`). NBP: tabela A z 32 walutami, `AmountPln = 430` i `SelectedRate = EUR` dają `430,00 PLN = 98,23 EUR` (kurs 4,3775 z tabeli z 2026‑10‑09).
- Okien WPF nie uruchamiano (środowisko bez Windows). Przejdź bloki 3–8 przed zajęciami: w szczególności sprawdź `StringFormat` na `Label.Content`, widoczność `ProgressBar` przy `FakeMeteoService` i komunikat `Unable to resolve service` z pokazu 7.3.
