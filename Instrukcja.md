# Dzień 12: MVVM, Dependency Injection, delegaty

> **Technologia:** .NET 10 (LTS), C# 14, Visual Studio 2026, solucja `32Dzien12.slnx`. Pakiety: `CommunityToolkit.Mvvm 8.4.2`, `Microsoft.Extensions.Hosting 10.0.12`, `Microsoft.Extensions.Http 10.0.12`.
>
> **Punkt startowy:** projekt `P03WeatherForecastWPF.Client` z dnia 11 (klient Open‑Meteo z logiką w code‑behind). Pozostałe projekty z dnia 11 (P01, P02) zostają w repozytorium `32Dzien11`; dziś ich nie ruszamy.
>
> **Cel dnia:** ta sama aplikacja pogodowa, ale zbudowana „jak w firmie”: logika w **ViewModelu** (wzorzec MVVM), okno podpięte **bindingiem** (wiązaniem danych), obiekty tworzone przez **kontener Dependency Injection** (Generic Host). Po drodze powstaje projekt konsolowy **P04Delegaty**, bo bez delegatów i zdarzeń nie da się zrozumieć `ICommand` ani `INotifyPropertyChanged`. Na koniec zadanie **Z01AplikacjaNBP.Client** (kursy walut NBP w `DataGrid`) do samodzielnego wykonania tym samym schematem. Od dnia 13 do P03 dojdzie własne Web API; ViewModel i serwisy przez interfejs zostają bez zmian.

**Projekty dnia:**

| Projekt | Typ | Co powstaje |
|---|---|---|
| `P04Delegaty` | konsola | własne delegaty, `Action`/`Func`/`Predicate`, lambdy, multicast, zdarzenia (`event`) – fundament pod `ICommand` i `INotifyPropertyChanged` |
| `P03WeatherForecastWPF.Client` | WPF | aplikacja pogodowa z dnia 11 przebudowana na MVVM: najpierw ręcznie (`ViewModels/Classic`, własny `RelayCommand`), potem przez CommunityToolkit.Mvvm; binding, konwertery, Dependency Injection przez Generic Host, `IMeteoService` z dwiema implementacjami (Open‑Meteo i Fake), `IOptions<AppSettings>` |
| `Z01AplikacjaNBP.Client` | WPF | zadanie: kursy walut NBP w `DataGrid` + przelicznik, ten sam schemat MVVM + DI |

**Stan P03 na starcie i co się z nim dziś dzieje:**

| Plik | Co zawiera | Co się z nim dziś dzieje |
|---|---|---|
| `Models/City.cs`, `GeocodingResponse.cs`, `ForecastResponse.cs`, `Weather.cs` | DTO Open‑Meteo i model domenowy | **bez zmian** |
| `Services/OpenMeteoService.cs` | geocoding + pogoda, `HttpClient` i język przez konstruktor | implementuje `IMeteoService`, język z `IOptions<AppSettings>` (sekcja 4) |
| `MainWindow.xaml` | `x:Name`, `Click`, `SelectionChanged` | binding i `Command` zamiast `x:Name` i `Click` (sekcje 3, 5, 6) |
| `MainWindow.xaml.cs` | 91 linii logiki w code‑behind | 18 linii: `DataContext = viewModel` (sekcje 2, 7) |
| `App.xaml`, `App.xaml.cs` | `StartupUri`, pusta klasa | konwertery, style, Generic Host (sekcje 6, 7) |
| `appsettings.json` | `AppSettings:DefaultLanguage = pl` | **bez zmian**, czyta go Generic Host |
| `P03WeatherForecastWPF.Client.csproj` | `Microsoft.Extensions.Configuration.Json` | trzy nowe pakiety (sekcja 2) |

## Jak czytać tę instrukcję

| Znacznik | Znaczenie |
|---|---|
| 💡 **Wyjaśnienie** | omówienie: co się dzieje i dlaczego |
| ⚠️ **Uwaga** | pułapka albo stan przejściowy, o którym trzeba wiedzieć |
| ✅ **Punkt kontrolny** | co ma działać, zanim przejdziemy dalej |
| 🔧 **Zmiana względem poprzednich edycji** | tu robimy **inaczej niż w edycjach 26/31**; tamto działało, to jest poprawka lub nowsza praktyka |
| 🟩 **Co dopisujemy** | podgląd zmian w istniejącym pliku: linie `+` dopisujemy, `-` usuwamy, `⋯` to pominięty kod; pod spodem zawsze jest pełny plik |
| 🧪 **Dane testowe / Eksperyment** | wartości do sprawdzenia programu; eksperyment to krok opcjonalny, który celowo coś psuje, żeby zobaczyć skutek |

Zasady dotyczące kodu:

- Każdy blok kodu jest **gotowy do skopiowania w całości**. Nad blokiem jest ścieżka pliku (także w pierwszej linii komentarza w bloku). Blok zastępuje całą zawartość tego pliku.
- Znaków `+`/`-` z bloków 🟩 nie kopiujemy; pełna wersja pliku jest zawsze pod blokiem 🟩.
- Pliki oznaczone **„wersja tymczasowa”** są etapem pośrednim. Nadpisujemy je w późniejszej sekcji, ale dzięki nim projekt kompiluje się po każdej sekcji.
- Numeracja sekcji (1–8) odpowiada „etapom” w komentarzach kodu: „etap 7” to sekcja 7.
- Fragmenty **Dlaczego tak** wyjaśniają decyzje projektowe; opierają się na nich dni 13–18.
- Gotowy kod jest w repozytorium GitHub `tomasz-trainer/32Dzien12`, ale najwięcej daje napisanie go samodzielnie, krok po kroku.

## Plan dnia (orientacyjnie)

| Sekcja | Temat | Czas |
|---|---|---|
| 0 | Wstęp: po co MVVM, po co Dependency Injection, Generic Host | 25 min |
| 1 | P04Delegaty: delegaty, `Action`/`Func`/`Predicate`, lambdy, multicast, zdarzenia | 40 min |
| 2 | P03: MVVM „ręcznie” – `INotifyPropertyChanged`, własny `RelayCommand`, `MainViewModelClassic` | 50 min |
| 3 | P03: binding w XAML, `Command` zamiast `Click` | 40 min |
| 4 | P03: interfejs `IMeteoService`, `FakeMeteoService`, wzorzec Options (`AppSettings`) | 30 min |
| 5 | P03: CommunityToolkit.Mvvm – `ObservableObject`, `[ObservableProperty]`, `[RelayCommand]` | 40 min |
| 6 | P03: konwertery i zasoby w `App.xaml` | 20 min |
| 7 | P03: Dependency Injection przez Generic Host w `App.xaml.cs` | 50 min |
| 8 | Zadanie Z01AplikacjaNBP.Client: praca własna (45 min) + omówienie rozwiązania (30 min) | 90 min |
| 9 | Podsumowanie, pytania, commit | 15 min |

Przerwy po sekcjach 2, 4 i 7. Sekcja 8 zaczyna się od 45 minut pracy własnej.

### 🧪 Dane testowe (zawsze te same)

| Gdzie | Wartość |
|---|---|
| P04Delegaty | bez wejścia – program wypisuje 5 sekcji |
| P03, miasto | `Warszawa` (Open‑Meteo zwraca 2 wyniki, pierwszy to Warszawa, Województwo mazowieckie), `Kraków` (na liście jest też Krakow w Wisconsin), `xyzxyz` (komunikat „Nie znaleziono miasta”), pole puste (przycisk **Szukaj** wyłączony) |
| P03 z `FakeMeteoService` | `Warszawa` → 1 wynik, `a` → 3 wyniki (Warszawa, Kraków, Gdańsk), `xyzxyz` → brak |
| Z01, przelicznik | kwota `430` PLN, waluta `EUR` → `430,00 PLN = 98,23 EUR` (kurs 4,3775 z tabeli z 2026‑10‑09; w innym dniu wynik zależy od aktualnego kursu) |

## Wymagania i przygotowanie

**Wymagania:**

- Visual Studio 2026 z workloadem **.NET desktop development** (albo VS Code + .NET 10 SDK).
- .NET 10 SDK.
- Dostęp do internetu: `open-meteo.com`, `api.nbp.pl`, `nuget.org`.
- Kod z dnia 11: folder `P03WeatherForecastWPF.Client` (bez folderów `bin` i `obj`). Brakujący kod jest w repozytorium `tomasz-trainer/32Dzien11`.

**Przygotowanie:**

1. Sprawdzamy SDK w **Developer PowerShell** (Visual Studio: **View → Terminal**): `dotnet --version` ma pokazać `10.0.x`.
2. Sprawdzamy w przeglądarce usługi, z których dziś korzystamy:
   - `https://geocoding-api.open-meteo.com/v1/search?name=Warszawa&count=10&language=pl&format=json` → JSON z listą miast,
   - `https://api.nbp.pl/api/exchangerates/tables/A?format=json` → JSON: tablica z jedną tabelą kursów.
3. Tworzymy pusty folder `32Dzien12` i kopiujemy do niego folder `P03WeatherForecastWPF.Client` z dnia 11 (bez `bin` i `obj`).
4. Sprawdzamy, że pakiety NuGet są osiągalne: `CommunityToolkit.Mvvm 8.4.2`, `Microsoft.Extensions.Hosting 10.0.12`, `Microsoft.Extensions.Http 10.0.12`. Przy słabym internecie warto przywrócić je wcześniej (`dotnet restore`) i mieć kopię folderu `%USERPROFILE%\.nuget\packages`.
5. Otwieramy okno **Output** (**View → Output**, źródło **Debug**). W nim widać błędy bindingu.

Solucję i plik `.gitignore` tworzymy w sekcji 1, razem z projektem P04Delegaty. Kto chce od razu sprawdzić, że P03 z dnia 11 działa, może wykonać w folderze `32Dzien12` pierwsze dwa polecenia z sekcji 1, potem `dotnet build`, otworzyć `32Dzien12.slnx` w Visual Studio, ustawić P03 jako projekt startowy (**Set as Startup Project**) i uruchomić F5: po wpisaniu `Warszawa`, kliknięciu **Szukaj** i wybraniu miasta aplikacja ma działać jak wczoraj.

---

## 0. Teoria wstępna: MVVM i Dependency Injection

### Co jest nie tak z kodem z dnia 11?

Aplikacja pogodowa z dnia 11 działa. Gdy jednak otworzymy `MainWindow.xaml.cs`, widzimy 91 linii, a w nich pobieranie z internetu, obsługę błędów, czytanie konfiguracji **i** ustawianie `lblTemperature.Content`. Wszystko jest w jednej klasie, która dziedziczy po `Window`. To daje trzy problemy:

| Problem | Na czym polega |
|---|---|
| **Testowalność** | żeby sprawdzić, czy dla `xyzxyz` pojawia się komunikat, trzeba uruchomić okno i kliknąć. Testu automatycznego nie da się napisać, bo logika żyje w metodzie `btnSearch_Click`, która potrzebuje kontrolek |
| **Duplikacja** | w dniu 18 ta sama aplikacja ma działać na Androidzie (MAUI). Z code‑behind trzeba by skopiować całą logikę do drugiego okna i utrzymywać dwie kopie |
| **Sprzężenie z kontrolkami** | gdy grafik zamieni `Label` na `TextBlock`, kod przestaje się kompilować (`Content` → `Text`). Logika wie za dużo o wyglądzie |

Rozwiązaniem jest wzorzec **MVVM** (Model–View–ViewModel), stworzony przez Microsoft właśnie dla WPF.

### MVVM (Model–View–ViewModel)

💡 **Wyjaśnienie.** Trzy warstwy i trzy mechanizmy, które je łączą:

| Element | Co to jest | U nas |
|---|---|---|
| **Model** | dane i reguły domenowe, nie wie o UI | `City`, `Weather`, `OpenMeteoService` |
| **View** (widok) | XAML, wygląd; code‑behind prawie pusty | `MainWindow.xaml` |
| **ViewModel** | stan okna jako właściwości (`CityName`, `Cities`, `Weather`) i akcje jako komendy (`LoadCitiesCommand`); zwykła klasa C#, nie zna kontrolek | `MainViewModel` |
| **Binding** (wiązanie) | mechanizm WPF, który łączy właściwość kontrolki z właściwością ViewModelu w obie strony | `Text="{Binding CityName}"` |
| **Komenda** (`ICommand`) | obiekt „wykonaj” + „czy można wykonać”, zamiast obsługi `Click` | `Command="{Binding LoadCitiesCommand}"` |
| **Powiadomienie** (`INotifyPropertyChanged`) | zdarzenie, przez które ViewModel mówi widokowi „zmieniłem się, odśwież” | `OnPropertyChanged()` |

ViewModel nie ma referencji do okna. Okno ma referencję do ViewModelu przez `DataContext`. Kierunek zależności jest jeden: widok → ViewModel → model. Dlatego ViewModel można testować w konsoli i użyć ponownie w MAUI.

**Zalety, wady i alternatywy:**

| Podejście | Zalety | Wady |
|---|---|---|
| **Code‑behind** (dzień 11) | najszybsze na start, zero dodatkowych klas; dobre dla małych narzędzi | nietestowalne, nie do ponownego użycia, rośnie bez kontroli |
| **MVP** (Model–View–Presenter, Windows Forms) | testowalny presenter | widok musi implementować interfejs, presenter ręcznie ustawia kontrolki |
| **MVVM** (WPF, MAUI, Avalonia, WinUI) | binding „przepisuje” dane za nas, testowalny ViewModel, jeden ViewModel na wiele platform, grafik może zmieniać XAML bez ruszania C# | więcej plików, trochę kodu powtarzalnego (dziś zobaczymy, jak CommunityToolkit go generuje), błędy bindingu widać dopiero w czasie działania (w oknie Output) |
| **MVU** (ReactiveUI, Comet, Elm) | stan niezmienny, przewidywalny | inny sposób myślenia, mniejszy ekosystem w .NET |
| **Komponenty** (Blazor, dzień 17) | kod i znaczniki w jednym pliku `.razor` | tylko web; w dniu 17 okaże się, że i tam serwisy wstrzykujemy tak samo |

### Dependency Injection (DI, wstrzykiwanie zależności)

W dniu 11 konstruktor okna robił `new OpenMeteoService(new HttpClient(), language)`. Okno samo tworzyło serwis, więc:

- nie da się podmienić serwisu na wersję testową bez zmiany okna,
- gdy serwis dostanie trzeci parametr, trzeba poprawić każde miejsce, gdzie jest `new`,
- `HttpClient` tworzymy ręcznie i musimy pamiętać, że ma być jeden na aplikację.

**Zasada odwrócenia zależności** (Dependency Inversion): klasa nie tworzy swoich zależności, tylko dostaje je **konstruktorem**, najlepiej jako **interfejs**. `MainViewModel(IMeteoService service)` nie wie, czy dostał Open‑Meteo, dane testowe, czy (od jutra) nasze własne API. Obiekty tworzy **kontener DI**. Rejestrujemy w nim „dla `IMeteoService` użyj `OpenMeteoService`”, a kontener sam buduje cały łańcuch: okno → ViewModel → serwis → `HttpClient` + konfiguracja.

💡 **Wyjaśnienie: cykle życia** (wrócą w dniach 13 i 14):

| Rejestracja | Ile obiektów | Kiedy używać |
|---|---|---|
| `AddSingleton<T>()` | jeden na całą aplikację | serwisy bez stanu użytkownika, cache, ViewModel okna głównego |
| `AddTransient<T>()` | nowy przy każdym pobraniu | lekkie obiekty, okna (`MainWindow`) |
| `AddScoped<T>()` | jeden na „zakres” (w ASP.NET Core: jedno żądanie HTTP) | serwisy z `DbContext` w API (dni 13–14); w WPF zakres trzeba tworzyć ręcznie, więc dziś go nie używamy |

⚠️ **Uwaga.** Singleton nie może zależeć od obiektu o krótszym życiu, bo przechowałby go na zawsze. Dlatego `MainViewModel` (singleton) dostaje serwis zarejestrowany też jako singleton albo przez fabrykę `HttpClient` (`AddHttpClient`), która sama dba o odświeżanie połączeń.

**Generic Host** (`Host.CreateApplicationBuilder`) łączy w jednym obiekcie konfigurację (`builder.Configuration`, czyta `appsettings.json`), logowanie (`builder.Logging`) i kontener DI (`builder.Services`). Ten sam obiekt pojawi się jutro w `Program.cs` projektu Web API, a potem w Blazorze i MAUI. Uczymy się go raz.

Zanim powstanie ViewModel, potrzebne są dwa mechanizmy, na których on stoi: komenda to **delegat** opakowany w obiekt, a powiadomienie o zmianie to **zdarzenie**. Dlatego zaczynamy od krótkiego projektu konsolowego.

---

## 1. P04Delegaty: delegaty, lambdy, zdarzenia

### Cel

Zrozumieć, że delegat to „zmienna, w której trzymamy metodę”. Poznać `Action`/`Func`/`Predicate`, multicast i różnicę między `event` a zwykłym delegatem. Zobaczyć, gdzie te mechanizmy wracają w MVVM.

### Teoria

- **Delegat** (delegate) to **typ** opisujący sygnaturę metody (parametry + wynik). Zmienna tego typu trzyma metodę: nazwaną, anonimową albo **lambdę** (lambda expression, np. `c => c + 273.15`). Wywołanie delegatu wygląda jak wywołanie metody.
- **Metoda wyższego rzędu** (higher-order function) przyjmuje delegat jako parametr. Nie wie, *jak* coś zrobić; dostaje „przepis” z zewnątrz. Tak samo `RelayCommand` (sekcja 2) nie wie, co zrobić po kliknięciu; dostaje to z ViewModelu.
- .NET ma gotowe delegaty generyczne, więc zwykle nie deklarujemy własnych typów: `Action<T>` (nic nie zwraca), `Func<T, TResult>` (zwraca wynik), `Predicate<T>` (zwraca `bool`). LINQ to delegaty w praktyce: `Where` przyjmuje `Func<T, bool>`, `Select` przyjmuje `Func<T, TResult>`.
- **Multicast:** jeden delegat może wskazywać wiele metod (`+=` dokłada, `-=` usuwa). Jedno wywołanie uruchamia wszystkie po kolei, w kolejności dodania.
- **`event`** (zdarzenie) to delegat multicast z ograniczeniem: z zewnątrz wolno tylko `+=` i `-=`. Wywołać go (`Invoke`) może tylko klasa, w której jest zadeklarowany. Standardowy delegat zdarzeń w .NET to `EventHandler<T>` z sygnaturą `(object? sender, T e)`.
- **Alternatywa dla delegatu:** interfejs z jedną metodą (wzorzec Strategia, strategy pattern). Interfejs jest lepszy, gdy zachowanie ma stan albo kilka metod; delegat, gdy to jedna funkcja. W LINQ i w komendach WPF wygrały delegaty, bo lambda jest krótsza niż klasa.

### Kroki

1. W terminalu, w folderze `32Dzien12` (jest w nim już skopiowany folder `P03WeatherForecastWPF.Client`), tworzymy solucję, dodajemy P03, tworzymy projekt konsolowy i plik `.gitignore`:

```powershell
dotnet new sln -n 32Dzien12 --format slnx
dotnet sln 32Dzien12.slnx add P03WeatherForecastWPF.Client/P03WeatherForecastWPF.Client.csproj
dotnet new console -n P04Delegaty -o P04Delegaty -f net10.0
dotnet sln 32Dzien12.slnx add P04Delegaty/P04Delegaty.csproj
dotnet new gitignore
```

2. Otwieramy `32Dzien12.slnx` w Visual Studio. Plik projektu P04 jest standardowy:

**Plik:** `P04Delegaty/P04Delegaty.csproj`

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

3. Dodajemy plik `Converters.cs`: własny typ delegata i metoda wyższego rzędu, która przyjmuje delegat jako parametr.

**Plik:** `P04Delegaty/Converters.cs`

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

💡 **Wyjaśnienie.** `delegate double TemperatureConverter(double celsius)` to **typ**. Zmienna tego typu może trzymać każdą metodę, która bierze `double` i zwraca `double`: nazwaną, anonimową albo lambdę. `PrintConverted` nie wie, jak przeliczać; dostaje przepis z zewnątrz.

4. Dodajemy plik `TemperatureSensor.cs`: zdarzenie z własną klasą argumentów.

**Plik:** `P04Delegaty/TemperatureSensor.cs`

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

💡 **Wyjaśnienie.**

- Bez słowa `event` kod spoza klasy mógłby zrobić `sensor.TemperatureChanged = null` i wyrzucić wszystkich subskrybentów albo sam wywołać zdarzenie.
- `TemperatureChangedEventArgs` dziedziczy po `EventArgs` (konwencja .NET) i ma **konstruktor podstawowy** (primary constructor, C# 12): parametry w nagłówku klasy, przypisane do właściwości.
- `TemperatureChanged?.Invoke(...)`: gdy nikt nie subskrybuje, delegat jest `null`. Operator `?.` chroni przed `NullReferenceException`.
- Setter `Temperature` sprawdza, czy wartość się zmieniła, i dopiero wtedy powiadamia. Dokładnie taki setter powstanie w sekcji 2 w ViewModelu, tylko zdarzenie będzie się nazywać `PropertyChanged`.

5. Zastępujemy `Program.cs`:

**Plik:** `P04Delegaty/Program.cs`

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

6. Uruchamiamy: `dotnet run --project P04Delegaty`.

### ✅ Punkt kontrolny

Wyjście ma wyglądać dokładnie tak:

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

W sekcji 4 wyjścia są dokładnie trzy linie „Zmiana” (nie cztery: drugie przypisanie `35.0` nie zmienia wartości) i jedna linia „ALARM” (po `-=` alarm milczy przy `40.0`).

Omówienie wyjścia, sekcja po sekcji:

1. **Własny delegat.** `toFahrenheit` wskazuje metodę nazwaną, `toKelvin` lambdę. `PrintConverted([0, 10, 25], toFahrenheit)` przekazuje *zachowanie* jako argument.
2. **Action / Func / Predicate.** `Action<string> print = Console.WriteLine` to **grupa metod** (method group): kompilator sam wybiera przeciążenie z parametrem `string`. `Where(n => isEven(n)).Select(n => n * n)` to dwa delegaty przekazane do LINQ.
3. **Multicast.** Jedno wywołanie `notify(...)` uruchamia trzy metody po kolei, w kolejności dodania.
4. **Zdarzenia.** Dwóch subskrybentów: lambda i metoda `Alarm.OnTemperatureChanged`. Trzecie przypisanie (`35.0` drugi raz) nie wypisuje nic, bo setter wykrył brak zmiany. Po `-=` alarm milczy przy `40.0`.
5. **Związek z MVVM.** To mapa na dziś: `PropertyChanged` jest zdarzeniem, `RelayCommand` trzyma `Action` i `Predicate`, a `Click="..."` z dnia 11 to też delegat (`RoutedEventHandler`), tylko WPF podpinał go za nas.

### Dlaczego tak

- Delegat jako parametr oddziela „co zrobić” od „kiedy to zrobić”. Na tym samym pomyśle stoi `ICommand`: przycisk wie *kiedy*, ViewModel wie *co*.
- Multicast z wartością zwracaną zwraca wynik tylko ostatniej metody, a wyjątek w jednej metodzie przerywa wywołanie kolejnych. Dlatego zdarzenia zwykle są `void`.
- `event` zamiast publicznego pola delegatu chroni listę subskrybentów: z zewnątrz można się tylko zapisać i wypisać.

🔧 **Zmiana względem poprzednich edycji.** Edycja 31 miała w P04 tylko cztery przypisania `Action`/`Func`/`Predicate` bez kontekstu. Tu jest własny typ delegata, metoda wyższego rzędu, LINQ, multicast, pełne zdarzenie z `EventArgs` i wypisanie się, a na końcu mapa „gdzie to spotkamy w WPF”. Dzięki temu `RelayCommand` i `INotifyPropertyChanged` w sekcji 2 nie są magią.

### Częste problemy

| Objaw | Rozwiązanie |
|---|---|
| `°` wyświetla się jako krzaczki | brakuje `Console.OutputEncoding = UTF8` (jest w pierwszej linii kodu) |
| błąd `cannot be accessed with an instance reference` przy `Alarm.OnTemperatureChanged` | `Alarm` jest klasą statyczną; subskrybujemy bez `new` |
| `TemperatureChanged` nie da się wywołać spoza klasy | tak ma być: to `event`; wywołuje go tylko `TemperatureSensor` |

Mamy klocki: delegat, zdarzenie, lambda. Budujemy z nich ViewModel. Najpierw ręcznie, żeby zobaczyć każdą śrubkę, potem biblioteką, która wygeneruje to za nas.

---

## 2. P03: MVVM „ręcznie”

### Cel

Napisać ViewModel z `INotifyPropertyChanged` i własnym `RelayCommand`. Zrozumieć, co robi każda linia, i zobaczyć, ile w tym kodu powtarzalnego.

### Teoria

- `INotifyPropertyChanged` to jeden interfejs z jednym zdarzeniem: `PropertyChanged`. Binding WPF subskrybuje je przy starcie i po każdym `OnPropertyChanged("Weather")` odczytuje `Weather` na nowo. Bez powiadomienia binding pokaże wartość początkową i nigdy jej nie odświeży.
- `[CallerMemberName]` to atrybut z `System.Runtime.CompilerServices`. Kompilator wstawia w miejsce parametru nazwę składowej, z której wołamy metodę. Wywołanie `OnPropertyChanged()` w setterze `CityName` wysyła do zdarzenia `"CityName"`. Zero literówek w napisach.
- `ICommand` ma trzy składowe: `Execute` (co zrobić), `CanExecute` (czy wolno) i zdarzenie `CanExecuteChanged` (sygnał dla przycisku: „zapytaj ponownie”). Nasz `RelayCommand` opakowuje dwa delegaty z sekcji 1: `Action<object?>` i `Predicate<object?>`.
- Interfejs serwisu (`IMeteoService`) pozwala podmienić implementację. Gdyby ViewModel znał `OpenMeteoService`, byłby związany z jedną implementacją. Dlatego od razu powstaje druga implementacja, `FakeMeteoService`, która nie potrzebuje internetu. Dzięki niej etap „ręczny” uruchomimy bez `HttpClient` i bez konfiguracji; prawdziwy serwis podepniemy pod interfejs w sekcji 4.

### Kroki

#### 2.1 Pakiety NuGet

1. Prawy przycisk na projekcie P03 → **Manage NuGet Packages**. Usuwamy `Microsoft.Extensions.Configuration.Json`, instalujemy `CommunityToolkit.Mvvm` (8.4.2), `Microsoft.Extensions.Hosting` (10.0.12) i `Microsoft.Extensions.Http` (10.0.12). To samo z konsoli:

```powershell
dotnet remove P03WeatherForecastWPF.Client package Microsoft.Extensions.Configuration.Json
dotnet add P03WeatherForecastWPF.Client package CommunityToolkit.Mvvm --version 8.4.2
dotnet add P03WeatherForecastWPF.Client package Microsoft.Extensions.Hosting --version 10.0.12
dotnet add P03WeatherForecastWPF.Client package Microsoft.Extensions.Http --version 10.0.12
```

🟩 **Co dopisujemy** w `P03WeatherForecastWPF.Client/P03WeatherForecastWPF.Client.csproj`:

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

**Plik:** `P03WeatherForecastWPF.Client/P03WeatherForecastWPF.Client.csproj`

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

💡 **Wyjaśnienie.** Wszystkie pakiety instalujemy od razu, żeby nie przerywać pracy później. `Microsoft.Extensions.Hosting` ciągnie za sobą konfigurację (w tym `Configuration.Json`, dlatego go usuwamy), logowanie, `Options` i kontener DI. `Microsoft.Extensions.Http` dokłada `AddHttpClient`. `CommunityToolkit.Mvvm` użyjemy w sekcji 5; teraz piszemy ręcznie. `Hosting` i `Http` przydadzą się w sekcjach 4 i 7.

#### 2.2 Własny `RelayCommand`

2. Tworzymy folder `Commands` (prawy przycisk na projekcie → **Add → New Folder**) i w nim klasę `RelayCommand.cs` (**Add → Class**):

**Plik:** `P03WeatherForecastWPF.Client/Commands/RelayCommand.cs`

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

💡 **Wyjaśnienie.**

- `CanExecuteChanged` nie trzyma własnej listy subskrybentów. Przekazuje `add`/`remove` do `CommandManager.RequerySuggested`. WPF podnosi to zdarzenie po każdej interakcji (kliknięcie, zmiana fokusu, naciśnięcie klawisza). Dzięki temu przycisk sam się wyłącza, gdy `CanExecute` zwróci `false`, bez jednej linii w oknie. Koszt: WPF pyta często, więc `CanExecute` ma być tanie.
- `object? parameter`: komenda może dostać parametr z XAML (`CommandParameter`). Dziś go nie używamy, stąd `_` w lambdach.

#### 2.3 Interfejs serwisu i wersja testowa

3. W folderze `Services` dodajemy interfejs i implementację testową:

**Plik:** `P03WeatherForecastWPF.Client/Services/IMeteoService.cs`

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

**Plik:** `P03WeatherForecastWPF.Client/Services/FakeMeteoService.cs`

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

💡 **Wyjaśnienie.** Sygnatury w interfejsie są **identyczne** z metodami `OpenMeteoService` z dnia 11. Visual Studio zrobiłoby to samo automatycznie: prawy przycisk na klasie → **Quick Actions and Refactorings → Extract Interface**. `FakeMeteoService` ma `Task.Delay(500)`, żeby było widać pasek postępu.

#### 2.4 `MainViewModelClassic`

4. Tworzymy folder `ViewModels`, w nim podfolder `Classic`, a w nim klasę `MainViewModelClassic.cs`:

**Plik:** `P03WeatherForecastWPF.Client/ViewModels/Classic/MainViewModelClassic.cs`

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

💡 **Wyjaśnienie** (od góry pliku):

1. **`INotifyPropertyChanged`** i metoda `OnPropertyChanged` z `[CallerMemberName]` – opisane w części Teoria.
2. **Pole + właściwość.** Każda właściwość to 8 linii: pole, getter, setter z powiadomieniem. Sześć właściwości daje 50 linii, które wyglądają tak samo. Ten „ból” zniknie w sekcji 5.
3. **`SelectedCity`.** Setter robi coś więcej: po powiadomieniu uruchamia `LoadWeatherAsync()`. Wybór na liście = pobranie pogody; w dniu 11 robiła to obsługa `SelectionChanged`. `_ = ...` to **discard**: informacja dla kompilatora „wiem, że nie czekam na ten `Task`”. Alternatywa `async void` jest gorsza: wyjątek z `async void` zabija aplikację, a z `Task` można go złapać w środku metody.
4. **Komenda.** `LoadCitiesCommand` dostaje dwie lambdy: `execute` uruchamia pobieranie, `canExecute` zwraca `false`, gdy trwa pobieranie albo pole jest puste. Przycisk w oknie sam to odczyta.
5. **`IsBusy` i `ErrorMessage`.** W dniu 11 było `btnSearch.IsEnabled = false` i `tbStatus.Text = ...`. Teraz ViewModel ustawia **stan** (`IsBusy = true`), a widok decyduje, jak go pokazać: wyłączony przycisk, pasek postępu, kursor. ViewModel nie zna żadnej kontrolki, więc da się go przetestować w konsoli.
6. **`catch (Exception)`.** Łapiemy szerzej niż w dniu 11 (`HttpRequestException`), bo serwis za interfejsem może rzucić cokolwiek: błąd sieci, przekroczenie czasu (`TaskCanceledException`), błąd JSON. ViewModel zamienia każdy wyjątek na komunikat.

#### 2.5 Tymczasowe podpięcie w code‑behind

5. Zastępujemy `MainWindow.xaml.cs` w całości (zmienia się wszystko, więc bez bloku 🟩). To **wersja tymczasowa**: ViewModel tworzymy ręcznie; w sekcji 7 zrobi to kontener DI.

**Plik:** `P03WeatherForecastWPF.Client/MainWindow.xaml.cs` (wersja tymczasowa)

```csharp
// P03WeatherForecastWPF.Client/MainWindow.xaml.cs  (wersja tymczasowa – pełna w sekcji 7)
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

💡 **Wyjaśnienie.** 91 linii zamieniło się w jedną: `DataContext = viewModel`. `DataContext` to obiekt, do którego odnoszą się wszystkie `{Binding}` w tym oknie i jego dzieciach. Celowo wstawiamy `FakeMeteoService`: w wersji końcowej `OpenMeteoService` będzie wymagał `IOptions<AppSettings>`, którego jeszcze nie mamy, a etap pośredni ma się kompilować. ViewModel i tak nie widzi różnicy, bo zna tylko interfejs. Na tym polega DI, choć na razie robimy je ręcznie, słowem `new`.

⚠️ **Uwaga.** Po tym kroku projekt **nie zbuduje się**: `MainWindow.xaml` nadal odwołuje się do `btnSearch_Click` i `lbCities_SelectionChanged`, których już nie ma. To dobrze: kompilator pilnuje, żeby widok nie wisiał na usuniętym kodzie. Naprawiamy to w sekcji 3.

### ✅ Punkt kontrolny

- Błąd kompilacji dotyczy **tylko** `MainWindow.xaml` (`does not contain a definition for 'btnSearch_Click'`). Inne błędy zwykle oznaczają złą przestrzeń nazw (`P03WeatherForecastWPF.Client.Commands`, `.Services`, `.ViewModels.Classic`).
- `MainViewModelClassic` nie ma żadnego `using System.Windows.Controls`: ViewModel nie zna kontrolek.

### Dlaczego tak

- **Pole + właściwość + `OnPropertyChanged()`** w każdej właściwości pokazuje, co dzieje się pod spodem. Ten sam kod w sekcji 5 wygeneruje biblioteka.
- **`async Task` + discard** zamiast `async void`: wyjątek nie wywraca aplikacji.
- **`IsBusy` i `ErrorMessage`** zamiast ustawiania kontrolek: ViewModel opisuje stan, widok go prezentuje.
- **`CanExecuteChanged` przez `CommandManager.RequerySuggested`**: przycisk sam się włącza i wyłącza, bez kodu w oknie.
- **`FakeMeteoService` w wersji tymczasowej**: etap ręcznego MVVM działa bez internetu i bez konfiguracji.

### Częste problemy

| Objaw | Rozwiązanie |
|---|---|
| `does not contain a definition for 'btnSearch_Click'` | oczekiwane po tym kroku; znika po sekcji 3 |
| `The type or namespace name 'RelayCommand' could not be found` | brak `using P03WeatherForecastWPF.Client.Commands;` albo inna przestrzeń nazw w pliku `RelayCommand.cs` |
| ostrzeżenia nullable przy `RelayCommand` | parametry muszą mieć `?`: `Action<object?>`, `Predicate<object?>?` |

Okno ma teraz ViewModel, ale żadna kontrolka jeszcze o nim nie wie. Pora na binding.

---

## 3. P03: binding w XAML

### Cel

Zbindować tekst, listę, zaznaczenie, widoczność i komendę – bez `x:Name`, `Click` i `SelectionChanged`. Umieć znaleźć błąd bindingu.

### Teoria

💡 **Wyjaśnienie: składnia bindingu.**

| Składnia | Co robi | Komentarz |
|---|---|---|
| `{Binding CityName}` | łączy właściwość kontrolki z właściwością `CityName` obiektu z `DataContext` | `DataContext` dziedziczy się w dół drzewa: ustawiony na oknie, działa w każdym dziecku |
| `Mode` (nie piszemy) | kierunek: `OneWay` (VM → widok), `TwoWay` (oba), `OneTime` | `TextBox.Text`, `SelectedItem` są domyślnie `TwoWay`; `Label.Content` jest `OneWay` |
| `UpdateSourceTrigger=PropertyChanged` | kiedy widok zapisuje do VM: po każdej literze zamiast po opuszczeniu pola (`LostFocus`) | bez tego `CanExecute` zobaczy pusty tekst dopiero po kliknięciu gdzie indziej |
| `ItemsSource="{Binding Cities}"` + `SelectedItem="{Binding SelectedCity}"` | lista bierze elementy z kolekcji, a zaznaczony element zapisuje do VM | zastępuje `lbCities.ItemsSource = cities` i `SelectionChanged` |
| `SelectedCity.FullName` | ścieżka przez kilka właściwości | gdy `SelectedCity` jest `null`, binding nie rzuca wyjątku, tylko loguje błąd i pokazuje `FallbackValue` |
| `StringFormat={}{0:F1} °C` | formatowanie jak w `string.Format` | `{}` na początku to „escape”: bez niego XAML uznałby `{0:F1}` za rozszerzenie znaczników (markup extension). `StringFormat=g` dla daty nie potrzebuje `{}` |
| `FallbackValue=—` | tekst, gdy ścieżka bindingu nie działa (np. `Weather` jest `null`) | w dniu 11 `—` było wpisane w `Content` |
| `Command="{Binding LoadCitiesCommand}"` | przycisk woła `Execute`, a `IsEnabled` bierze z `CanExecute` | nie ma `Click`; przycisk wyłączony = `CanExecute` zwrócił `false` |
| `KeyBinding Key="Enter"` | ta sama komenda z klawiatury | w code‑behind byłoby `KeyDown` z `if (e.Key == Key.Enter)` |
| `Converter={StaticResource BoolToVisibility}` | `bool` → `Visibility`, bo `ProgressBar.Visibility` nie przyjmie `true` | wbudowany `BooleanToVisibilityConverter`; własne konwertery powstaną w sekcji 6 |
| `d:DataContext="{d:DesignInstance Type=vm:MainViewModelClassic}"` | tylko dla edytora: IntelliSense podpowiada nazwy właściwości i podkreśla literówki | prefiks `d:` jest ignorowany w czasie działania (`mc:Ignorable="d"`) |

### Kroki

1. Zastępujemy `MainWindow.xaml` w całości (wszystkie kontrolki dostają binding, więc bez bloku 🟩). To wersja tymczasowa, bez własnych konwerterów, które dojdą w sekcji 6.

**Plik:** `P03WeatherForecastWPF.Client/MainWindow.xaml` (wersja tymczasowa)

```xml
<!-- P03WeatherForecastWPF.Client/MainWindow.xaml  (wersja tymczasowa – pełna w sekcji 6) -->
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

Zniknęły wszystkie `x:Name`, `Click` i `SelectionChanged`. Każda kontrolka mówi tylko, **z czym** jest związana, nie **co robić**.

2. Uruchamiamy F5. Wpisujemy `Warszawa` i klikamy **Szukaj** (albo naciskamy Enter). Pasek postępu widać przez pół sekundy, lista ma jeden wynik (dane z `FakeMeteoService`), a po kliknięciu miasta pojawia się losowa temperatura.
3. Kasujemy tekst w polu i obserwujemy przycisk: **Szukaj** gaśnie. Wpisujemy `a`: trzy miasta.
4. 🧪 **Eksperyment: błąd bindingu** (ok. 3 min). Zmieniamy w XAML `{Binding CityName}` na `{Binding CityNam}`, uruchamiamy F5 i otwieramy okno **Output** (**View → Output**, źródło **Debug**). Widać linię:

   `System.Windows.Data Error: 40 : BindingExpression path error: 'CityNam' property not found on 'object' ''MainViewModelClassic'`

   Binding nie rzuca wyjątku, tylko loguje. Aplikacja działa, ale pole jest „martwe”. Gdy coś „nie odświeża się”, okno Output to pierwsze miejsce do sprawdzenia. Druga ochrona to `d:DataContext`: z nim edytor podkreśli `CityNam` na niebiesko. Przywracamy `CityName`.

### ✅ Punkt kontrolny

- `Warszawa` → pasek postępu przez pół sekundy, lista z jednym wynikiem, po kliknięciu losowa temperatura, wiatr, opis, czas.
- `a` → trzy miasta (Warszawa, Kraków, Gdańsk). `xyzxyz` → czerwony komunikat „Nie znaleziono miasta o takiej nazwie”.
- Puste pole → przycisk **Szukaj** wyłączony.
- `MainWindow.xaml.cs` ma jedną linię logiki.
- W oknie Output nie ma linii `System.Windows.Data Error` (poza tą z eksperymentu w kroku 4).

### Dlaczego tak

- Kontrolki wiedzą tylko, z czym są związane. Dzięki temu w sekcji 5 podmienimy ViewModel na inny, a XAML nie zmieni ani jednego bindingu.
- Binding loguje błędy zamiast rzucać wyjątki. Okno Output i `d:DataContext` to dwa sposoby, żeby je wyłapać.
- `BoolToVisibility` jest na razie w `Window.Resources`; w sekcji 6 zasoby przeniesiemy do `App.xaml`.

🔧 **Zmiana względem poprzednich edycji.** W edycjach 26/31 `CityName` w ViewModelu nie wywoływał `OnPropertyChanged`, `TextBox` był bez `UpdateSourceTrigger`, nie było `FallbackValue`, `KeyBinding` ani `d:DataContext`, a `Button` nie miał `CanExecute` (kliknięcie przy pustym polu wysyłało zapytanie o pusty napis). Teraz okno reaguje na każdą literę, a przycisk wie, kiedy ma być wyłączony.

### Częste problemy

| Objaw | Rozwiązanie |
|---|---|
| przycisk **Szukaj** zawsze wyłączony | `CanExecute` zwraca `false`: sprawdzamy, czy `CityName` ma wartość początkową `"Warszawa"` i czy `IsBusy` wraca na `false` w `finally` |
| przycisk włącza się dopiero po kliknięciu gdzie indziej | brak `UpdateSourceTrigger=PropertyChanged` |
| lista pokazuje `P03WeatherForecastWPF.Client.Models.City` | brak `DisplayMemberPath="FullName"` |
| temperatura pokazuje `{0:F1} °C` dosłownie | brak `{}` przed `{0:F1}` |
| `XamlParseException: Cannot find resource named 'BoolToVisibility'` | brak `<Window.Resources>` z `BooleanToVisibilityConverter` |

Okno działa na danych „na niby”. Teraz podłączamy prawdziwy serwis, ale tak, żeby ViewModel tego nie zauważył.

---

## 4. P03: `OpenMeteoService` za interfejsem i wzorzec Options

### Cel

Prawdziwy serwis implementuje `IMeteoService`. Ustawienia wchodzą jako silnie typowany obiekt `AppSettings` zamiast tekstowego klucza.

### Teoria

- W dniu 11 czytaliśmy `configuration["AppSettings:DefaultLanguage"]`: napis, w którym literówka wychodzi dopiero w czasie działania.
- **Wzorzec Options** (options pattern): klasa, której właściwości odpowiadają kluczom w sekcji JSON (`AppSettings:DefaultLanguage` → `AppSettings.DefaultLanguage`). Kontener zbinduje sekcję do obiektu i poda go jako `IOptions<AppSettings>`. `IOptions<T>` to opakowanie z jedną właściwością `Value`.
- Odmiany: `IOptionsSnapshot<T>` (odczyt na nowo w każdym zakresie) i `IOptionsMonitor<T>` (powiadamia o zmianie pliku). Do ustawień czytanych raz wystarczy `IOptions<T>`.
- `Options.Create(obiekt)` to statyczna metoda z `Microsoft.Extensions.Options`, która opakowuje gotowy obiekt w `IOptions<T>` bez kontenera. Przydaje się w testach i na tym etapie: kontenera jeszcze nie ma, a serwis chce `IOptions`.

### Kroki

#### 4.1 Klasa ustawień

1. Tworzymy folder `Configuration` i klasę `AppSettings.cs`:

**Plik:** `P03WeatherForecastWPF.Client/Configuration/AppSettings.cs`

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

#### 4.2 Serwis

2. Zmieniamy `Services/OpenMeteoService.cs`. Są trzy zmiany: `: IMeteoService`, drugi parametr konstruktora to `IOptions<AppSettings>` zamiast `string`, a język bierzemy z `options.Value.DefaultLanguage`.

🟩 **Co dopisujemy** w `P03WeatherForecastWPF.Client/Services/OpenMeteoService.cs`:

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

**Plik:** `P03WeatherForecastWPF.Client/Services/OpenMeteoService.cs`

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

#### 4.3 Tymczasowe podpięcie prawdziwego serwisu

3. W `MainWindow.xaml.cs` tworzymy prawdziwy serwis, nadal ręcznie:

🟩 **Co dopisujemy** w `P03WeatherForecastWPF.Client/MainWindow.xaml.cs`:

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

**Plik:** `P03WeatherForecastWPF.Client/MainWindow.xaml.cs` (wersja tymczasowa)

```csharp
// P03WeatherForecastWPF.Client/MainWindow.xaml.cs  (wersja tymczasowa – pełna w sekcji 7)
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

4. Uruchamiamy F5 i testujemy: `Warszawa` → 2 wyniki, klikamy pierwszy: prawdziwa temperatura. `Kraków` → lista z Krakowem w Wisconsin. `xyzxyz` → „Nie znaleziono miasta o takiej nazwie”.
5. Zamieniamy komentarze w konstruktorze (wersja z `FakeMeteoService`), uruchamiamy i sprawdzamy, że okno działa tak samo. Przywracamy wersję z `OpenMeteoService`.

### ✅ Punkt kontrolny

- `Warszawa` → 2 wyniki, pierwszy to Warszawa, Województwo mazowieckie (Polska); po kliknięciu prawdziwa temperatura.
- `Kraków` → na liście jest też Krakow w Wisconsin.
- `xyzxyz` → „Nie znaleziono miasta o takiej nazwie”.
- Z `FakeMeteoService` okno działa tak samo. Podmiana serwisu to zmiana jednej linii w konstruktorze okna; ViewModel i XAML są nietknięte.

### Dlaczego tak

- Logika pobierania w `OpenMeteoService` jest **identyczna** jak w dniu 11. Zmienił się tylko sposób, w jaki serwis dostaje ustawienia.
- `SectionName` jako stała: nazwa sekcji jest w jednym miejscu. Wartość domyślna `"en"` działa, gdy klucza nie ma w pliku.
- `Options.Create` robi to samo, co później zrobi kontener DI, tylko wartości kontener weźmie z `appsettings.json` (sekcja 7). Na razie język jest wpisany na sztywno.
- W konstruktorze okna są teraz cztery `new`: `HttpClient`, `AppSettings`, `OpenMeteoService`, `MainViewModelClassic`. To dużo „ręcznego” tworzenia obiektów. W sekcji 7 wszystkie znikną.

### Częste problemy

| Objaw | Rozwiązanie |
|---|---|
| `'OpenMeteoService' does not implement interface member 'GetCurrentWeatherAsync'` | sygnatury w `IMeteoService` muszą być identyczne z metodami serwisu (w tym `Task<Weather?>`) |
| `The type or namespace name 'Options' could not be found` | brakuje `using Microsoft.Extensions.Options;` albo pakietu `Microsoft.Extensions.Hosting` |
| nazwy krajów po angielsku | sprawdzamy, czy `DefaultLanguage` w `Options.Create` ma `"pl"` |

`MainViewModelClassic` ma 146 linii, z czego 60 to powtarzalne settery. Biblioteka CommunityToolkit.Mvvm wygeneruje je za nas w czasie kompilacji.

---

## 5. P03: CommunityToolkit.Mvvm

### Cel

Ten sam ViewModel bez kodu powtarzalnego, generowany w czasie kompilacji. Zrozumieć, co generator dopisuje i gdzie to podejrzeć.

### Teoria

- **CommunityToolkit.Mvvm** (biblioteka Microsoftu) daje `ObservableObject` (gotowy `INotifyPropertyChanged`) i **generator kodu** (source generator). Generator na podstawie atrybutów dopisuje do klasy `partial` brakujący kod w czasie kompilacji.
- `[ObservableProperty]` na **właściwości częściowej** (partial property, C# 13, toolkit 8.4): deklarujemy tylko nagłówek `public partial string CityName { get; set; }`, a generator dopisuje pole, getter i setter z powiadomieniem.
- Do wersji 8.3 toolkitu atrybut stawiało się na **polu** z małej litery: `[ObservableProperty] private string cityName;`, a generator tworzył właściwość `CityName`. Działało, ale myliło: w kodzie pisało się `cityName` (pole, bez powiadomienia) albo `CityName` (właściwość) i łatwo było o błąd. Tak było w edycjach 26 i 31. Od C# 13 język ma właściwości częściowe i toolkit 8.4 z nich korzysta: jedna nazwa, zero pola. Wymaga .NET 9+ (używamy .NET 10).
- `[NotifyCanExecuteChangedFor(nameof(LoadCitiesCommand))]`: po zmianie właściwości komenda podnosi `CanExecuteChanged`.
- `[RelayCommand(CanExecute = nameof(CanLoadCities))]` na metodzie `LoadCitiesAsync` generuje właściwość `LoadCitiesCommand` typu `IAsyncRelayCommand`; końcówka `Async` jest obcinana.
- `partial void OnSelectedCityChanged(City? value)` to „hak” (hook) wołany przez wygenerowany setter po zmianie wartości.
- `ObservableCollection<T>` sama powiadamia listę o `Add`/`Clear`, więc nie trzeba podmieniać całej kolekcji.

💡 **Wyjaśnienie: porównanie z wersją klasyczną.**

| Element | Co robi | Odpowiednik w `MainViewModelClassic` |
|---|---|---|
| `: ObservableObject` | gotowa implementacja `INotifyPropertyChanged` z `OnPropertyChanged`, `SetProperty` | zdarzenie + metoda `OnPropertyChanged` |
| `partial class` | generator kodu dopisuje drugą część klasy w czasie kompilacji | – |
| `[ObservableProperty] public partial string CityName { get; set; }` | deklarujemy nagłówek, generator dopisuje pole, getter i setter z powiadomieniem | 8 linii: pole `_cityName`, getter, setter z `OnPropertyChanged()` |
| `[NotifyCanExecuteChangedFor(nameof(LoadCitiesCommand))]` | po zmianie tej właściwości komenda podnosi `CanExecuteChanged` | `CommandManager.RequerySuggested` (WPF pytał „na wszelki wypadek” przy każdym kliknięciu) |
| `[RelayCommand(CanExecute = nameof(CanLoadCities))]` na `LoadCitiesAsync` | generator tworzy właściwość `LoadCitiesCommand` typu `IAsyncRelayCommand` | `new RelayCommand(execute, canExecute)` w konstruktorze |
| `partial void OnSelectedCityChanged(City? value)` | „hak” wołany przez wygenerowany setter po zmianie wartości | kod w setterze `SelectedCity` po `OnPropertyChanged()` |
| `ObservableCollection<City> Cities { get; } = []` | kolekcja, która sama powiadamia listę o `Add`/`Clear`; właściwość tylko do odczytu, bo podmieniamy zawartość, nie obiekt | `City[]` z powiadomieniem przy podmianie tablicy |

**Wady i alternatywy.** Generator to zależność od biblioteki i od wersji C#. Błędy w atrybutach pojawiają się jako diagnostyki `MVVMTK0xxx` i trzeba je umieć czytać. Alternatywy: Prism, ReactiveUI (bardziej „reaktywne”, trudniejsze), DevExpress MVVM. CommunityToolkit jest od Microsoftu i działa w WPF, MAUI, WinUI, Avalonii i Blazorze, dlatego używamy go do końca kursu.

### Kroki

1. W folderze `ViewModels` dodajemy `MainViewModel.cs`. Wersja klasyczna zostaje w `Classic` do porównania.

**Plik:** `P03WeatherForecastWPF.Client/ViewModels/MainViewModel.cs`

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

#### 5.1 Przełączenie okna na nowy ViewModel

2. W `MainWindow.xaml.cs` zamieniamy `MainViewModelClassic` na `MainViewModel`:

🟩 **Co dopisujemy** w `P03WeatherForecastWPF.Client/MainWindow.xaml.cs`:

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

**Plik:** `P03WeatherForecastWPF.Client/MainWindow.xaml.cs` (wersja tymczasowa)

```csharp
// P03WeatherForecastWPF.Client/MainWindow.xaml.cs  (wersja tymczasowa – pełna w sekcji 7)
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

3. W `MainWindow.xaml` zmieniamy tylko przestrzeń nazw w `xmlns:vm` i typ w `d:DataContext`:

🟩 **Co dopisujemy** w `P03WeatherForecastWPF.Client/MainWindow.xaml`:

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

**Plik:** `P03WeatherForecastWPF.Client/MainWindow.xaml` (wersja tymczasowa)

```xml
<!-- P03WeatherForecastWPF.Client/MainWindow.xaml  (wersja tymczasowa – pełna w sekcji 6) -->
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

4. Uruchamiamy F5 i powtarzamy testy z sekcji 4: `Warszawa`, `Kraków`, `xyzxyz`, puste pole.
5. Oglądamy wygenerowany kod: **Solution Explorer** → P03 → **Dependencies → Analyzers → CommunityToolkit.Mvvm.SourceGenerators** → `P03WeatherForecastWPF.Client.ViewModels.MainViewModel.g.cs` (osobne pliki dla właściwości i komend). Szukamy settera `CityName` i właściwości `LoadCitiesCommand`. To dokładnie to, co powstało ręcznie w sekcji 2: setter z `OnPropertyChanging` i `OnPropertyChanged`, wywołanie `OnSelectedCityChanged`, `LoadCitiesCommand` jako `AsyncRelayCommand`. Nic magicznego, tylko mniej pisania.

### ✅ Punkt kontrolny

- Okno działa identycznie jak w sekcji 4: `Warszawa`, `Kraków`, `xyzxyz`, puste pole.
- `MainWindow.xaml` nie zmienił żadnego bindingu (tylko `d:DataContext`), bo nazwy właściwości i komendy są takie same. Lista działa z `ObservableCollection` tak samo jak z tablicą.
- W wygenerowanym pliku widać `OnPropertyChanged`, `OnSelectedCityChanged` i `new AsyncRelayCommand(...)`.
- Klasyczna wersja zostaje w `ViewModels/Classic` jako materiał porównawczy: nie jest nigdzie używana, ale się kompiluje.

### Dlaczego tak

- `MainViewModelClassic` + `RelayCommand` = 146 + 36 linii; `MainViewModel` = 120 linii **z komentarzami** (bez nich ok. 80). Różnica rośnie z każdą właściwością. Ważniejsze: w wersji z toolkitem nie da się zapomnieć `OnPropertyChanged`.
- `Cities` jako `ObservableCollection { get; }` (tylko do odczytu): podmieniamy zawartość (`Clear` + `Add`), nie obiekt. Lista nie musi odpinać się od starej kolekcji.
- `private async Task LoadCitiesAsync()` zamiast `async void`: wyjątek wraca do `await`, a `AsyncRelayCommand` sam blokuje ponowne wykonanie w trakcie.

🔧 **Zmiana względem poprzednich edycji.** Edycje 26/31 miały `[RelayCommand] public async void LoadCities()` (wyjątek z `async void` wywraca aplikację, brak `CanExecute`), `[ObservableProperty]` na polach z małej litery, a `Cities` raz jako `ObservableCollection` podmienianą w całości, raz z `Clear`/`Add`. Teraz: `private async Task` + `CanExecute`, właściwości częściowe, kolekcja tylko do odczytu, `IsBusy` i `ErrorMessage` z `try/catch/finally`.

### Częste problemy

| Objaw | Rozwiązanie |
|---|---|
| `MVVMTK0045` / błąd przy `partial string` | wymagany C# 13+ (`net10.0` go ma) i toolkit 8.4+; sprawdzamy wersję pakietu |
| `'MainViewModel' does not contain a definition for 'LoadCitiesCommand'` | klasa nie jest `partial` albo metoda nie ma `[RelayCommand]`; budujemy projekt (generator działa przy kompilacji) |
| pogoda nie pobiera się po wyborze miasta | `OnSelectedCityChanged` musi być `partial void` z dokładnie tą nazwą i typem parametru `City?` |
| przycisk nie wyłącza się przy pustym polu | brak `[NotifyCanExecuteChangedFor(nameof(LoadCitiesCommand))]` na `CityName` |

Temperatura w XAML ma `StringFormat`. Co jednak, gdy formatowanie jest bardziej złożone, np. `null` ma ukryć kontrolkę? Do tego służą konwertery.

---

## 6. P03: konwertery i zasoby aplikacji

### Cel

`IValueConverter` jako most między typem w ViewModelu a typem, którego oczekuje kontrolka. Zasoby wspólne na poziomie aplikacji (`App.xaml`).

### Teoria

- **`IValueConverter`** ma dwie metody: `Convert` (ViewModel → widok) i `ConvertBack` (widok → ViewModel). Nasze bindingi są jednokierunkowe, więc `ConvertBack` rzuca `NotSupportedException`: lepiej głośno, niż cicho zwrócić coś dziwnego.
- Parametr `culture` to kultura z bindingu. `ToString("F1", culture)` da `12,5` na polskim Windowsie i `12.5` na angielskim. Formatowanie **dla człowieka** może zależeć od kultury; parsowanie danych z API (dzień 11) nie.
- `value is string { Length: 0 }` to wzorzec właściwości (property pattern, C# 8): dopasowanie, gdy `value` jest napisem o długości zero.
- Konwerter jako zasób aplikacji to **jedna instancja** na aplikację, dostępna w każdym oknie przez `StaticResource`.
- **Kiedy konwerter, a kiedy właściwość w ViewModelu?** Czysta prezentacja (format, kolor, widoczność) → konwerter, bo ViewModel ma zostać niezależny od WPF. Logika (np. „czy pokazać ostrzeżenie o mrozie”) → właściwość `bool` w ViewModelu, a w widoku tylko `BoolToVisibility`. Trzecia droga to `DataTrigger` w stylu; działa bez kodu C#, ale jest rozwlekła.

### Kroki

1. Tworzymy folder `Converters` z dwoma plikami:

**Plik:** `P03WeatherForecastWPF.Client/Converters/TemperatureToDisplayConverter.cs`

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

**Plik:** `P03WeatherForecastWPF.Client/Converters/NullToVisibilityConverter.cs`

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

2. Zastępujemy `App.xaml`: konwertery jako zasoby aplikacji i dwa proste style. `StartupUri` zostaje do sekcji 7.

**Plik:** `P03WeatherForecastWPF.Client/App.xaml` (wersja tymczasowa)

```xml
<!-- P03WeatherForecastWPF.Client/App.xaml  (wersja tymczasowa – pełna w sekcji 7) -->
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

💡 **Wyjaśnienie.** `xmlns:converters="clr-namespace:…Converters"` mapuje przestrzeń nazw C# na prefiks XAML; `<converters:NullToVisibilityConverter x:Key="…"/>` tworzy obiekt tej klasy. W dniu 11 zasoby były w `Window.Resources`; teraz przenosimy je wyżej, bo w dniu 13 dojdą kolejne okna. Style `Button` i `Label` bez klucza (style niejawne, implicit styles) zastępują `Margin="0,4"` wpisywane przy każdym przycisku.

3. Końcowa wersja okna: podmieniamy `StringFormat` temperatury na konwerter, chowamy pusty komunikat, usuwamy zasób z okna (jest w aplikacji) i `Margin` przycisku (jest w stylu):

🟩 **Co dopisujemy** w `P03WeatherForecastWPF.Client/MainWindow.xaml`:

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

**Plik:** `P03WeatherForecastWPF.Client/MainWindow.xaml`

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

4. Uruchamiamy F5.

### ✅ Punkt kontrolny

- `MainWindow.xaml` jest w wersji końcowej.
- Temperatura wyświetla się z jednostką, np. `12,5 °C` (przecinek z kultury polskiej).
- Czerwony komunikat pojawia się tylko dla `xyzxyz` i **znika** po udanym wyszukaniu (bo `ErrorMessage = null` → `Collapsed`).
- Przyciski i etykiety mają odstępy ze stylów w `App.xaml`.

### Dlaczego tak

- Formatowanie należy do widoku: ViewModel trzyma czystą liczbę, konwerter zamienia ją na tekst.
- Zasoby w `App.xaml`, nie w `Window.Resources`: w dniu 13 dojdą kolejne okna i będą używać tych samych konwerterów.
- `NotSupportedException` w `ConvertBack` od razu pokazuje błędne użycie konwertera w bindingu dwukierunkowym.

🔧 **Zmiana względem poprzednich edycji.** W edycji 26 konwerter nazywał się `TemperatureToDisplayConveter` (literówka), nie używał `culture`, a `ConvertBack` próbował parsować tekst, choć binding był jednokierunkowy. Zasoby siedziały w `Window.Resources`. Teraz: poprawna nazwa, kultura, `NotSupportedException`, zasoby w `App.xaml`, dodatkowy `NullToVisibilityConverter` zamiast zawsze widocznego `TextBlock`.

### Częste problemy

| Objaw | Rozwiązanie |
|---|---|
| `XamlParseException: Cannot find resource named 'TemperatureToDisplay'` | konwerter nie jest w `App.xaml` albo klucz się różni (wielkość liter!) |
| `The tag 'TemperatureToDisplayConverter' does not exist in XML namespace` | brak `xmlns:converters` w `App.xaml` albo inna przestrzeń nazw w pliku `.cs` |
| temperatura `12.5 °C` z kropką | ustawienia regionalne Windows są angielskie; to poprawne zachowanie `culture` |

Zostało jedno brzydkie miejsce: konstruktor okna z czterema `new`. Zastąpi go kontener DI.

---

## 7. P03: Dependency Injection przez Generic Host

### Cel

`App.xaml.cs` buduje hosta, rejestruje serwisy i tworzy okno. `appsettings.json` wchodzi przez `IOptions<AppSettings>`. Podmiana implementacji to jedna linia, a w kodzie nie ma `new OpenMeteoService` ani `new MainViewModel`.

### Teoria

- `StartupUri` tworzyło okno konstruktorem bezparametrowym: `new MainWindow()`. Okno będzie teraz wymagało `MainViewModel` w konstruktorze, więc tworzyć je musi ktoś, kto umie dostarczyć ViewModel: kontener. Zamiast `StartupUri` podpinamy zdarzenia `Startup` i `Exit`.
- `GetRequiredService<T>()` rzuca wyjątek, gdy typ nie jest zarejestrowany. `GetService<T>()` zwróciłby `null` i błąd wyszedłby później, w mniej oczywistym miejscu. W kodzie aplikacji zawsze używamy `GetRequiredService`.
- Jedyne miejsce, gdzie **my** pytamy kontener, to linia w `OnStartup`. Reszta obiektów dostaje zależności konstruktorem. Wstrzykiwanie `IServiceProvider` do ViewModelu i wołanie `GetService` w środku to antywzorzec (service locator): ukrywa zależności.
- **Alternatywy.** W edycjach 26/31 używaliśmy `new ServiceCollection()` + `BuildServiceProvider()`. To ten sam kontener, ale bez konfiguracji i logowania; `appsettings.json` trzeba było czytać osobno. Generic Host daje wszystko w jednym i jest identyczny z `WebApplication.CreateBuilder` z ASP.NET Core. Inne kontenery (Autofac, DryIoc) mają więcej możliwości, ale `Microsoft.Extensions.DependencyInjection` wystarcza w 95% projektów i jest standardem w .NET.

### Kroki

#### 7.1 `App.xaml` bez `StartupUri`

1. W `App.xaml` zamieniamy `StartupUri` na zdarzenia `Startup` i `Exit`:

🟩 **Co dopisujemy** w `P03WeatherForecastWPF.Client/App.xaml`:

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

**Plik:** `P03WeatherForecastWPF.Client/App.xaml`

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

#### 7.2 `App.xaml.cs` z hostem

2. Zastępujemy `App.xaml.cs` (najważniejszy plik dnia):

**Plik:** `P03WeatherForecastWPF.Client/App.xaml.cs`

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

💡 **Wyjaśnienie** linia po linii:

| Linia | Co robi |
|---|---|
| `Host.CreateApplicationBuilder()` | tworzy budowniczego z domyślną konfiguracją: czyta `appsettings.json` z folderu aplikacji (dlatego `PreserveNewest` w `.csproj`), `appsettings.{Environment}.json`, zmienne środowiskowe, User Secrets (gdy `UserSecretsId` jest w projekcie), włącza logowanie do konsoli/debuggera. `builder.Services` to kontener, `builder.Configuration` to konfiguracja |
| `builder.Services.Configure<AppSettings>(builder.Configuration.GetSection("AppSettings"))` | binduje sekcję JSON do klasy i rejestruje `IOptions<AppSettings>` |
| `AddHttpClient<IMeteoService, OpenMeteoService>(client => …)` | **typed client**: rejestruje `IMeteoService` → `OpenMeteoService` i dla każdego egzemplarza podaje `HttpClient` z `IHttpClientFactory`. Fabryka współdzieli połączenia (handler) między klientami i odświeża je co 2 minuty, więc nie ma ani wyczerpania gniazd, ani przeterminowanego DNS. Lambda konfiguruje klienta (timeout, od jutra też `BaseAddress`) |
| `AddSingleton<MainViewModel>()` | jeden ViewModel na aplikację; rejestracja po typie konkretnym, bo nikt nie potrzebuje interfejsu do niego |
| `AddTransient<MainWindow>()` | okno tworzone na żądanie; kontener widzi konstruktor `MainWindow(MainViewModel)` i sam poda ViewModel |
| `_host = builder.Build()` | buduje kontener; od teraz rejestracje są zamrożone |
| `OnStartup` → `_host.Start()` | uruchamia hosta (usługi w tle, logowanie); `GetRequiredService<MainWindow>()` tworzy okno **i cały łańcuch zależności**: `MainWindow` → `MainViewModel` → `IMeteoService` → `HttpClient` + `IOptions<AppSettings>` |
| `OnExit` | zatrzymuje hosta i zwalnia zasoby (`Dispose` wszystkich singletonów implementujących `IDisposable`) |

#### 7.3 Okno z konstruktorem

3. Końcowa wersja `MainWindow.xaml.cs`: ViewModel wchodzi konstruktorem.

🟩 **Co dopisujemy** w `P03WeatherForecastWPF.Client/MainWindow.xaml.cs`:

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

**Plik:** `P03WeatherForecastWPF.Client/MainWindow.xaml.cs`

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

4. Uruchamiamy F5 i testujemy jak w sekcji 4. Okno działa tak samo jak w sekcji 6, ale nigdzie w kodzie nie ma `new OpenMeteoService`, `new HttpClient` ani `new MainViewModel` (potwierdzamy wyszukiwaniem w całej solucji: **Ctrl+Shift+F**).
5. 🧪 **Eksperyment: podmiana implementacji** (ok. 2 min). W `ConfigureServices` komentujemy `AddHttpClient<…>` i odkomentowujemy `services.AddSingleton<IMeteoService, FakeMeteoService>();`. F5: dane testowe. To jedna linia; ViewModel, okno i XAML są nietknięte. Tak samo w dniu 13 podmienimy Open‑Meteo na nasze API, a w testach na atrapę. Przywracamy.
6. 🧪 **Eksperyment: konfiguracja bez kompilacji** (ok. 2 min). W `bin\Debug\net10.0-windows\appsettings.json` zmieniamy `"pl"` na `"en"` i uruchamiamy `.exe` z Eksploratora plików. Wynik: `Warsaw, Masovian Voivodeship (Poland)`. Przywracamy `pl`.
7. 🧪 **Eksperyment: brak rejestracji** (ok. 2 min). Komentujemy `services.AddSingleton<MainViewModel>();` i uruchamiamy F5. Pojawia się wyjątek `InvalidOperationException: Unable to resolve service for type 'MainViewModel' while attempting to activate 'MainWindow'`. Komunikat mówi dokładnie, czego brakuje i kto tego chciał. To najczęstszy błąd DI; wróci jutro w API. Przywracamy.

### ✅ Punkt kontrolny

- P03 jest w wersji końcowej. `dotnet build` przechodzi bez ostrzeżeń.
- Trzy testy: `Warszawa` (2 wyniki, pogoda po kliknięciu), `xyzxyz` (komunikat), puste pole (przycisk wyłączony).
- W kodzie nie ma `new OpenMeteoService`, `new HttpClient` ani `new MainViewModel`.
- Eksperyment 5: dane testowe bez zmiany ViewModelu i XAML. Eksperyment 6: angielskie nazwy bez kompilacji. Eksperyment 7: czytelny wyjątek o brakującej rejestracji.

### Dlaczego tak

- `Startup="OnStartup"` i `GetRequiredService<MainWindow>()` zamiast `StartupUri`, bo okno z parametrem `MainViewModel` musi tworzyć kontener.
- `MainViewModel` jako singleton: jeden stan okna na aplikację (stan przeżywa ponowne otwarcie okna). `MainWindow` jako transient: nowe okno przy każdym żądaniu.
- `OnExit` zatrzymuje hosta i wywołuje `Dispose` singletonów. Bez tego zasoby (np. logger) nie są zwalniane.
- `GetRequiredService` tylko w `OnStartup`: cała reszta grafu obiektów powstaje przez konstruktory. Dzięki temu zależności każdej klasy widać w jej sygnaturze.
- Ten sam `builder.Services`, `AddScoped` i `appsettings.json` pojawią się jutro w `Program.cs` projektu Web API.

🔧 **Zmiana względem poprzednich edycji.** Edycje 26/31: `ServiceCollection` bez hosta, `OpenMeteoService` zarejestrowany jako singleton tworzący `HttpClient` w `using` przy każdym zapytaniu, konfiguracja czytana w konstruktorze serwisu, pusty interfejs `IMainViewModel`, drugie okno otwierane przez `IServiceProvider` wstrzyknięty do ViewModelu. Edycja 32: Generic Host, `AddHttpClient` (typed client), `IOptions<AppSettings>`, brak pustych interfejsów, brak service locatora, `OnExit` zwalnia hosta.

### Częste problemy

| Objaw | Rozwiązanie |
|---|---|
| aplikacja uruchamia się i od razu kończy, bez okna | brak `Startup="OnStartup"` w `App.xaml` albo brak `mainWindow.Show()` |
| dwa okna albo `MissingMethodException: No parameterless constructor` | `StartupUri` zostało w `App.xaml` razem z hostem; usuwamy je |
| `Unable to resolve service for type 'Microsoft.Extensions.Options.IOptions…'` | brak `builder.Services.Configure<AppSettings>(…)` |
| `Unable to resolve service for type 'System.Net.Http.HttpClient'` | serwis zarejestrowany przez `AddSingleton` zamiast `AddHttpClient` (albo brak pakietu `Microsoft.Extensions.Http`) |
| nazwy miast po angielsku mimo `"pl"` w projekcie | zmieniony został `appsettings.json` w `bin`, a nie w projekcie (albo odwrotnie); `PreserveNewest` kopiuje nowszy plik |

Teraz zadanie samodzielne. Ten sam schemat: model z JSON, serwis za interfejsem, ViewModel z toolkitu, okno z bindingiem, host w `App.xaml.cs`. Dane: kursy walut NBP.

---

## 8. Zadanie: Z01AplikacjaNBP.Client

### Cel

Samodzielnie zbudować aplikację MVVM + DI według schematu z P03, na nowym API i z nową kontrolką (`DataGrid`). Czas: 45 minut pracy własnej, potem 30 minut omówienia rozwiązania.

### Treść zadania

Aplikacja WPF **Z01AplikacjaNBP.Client**, która:

1. Po kliknięciu **Pobierz kursy** pobiera tabelę A kursów średnich z API NBP: `https://api.nbp.pl/api/exchangerates/tables/A?format=json` i pokazuje ją w `DataGrid` z kolumnami **Kod**, **Waluta**, **Kurs średni (PLN)** (4 miejsca po przecinku), posortowaną po kodzie.
2. Nad tabelą pokazuje numer tabeli, datę i liczbę walut, np. `Tabela 196/A/NBP/2026 z dnia 09.10.2026 (32 walut)`.
3. Pod tabelą ma przelicznik: pole **Kwota w PLN** (domyślnie 100) i wynik `430,00 PLN = 98,23 EUR` dla waluty zaznaczonej w tabeli. Wynik przelicza się po każdej zmianie kwoty lub zaznaczenia.
4. Podczas pobierania pokazuje pasek postępu, a przycisk jest wyłączony. Błąd sieci to komunikat, nie wyjątek.
5. Architektura jak w P03: `Models/`, `Services/IExchangeRateService` + `NbpExchangeRateService`, `ViewModels/MainViewModel` (CommunityToolkit.Mvvm), `App.xaml.cs` z Generic Host i `AddHttpClient`, `MainWindow(MainViewModel)`.

### Teoria (wskazówki)

- Adres API warto najpierw otworzyć w przeglądarce. Odpowiedź to **tablica** tabel: `[ { "table": "A", "no": "…", "effectiveDate": "2026-10-09", "rates": [ { "currency": "euro", "code": "EUR", "mid": 4.3775 }, … ] } ]`. Deserializujemy ją do `ExchangeRateTable[]` i bierzemy `FirstOrDefault()`.
- NBP używa camelCase, więc mapujemy nazwy przez `[JsonPropertyName("effectiveDate")]`. Datę trzymamy w `DateOnly`, kurs w `decimal` (pieniądze nigdy w `double`).
- W `AddHttpClient` ustawiamy `client.BaseAddress = new Uri("https://api.nbp.pl/api/")`, a w serwisie używamy adresu względnego `exchangerates/tables/A?format=json`.
- `DataGrid`: `AutoGenerateColumns="False"`, `IsReadOnly="True"`, `CanUserAddRows="False"`, kolumny `DataGridTextColumn` z `Binding` i `StringFormat=N4`. `SelectedItem` działa jak w `ListBox`.
- Właściwość wyliczana (`ConvertedAmount`) nie ma settera, więc nie powiadomi sama. Potrzebny jest `[NotifyPropertyChangedFor(nameof(ConvertedAmount))]` na `AmountPln` i `SelectedRate`.
- `TextBox` zbindowany do `decimal`: po wpisaniu litery WPF sam pokaże czerwoną ramkę i nie zapisze wartości. To wbudowana walidacja konwersji.
- Pakiety: te same trzy co w P03.

### Kroki

1. Tworzymy projekt i dodajemy go do solucji:

```powershell
dotnet new wpf -n Z01AplikacjaNBP.Client -o Z01AplikacjaNBP.Client -f net10.0
dotnet sln 32Dzien12.slnx add Z01AplikacjaNBP.Client/Z01AplikacjaNBP.Client.csproj
```

2. Dodajemy pakiety `CommunityToolkit.Mvvm`, `Microsoft.Extensions.Hosting` i `Microsoft.Extensions.Http` (jak w sekcji 2.1).
3. Budujemy model, serwis, ViewModel, `App.xaml`/`App.xaml.cs` i okno według schematu z P03. Dopiero po 45 minutach porównujemy wynik z rozwiązaniem poniżej.
4. Ustawiamy Z01 jako projekt startowy (**Set as Startup Project**) i uruchamiamy F5.

### Kod (rozwiązanie)

Przy porównaniu nie chodzi o identyczny kod, tylko o ten sam podział odpowiedzialności: model ↔ serwis ↔ ViewModel ↔ widok ↔ rejestracja.

**Plik:** `Z01AplikacjaNBP.Client/Z01AplikacjaNBP.Client.csproj`

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

**Plik:** `Z01AplikacjaNBP.Client/Models/ExchangeRateTable.cs`

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

💡 **Wyjaśnienie.** `decimal` dla kursu, bo `0.1 + 0.2` w `double` to `0.30000000000000004`; przy pieniądzach to błąd księgowy. `DateOnly` (od .NET 6) zamiast `DateTime` z godziną 00:00; `System.Text.Json` deserializuje `"2026-10-09"` do `DateOnly` bez dodatkowego kodu. `List<Rate> = []` zamiast `null`, żeby pętla nigdy nie dostała `null`.

**Plik:** `Z01AplikacjaNBP.Client/Services/IExchangeRateService.cs`

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

**Plik:** `Z01AplikacjaNBP.Client/Services/NbpExchangeRateService.cs`

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

💡 **Wyjaśnienie.** Serwis nie zna pełnego adresu: `BaseAddress` jest w rejestracji `AddHttpClient`. Gdy NBP zmieni domenę albo zamiast NBP pojawi się własne API, zmieniamy jedną linię w `App.xaml.cs` (a w dniu 13 weźmiemy ją z `appsettings.json`). `FirstOrDefault()` zwraca `null` dla pustej tablicy; ViewModel to obsłuży.

**Plik:** `Z01AplikacjaNBP.Client/ViewModels/MainViewModel.cs`

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

💡 **Wyjaśnienie.**

- `ConvertedAmount` to właściwość **wyliczana** (computed property): nie ma pola ani settera, więc nikt nie wywoła dla niej `OnPropertyChanged`. `[NotifyPropertyChangedFor(nameof(ConvertedAmount))]` na `AmountPln` i `SelectedRate` mówi generatorowi: po zmianie tej właściwości powiadom też o tamtej. W edycji 26 robiło się to ręcznie: `OnPropertyChanged(nameof(CurrentTemperature))` w setterze.
- `AmountPln / SelectedRate.Mid`: kurs to cena jednej jednostki waluty w PLN, więc dzielimy. `430 / 4,3775 = 98,23`.
- `OrderBy(r => r.Code)`: sortowanie należy do ViewModelu (logika prezentacji), nie do serwisu (dostęp do danych) ani do `DataGrid` (użytkownik może sortować po kliknięciu w nagłówek, ale stan początkowy ustala ViewModel).
- `return` w `try` z `finally`: `IsBusy = false` wykona się mimo wczesnego wyjścia.

**Plik:** `Z01AplikacjaNBP.Client/App.xaml`

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

**Plik:** `Z01AplikacjaNBP.Client/App.xaml.cs`

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

**Plik:** `Z01AplikacjaNBP.Client/MainWindow.xaml`

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

💡 **Wyjaśnienie: `DataGrid`.** `AutoGenerateColumns="False"`, bo automatyczne kolumny miałyby nagłówki `Currency`, `Code`, `Mid` i domyślny format. `IsReadOnly` i `CanUserAddRows="False"` usuwają pusty wiersz do edycji na końcu. `SelectedItem="{Binding SelectedRate}"` działa tak samo jak w `ListBox`. `StringFormat=N4` w kolumnie nie potrzebuje `{}`, bo nie zaczyna się od nawiasu klamrowego.

**Plik:** `Z01AplikacjaNBP.Client/MainWindow.xaml.cs`

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

### ✅ Punkt kontrolny

- **Pobierz kursy** → tabela z 30+ walutami posortowana po kodzie (AUD, BGN, BRL, …), nad nią `Tabela …/A/NBP/… z dnia … (… walut)`.
- 🧪 Zaznaczamy `EUR` i wpisujemy `430` → `430,00 PLN = 98,23 EUR` (przy kursie 4,3775; przy innym kursie sprawdzamy wynik, dzieląc 430 przez kurs z tabeli).
- 🧪 Wpisujemy `abc` → czerwona ramka, wynik się nie zmienia.
- 🧪 Wyłączamy Wi‑Fi i klikamy **Pobierz kursy** → „Błąd połączenia: …”, aplikacja działa dalej.

### Dlaczego tak

- Ten sam schemat co w P03: model ↔ serwis za interfejsem ↔ ViewModel z toolkitu ↔ widok z bindingiem ↔ rejestracja w hoście. Nowe są tylko API i kontrolka.
- `BaseAddress` w rejestracji, adres względny w serwisie: zmiana domeny to jedna linia.
- `[NotifyPropertyChangedFor]` na dwóch właściwościach wejściowych odświeża właściwość wyliczaną.
- `decimal` i `DateOnly` opisują dane dokładnie: kwota bez błędów zaokrągleń, data bez godziny.

🔧 **Zmiana względem poprzednich edycji.** W edycji 31 (`P01AplikacjaNBP_WPF.Client`) serwis tworzył `HttpClient` w `using` przy każdym zapytaniu i używał Newtonsoft.Json, `Mid` był `double`, okno robiło `DataContext = new MainViewModel(new NbpWebApiService())`, nie było `IsBusy`, obsługi błędów ani przelicznika. Teraz: `AddHttpClient` z `BaseAddress`, `System.Text.Json` z `decimal` i `DateOnly`, Generic Host, przelicznik z właściwością wyliczaną, komunikat o błędzie.

### Częste problemy

| Objaw | Rozwiązanie |
|---|---|
| okno się nie pokazuje | brak `Startup="OnStartup"` w `App.xaml` |
| dwa okna albo wyjątek o konstruktorze | `StartupUri` zostało w `App.xaml` razem z hostem; usuwamy je |
| `DataGrid` pusty, bez błędu | `AutoGenerateColumns="False"` bez zdefiniowanych kolumn albo `ItemsSource` zbindowany do pola zamiast właściwości |
| `JsonException: The JSON value could not be converted to ExchangeRateTable` | deserializacja do pojedynczego obiektu, a API zwraca tablicę: potrzebne `ExchangeRateTable[]` |
| wynik przelicznika nie odświeża się | brak `[NotifyPropertyChangedFor(nameof(ConvertedAmount))]` |
| `Mid` wyświetla `4,3775000` albo `4.3775` | `StringFormat=N4` i typ `decimal`; `mid` jako `double` z `StringFormat` na `Label` bez `{}` też nie działa |

---

## 9. Podsumowanie dnia

1. **Delegaty i zdarzenia.** Delegat to metoda w zmiennej. `event` to delegat z ograniczeniem dostępu: z zewnątrz można go tylko subskrybować.
2. **MVVM.** ViewModel trzyma stan i komendy, widok tylko binduje, code‑behind ma jedną linię.
3. **`INotifyPropertyChanged` i `ICommand`** napisane ręcznie, a potem wygenerowane przez CommunityToolkit.Mvvm (`[ObservableProperty]`, `[RelayCommand]`).
4. **Binding:** `UpdateSourceTrigger`, `ItemsSource`/`SelectedItem`, `StringFormat`, `FallbackValue`, konwertery, zasoby w `App.xaml`.
5. **Dependency Injection:** interfejs + konstruktor + kontener, cykle życia Singleton/Transient, Generic Host z konfiguracją `IOptions`. Podmiana implementacji serwisu to jedna linia.

Jutro (dzień 13) powstaje własne Web API w tym samym Generic Hoście: `builder.Services`, `AddScoped`, `appsettings.json` będą wyglądały znajomo. P03 podłączymy do tego API. `MainViewModel`, `IMeteoService` i rejestracje w `App.xaml.cs` zostają; dojdzie `IProductService` z biblioteki wspólnej `P06Shop.Shared`.

### Zapisanie pracy

Solucja końcowa. Visual Studio dodało `README.md` i instrukcję do folderu **Solution Items**; ten folder jest opcjonalny.

**Plik:** `32Dzien12.slnx`

```xml
<!-- 32Dzien12.slnx -->
<Solution>
  <Folder Name="/Solution Items/">
    <File Path="README.md" />
    <File Path="Instrukcja.md" />
  </Folder>
  <Project Path="P03WeatherForecastWPF.Client/P03WeatherForecastWPF.Client.csproj" />
  <Project Path="P04Delegaty/P04Delegaty.csproj" />
  <Project Path="Z01AplikacjaNBP.Client/Z01AplikacjaNBP.Client.csproj" />
</Solution>
```

Commit i wysłanie do repozytorium (trzy ostatnie polecenia tylko wtedy, gdy mamy własne repozytorium na GitHubie; adres podajemy własny):

```powershell
git init
git add .
git commit -m "Dzien 12: delegaty, MVVM (klasyczny + CommunityToolkit), binding, DI przez Generic Host, zadanie NBP"
git branch -M main
git remote add origin https://github.com/tomasz-trainer/32Dzien12.git
git push -u origin main
```

## Pytania kontrolne

1. Czym różni się `event` od zwykłego pola typu delegata? Co może zrobić kod spoza klasy z jednym, a czego nie może z drugim?
2. Co się stanie, gdy setter właściwości w ViewModelu nie wywoła `OnPropertyChanged`? Czy aplikacja się wywróci?
3. Po co `UpdateSourceTrigger=PropertyChanged` na `TextBox` z nazwą miasta? Co by się działo bez niego z przyciskiem **Szukaj**?
4. Dlaczego `MainViewModel` przyjmuje `IMeteoService`, a nie `OpenMeteoService`? Podaj dwie korzyści.
5. Czym różni się `AddSingleton` od `AddTransient`? Co jest singletonem w P03, a co transientem i dlaczego?
6. Co generuje `[ObservableProperty]` na właściwości `partial`? Dlaczego klasa musi być `partial`?
7. Dlaczego `StringFormat={}{0:F1} °C` ma `{}` na początku, a `StringFormat=N4` nie?
8. Gdzie w kodzie dnia 12 jest jedyne wywołanie `GetRequiredService` i dlaczego nie wołamy go w ViewModelu?

## Zadania dodatkowe

1. **P03: `ClearCommand`.** Komenda czyści listę, zaznaczenie i pogodę; `CanExecute` tylko, gdy lista nie jest pusta. Do tego przycisk **Wyczyść** i `KeyBinding` na `Escape`. Podpowiedź: `[NotifyCanExecuteChangedFor]` nie zadziała dla `ObservableCollection`; po `Cities.Clear()`/`Add` trzeba wywołać `ClearCommand.NotifyCanExecuteChanged()`.
2. **P03: test ViewModelu bez WPF.** Program konsolowy tworzy ten sam host co `App.xaml.cs` (bez okna), pobiera `MainViewModel` z kontenera, ustawia `CityName = "Warszawa"`, wywołuje `await vm.LoadCitiesCommand.ExecuteAsync(null)` i wypisuje `Cities.Count`.
3. **Z01: wybór tabeli.** `ComboBox` z wyborem tabeli `A`/`B` (`exchangerates/tables/B`) zbindowany do właściwości `TableName`; serwis dostaje nazwę tabeli jako parametr.
4. **Z01: przelicznik w drugą stronę** (waluta → PLN) z przełącznikiem kierunku.

---

## Dodatek A: zmiany względem edycji 26 i 31

| Gdzie | Edycje 26/31 | Edycja 32 | Dlaczego |
|---|---|---|---|
| Platforma, pakiety | 26: .NET 8, toolkit 8.2.2, `Microsoft.Extensions.DependencyInjection` 8.0; 31: .NET 10, toolkit 8.4.2, DI 10.0.12, Newtonsoft 13 | .NET 10, `CommunityToolkit.Mvvm 8.4.2`, `Microsoft.Extensions.Hosting 10.0.12`, `Microsoft.Extensions.Http 10.0.12`; usunięty `Configuration.Json` (zawarty w Hosting), bez Newtonsoft | mniej pakietów, jeden host |
| P04Delegaty | 31: cztery przypisania `Action`/`Func`/`Predicate` w `Main` | własny typ delegata, metoda wyższego rzędu, lambdy, LINQ, multicast, `event` z `EventArgs` i `-=`, mapa „gdzie to w MVVM” | zrozumienie `RelayCommand` i `PropertyChanged` |
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
| Instrukcja | `Podsumowanie.txt` z 7 punktami; kod końcowy od razu, bez etapów pośrednich | jedna instrukcja z teorią, krokami, kodem i punktami kontrolnymi; etapy pośrednie, które się kompilują (`FakeMeteoService`, `Options.Create` w wersjach tymczasowych) | projekt nigdy nie jest „czerwony” dłużej niż jedną sekcję |

## Dodatek B: jak zweryfikowano instrukcję

- Cały kod końcowy (`P03WeatherForecastWPF.Client`, `P04Delegaty`, `Z01AplikacjaNBP.Client`) zbudowano `dotnet build 32Dzien12.slnx` z .NET SDK 10.0.401 bez błędów i ostrzeżeń. Dodatkowo zbudowano każdy etap pośredni (stan po sekcjach 3, 4, 5, 6, 7) z plikami „wersja tymczasowa” dokładnie w tej treści, jaka jest w instrukcji.
- `P04Delegaty` uruchomiono z konsoli; wyjście w sekcji 1 jest skopiowane z tego uruchomienia.
- Logikę `MainViewModel` (P03) i `MainViewModel` (Z01) przetestowano w programie konsolowym z tym samym kontenerem DI (`Host.CreateApplicationBuilder`, te same rejestracje co w `App.xaml.cs`): `LoadCitiesCommand` dla `Warszawa` zwraca 2 miasta, ustawienie `SelectedCity` pobiera pogodę (`Weather` z temperaturą i opisem), `xyzxyz` daje `ErrorMessage = "Nie znaleziono miasta o takiej nazwie."`, pusta nazwa wyłącza komendę (`CanExecute` zwraca `false`). NBP: tabela A z 32 walutami, `AmountPln = 430` i `SelectedRate = EUR` dają `430,00 PLN = 98,23 EUR` (kurs 4,3775 z tabeli z 2026‑10‑09).
- Każdy blok kodu z pełnym plikiem porównano skryptem z plikiem w repozytorium (treść identyczna znak w znak); bloki „wersja tymczasowa” pochodzą z etapów pośrednich opisanych wyżej.
- Okien WPF nie uruchamiano (środowisko bez Windows). Przed zajęciami warto przejść sekcje 3–8 w Visual Studio, w szczególności sprawdzić `StringFormat` na `Label.Content`, widoczność `ProgressBar` przy `FakeMeteoService` i komunikat `Unable to resolve service` z eksperymentu w sekcji 7. Gdy coś w XAML zachowuje się inaczej niż w punkcie kontrolnym, pierwsze miejsca do sprawdzenia to okno Output i tabele **Częste problemy**.
