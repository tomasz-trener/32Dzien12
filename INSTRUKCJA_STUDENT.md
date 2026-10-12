# Dzień 12: instrukcja dla uczestnika

> **Co dziś zbudujesz:**
> 1. **P04Delegaty** – program konsolowy: delegaty, lambdy, `Action`/`Func`/`Predicate`, zdarzenia. Fundament pod wszystko, co dalej.
> 2. **P03WeatherForecastWPF.Client** – tę samą aplikację pogodową co wczoraj, ale w architekturze **MVVM** (logika w ViewModelu, okno podpięte bindingiem) i z **Dependency Injection** (obiekty tworzy kontener, nie Ty).
> 3. **Z01AplikacjaNBP.Client** – zadanie do samodzielnego wykonania: kursy walut NBP w `DataGrid` i przelicznik. Rozwiązanie jest na końcu sekcji 8, ale najpierw spróbuj sam.
>
> Pracuj po kolei. Każda sekcja ma punkt **Sprawdź** – nie idź dalej, dopóki nie działa. Numeracja sekcji (0–9) jest taka sama jak w scenariuszu trenera, więc „etap 7” w komentarzach kodu to sekcja 7.

## Zanim zaczniesz

**Potrzebujesz:**

- Visual Studio 2026 z workloadem **.NET desktop development** (albo VS Code + .NET 10 SDK).
- .NET 10 SDK: `dotnet --version` ma pokazać `10.0.x`.
- Dostęp do internetu: `open-meteo.com`, `api.nbp.pl`, `nuget.org`.
- **Kod z dnia 11**: folder `P03WeatherForecastWPF.Client` (bez `bin` i `obj`). Jeśli go nie masz, pobierz z repozytorium `tomasz-trainer/32Dzien11`.

**Jak uruchomić stan z poprzedniego dnia:**

1. Utwórz pusty folder `32Dzien12`.
2. Skopiuj do niego folder `P03WeatherForecastWPF.Client` z dnia 11.
3. W terminalu (PowerShell) w folderze `32Dzien12`:

```powershell
dotnet new sln -n 32Dzien12 --format slnx
dotnet sln 32Dzien12.slnx add P03WeatherForecastWPF.Client/P03WeatherForecastWPF.Client.csproj
dotnet new gitignore
dotnet build
```

4. Otwórz `32Dzien12.slnx` w Visual Studio, ustaw P03 jako projekt startowy i uruchom F5. Wpisz `Warszawa`, kliknij **Szukaj**, wybierz miasto. Ma działać jak wczoraj.

**Jak korzystać z tej instrukcji:**

- Bloki kodu wklejasz **w całości** do pliku, którego ścieżka jest w pierwszej linii komentarza.
- Bloki 🟩 **Co dopisujesz** (bloki `diff`) pokazują zmiany w pliku, który już masz: linie `+` dopisujesz, `-` usuwasz, `⋯` to pominięty kod. Znaków `+`/`-` nie kopiuj; pod spodem jest zawsze pełny plik.
- Pliki z dopiskiem **wersja tymczasowa** nadpiszesz w późniejszej sekcji. Są po to, żeby projekt kompilował się po każdej sekcji.
- Fragmenty **Dlaczego tak** wyjaśniają decyzje; na nich opierają się dni 13–18.

**Gotowy kod** jest w repozytorium `tomasz-trainer/32Dzien12`, ale najwięcej nauczysz się, pisząc go sam.

**Dane testowe:** miasta `Warszawa`, `Kraków`, `xyzxyz`; w zadaniu kwota `430` PLN i waluta `EUR`.

---

## 0. Teoria: MVVM i Dependency Injection

### Co jest nie tak z kodem z dnia 11?

Otwórz `MainWindow.xaml.cs` z dnia 11. 91 linii, a w nich: pobieranie danych, obsługa błędów, czytanie konfiguracji **i** ustawianie `lblTemperature.Content`. Trzy problemy:

| Problem | Na czym polega |
|---|---|
| Testowalność | żeby sprawdzić komunikat dla `xyzxyz`, musisz uruchomić okno i kliknąć; nie da się napisać testu automatycznego |
| Duplikacja | w dniu 18 ta sama aplikacja ma działać na Androidzie (MAUI): logikę z code‑behind trzeba by skopiować |
| Sprzężenie z kontrolkami | zamiana `Label` na `TextBlock` psuje kod (`Content` → `Text`); logika wie za dużo o wyglądzie |

### MVVM (Model–View–ViewModel)

| Element | Co to jest | U nas |
|---|---|---|
| **Model** | dane i reguły, nie wie o UI | `City`, `Weather`, `OpenMeteoService` |
| **View** (widok) | XAML; code‑behind prawie pusty | `MainWindow.xaml` |
| **ViewModel** | stan okna jako właściwości (`CityName`, `Cities`) i akcje jako komendy (`LoadCitiesCommand`); zwykła klasa C#, nie zna kontrolek | `MainViewModel` |
| **Binding** (wiązanie) | mechanizm WPF łączący właściwość kontrolki z właściwością ViewModelu | `Text="{Binding CityName}"` |
| **Komenda** (`ICommand`) | „wykonaj” + „czy można wykonać”, zamiast `Click` | `Command="{Binding LoadCitiesCommand}"` |
| **Powiadomienie** (`INotifyPropertyChanged`) | zdarzenie: „zmieniłem się, odśwież” | `OnPropertyChanged()` |

Kierunek zależności jest jeden: widok zna ViewModel (przez `DataContext`), ViewModel zna model. ViewModel **nie zna** widoku, dlatego da się go testować w konsoli i użyć w MAUI.

**Zalety:** testowalny ViewModel, jeden ViewModel na wiele platform, binding „przepisuje” dane za Ciebie, grafik może zmieniać XAML bez ruszania C#.
**Wady:** więcej plików, trochę kodu powtarzalnego (dziś zobaczysz, jak CommunityToolkit go generuje), błędy bindingu widać dopiero w czasie działania (w oknie Output).
**Alternatywy:** code‑behind (małe narzędzia), MVP (Windows Forms), MVU/ReactiveUI (stan niezmienny, inny sposób myślenia), komponenty Blazor (dzień 17).

### Dependency Injection (DI, wstrzykiwanie zależności)

Wczoraj okno robiło `new OpenMeteoService(new HttpClient(), language)`. Klasa, która sama tworzy swoje zależności:

- nie da się jej podmienić serwisu na testowy bez zmiany kodu,
- każdy nowy parametr konstruktora serwisu to poprawka w każdym `new`,
- musi pamiętać o szczegółach (jeden `HttpClient` na aplikację).

**Zasada odwrócenia zależności:** klasa dostaje zależności **konstruktorem**, jako **interfejs**. `MainViewModel(IMeteoService service)` nie wie, czy to Open‑Meteo, dane testowe czy jutro nasze API. Obiekty tworzy **kontener DI**: rejestrujesz „dla `IMeteoService` użyj `OpenMeteoService`”, a on buduje cały łańcuch: okno → ViewModel → serwis → `HttpClient` + konfiguracja.

**Cykle życia:**

| Rejestracja | Ile obiektów | Kiedy |
|---|---|---|
| `AddSingleton<T>()` | jeden na aplikację | serwisy bez stanu użytkownika, ViewModel okna głównego |
| `AddTransient<T>()` | nowy przy każdym pobraniu | lekkie obiekty, okna |
| `AddScoped<T>()` | jeden na „zakres” (w ASP.NET Core: jedno żądanie HTTP) | serwisy z `DbContext` w API (dni 13–14); w WPF dziś nie używamy |

Pułapka: singleton nie może trzymać obiektu o krótszym życiu (zatrzymałby go na zawsze).

**Generic Host** (`Host.CreateApplicationBuilder`) to obiekt, który łączy konfigurację (`appsettings.json`), logowanie i kontener DI. Ten sam zobaczysz jutro w `Program.cs` projektu Web API, a potem w Blazorze i MAUI. Uczysz się raz.

### Pliki punktu startowego i co się z nimi stanie

| Plik | Co się zmienia |
|---|---|
| `Models/*.cs` | nic |
| `Services/OpenMeteoService.cs` | implementuje `IMeteoService`, język z `IOptions<AppSettings>` (sekcja 4) |
| `MainWindow.xaml` | binding i `Command` zamiast `x:Name` i `Click` (sekcje 3, 5, 6) |
| `MainWindow.xaml.cs` | 91 linii → 18 linii (sekcje 2, 7) |
| `App.xaml`, `App.xaml.cs` | konwertery, style, Generic Host (sekcje 6, 7) |
| `appsettings.json` | nic; czyta go Generic Host |
| `.csproj` | trzy nowe pakiety (sekcja 2) |

---

## 1. P04Delegaty: delegaty, lambdy, zdarzenia

**Cel:** zrozumieć, że delegat to „metoda w zmiennej”, poznać `Action`/`Func`/`Predicate`, multicast i `event`, i zobaczyć, gdzie to wraca w MVVM.

### Teoria w pigułce

- **Delegat** to typ opisujący sygnaturę metody (parametry + wynik). Zmienna tego typu trzyma metodę: nazwaną, anonimową albo **lambdę** (`c => c + 273.15`).
- .NET ma gotowe delegaty generyczne: `Action<T>` (nic nie zwraca), `Func<T, TResult>` (zwraca), `Predicate<T>` (zwraca `bool`). LINQ (`Where`, `Select`) to delegaty w praktyce.
- **Multicast:** jeden delegat może wskazywać wiele metod (`+=`, `-=`); wywołanie uruchamia wszystkie po kolei.
- **`event`** to delegat multicast, który z zewnątrz można tylko subskrybować. Wywołać go może tylko klasa, w której jest zadeklarowany. Standardowy delegat zdarzeń to `EventHandler<T>` z `(object? sender, T e)`.
- Alternatywa dla delegatu: interfejs z jedną metodą (wzorzec Strategia). Delegat wygrywa, gdy to jedna funkcja; interfejs, gdy zachowanie ma stan lub kilka metod.

### Krok po kroku

1. W folderze `32Dzien12`:

```powershell
dotnet new console -n P04Delegaty -o P04Delegaty -f net10.0
dotnet sln 32Dzien12.slnx add P04Delegaty/P04Delegaty.csproj
```

Plik projektu jest standardowy:

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

2. Dodaj plik `Converters.cs` (własny typ delegata i metoda, która przyjmuje delegat jako parametr):

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

3. Dodaj plik `TemperatureSensor.cs` (zdarzenie z własną klasą argumentów):

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

4. Zastąp `Program.cs`:

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

5. Uruchom: `dotnet run --project P04Delegaty`.

### Sprawdź

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

W sekcji 4 są trzy linie „Zmiana” (nie cztery: przypisanie `35.0` drugi raz nie zmienia wartości) i jedna linia „ALARM” (po `-=` alarm milczy).

### Dlaczego tak

- `PrintConverted` nie wie, **jak** przeliczać; dostaje przepis z zewnątrz. Tak samo `RelayCommand` (sekcja 2) nie wie, co zrobić po kliknięciu; dostanie to z ViewModelu.
- `Action<string> print = Console.WriteLine` to **grupa metod**: kompilator sam wybiera przeciążenie z parametrem `string`.
- `TemperatureChanged?.Invoke(...)`: gdy nikt nie subskrybuje, delegat jest `null`; `?.` chroni przed wyjątkiem.
- Setter `Temperature` sprawdza, czy wartość się zmieniła, i dopiero wtedy powiadamia. Dokładnie taki setter napiszesz w ViewModelu, tylko zdarzenie będzie się nazywać `PropertyChanged`.
- `TemperatureChangedEventArgs(double oldValue, double newValue)` to **konstruktor podstawowy** (primary constructor, C# 12): parametry w nagłówku klasy.
- Multicast z wartością zwracaną zwraca wynik tylko ostatniej metody, a wyjątek w jednej metodzie przerywa resztę. Dlatego zdarzenia zwykle są `void`.

### Częste problemy

| Objaw | Rozwiązanie |
|---|---|
| `°` wyświetla się jako krzaczki | brakuje `Console.OutputEncoding = UTF8` (jest w pierwszej linii kodu) |
| błąd `cannot be accessed with an instance reference` przy `Alarm.OnTemperatureChanged` | `Alarm` jest klasą statyczną; subskrybuj bez `new` |
| `TemperatureChanged` nie da się wywołać spoza klasy | tak ma być: to `event`; wywołuje go tylko `TemperatureSensor` |

---

## 2. P03: MVVM „ręcznie”

**Cel:** napisać ViewModel z `INotifyPropertyChanged` i własnym `RelayCommand`, żeby zobaczyć, co dzieje się pod spodem.

### Teoria w pigułce

- `INotifyPropertyChanged` ma jedno zdarzenie: `PropertyChanged`. Binding WPF subskrybuje je i po każdym `OnPropertyChanged("Weather")` czyta `Weather` na nowo. Bez powiadomienia binding pokaże wartość początkową i nigdy jej nie odświeży.
- `[CallerMemberName]` każe kompilatorowi wstawić nazwę składowej, z której wołasz metodę. `OnPropertyChanged()` w setterze `CityName` wysyła `"CityName"`. Zero literówek.
- `ICommand` ma `Execute`, `CanExecute` i zdarzenie `CanExecuteChanged`. `RelayCommand` opakowuje dwa delegaty z sekcji 1: `Action<object?>` i `Predicate<object?>`.
- Interfejs serwisu (`IMeteoService`) pozwala podmienić implementację. Na tym etapie użyjesz `FakeMeteoService` (dane bez internetu), bo prawdziwy serwis w wersji końcowej będzie wymagał konfiguracji, której jeszcze nie ma.

### Krok po kroku

1. Pakiety NuGet. Usuń `Microsoft.Extensions.Configuration.Json` (zawiera go `Hosting`), dodaj trzy nowe:

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

2. Utwórz folder `Commands` i klasę `RelayCommand.cs`:

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

3. W folderze `Services` dodaj interfejs i implementację testową:

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

4. Utwórz folder `ViewModels`, w nim podfolder `Classic`, a w nim klasę `MainViewModelClassic.cs`:

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

5. Zastąp `MainWindow.xaml.cs` wersją tymczasową (ViewModel tworzysz ręcznie; kontener DI zrobi to w sekcji 7):

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

> ⚠️ Po tym kroku projekt **nie zbuduje się**: `MainWindow.xaml` nadal odwołuje się do `btnSearch_Click` i `lbCities_SelectionChanged`. To normalne. Naprawisz to w sekcji 3.

### Sprawdź

- Błąd kompilacji dotyczy **tylko** `MainWindow.xaml` (`does not contain a definition for 'btnSearch_Click'`). Jeśli widzisz inne błędy, sprawdź przestrzenie nazw (`P03WeatherForecastWPF.Client.Commands`, `.Services`, `.ViewModels.Classic`).
- `MainViewModelClassic` nie ma żadnego `using System.Windows.Controls`: ViewModel nie zna kontrolek.

### Dlaczego tak

- **Pole + właściwość + `OnPropertyChanged()`** dla każdej z sześciu właściwości: 50 linii, które wyglądają tak samo. Zapamiętaj ten ból; w sekcji 5 zniknie.
- **`SelectedCity`** w setterze uruchamia `LoadWeatherAsync()`. Wybór na liście = pobranie pogody (wczoraj: `SelectionChanged`). `_ = ...` to **discard**: „wiem, że nie czekam na ten `Task`”. `async void` byłoby gorsze: wyjątek z `async void` zabija aplikację.
- **`IsBusy` i `ErrorMessage`** zamiast `btnSearch.IsEnabled = false` i `tbStatus.Text = ...`. ViewModel ustawia **stan**; jak go pokazać, decyduje widok.
- **`CanExecuteChanged` przez `CommandManager.RequerySuggested`**: WPF podnosi to zdarzenie po każdej interakcji, więc przycisk sam się włącza i wyłącza. `CanExecute` ma być tanie, bo WPF pyta często.
- **`catch (Exception)`** szerzej niż wczoraj: serwis za interfejsem może rzucić cokolwiek (sieć, timeout, JSON). ViewModel zamienia każdy wyjątek na komunikat.
- **Wszystkie pakiety od razu**, żeby nie przerywać później. `CommunityToolkit.Mvvm` użyjesz w sekcji 5, `Hosting` i `Http` w sekcjach 4 i 7.

---

## 3. P03: binding w XAML

**Cel:** podpiąć każdą kontrolkę do ViewModelu bez `x:Name`, `Click` i `SelectionChanged`.

### Teoria w pigułce

| Składnia | Co robi |
|---|---|
| `{Binding CityName}` | łączy właściwość kontrolki z `CityName` obiektu z `DataContext`; `DataContext` dziedziczy się w dół drzewa |
| `Mode` | `OneWay` (VM → widok), `TwoWay` (oba), `OneTime`; `TextBox.Text` i `SelectedItem` są domyślnie `TwoWay`, `Label.Content` jest `OneWay` |
| `UpdateSourceTrigger=PropertyChanged` | widok zapisuje do VM po każdej literze, nie dopiero po opuszczeniu pola |
| `ItemsSource` + `SelectedItem` | lista bierze elementy z kolekcji, zaznaczenie zapisuje do VM |
| `SelectedCity.FullName` | ścieżka przez kilka właściwości; gdy `SelectedCity` jest `null`, binding loguje błąd i pokazuje `FallbackValue` |
| `StringFormat={}{0:F1} °C` | formatowanie jak w `string.Format`; `{}` na początku to „escape”, żeby XAML nie uznał `{0:F1}` za rozszerzenie znaczników. `StringFormat=g` go nie potrzebuje |
| `FallbackValue=—` | tekst, gdy ścieżka nie działa (np. `Weather` jest `null`) |
| `Command="{Binding LoadCitiesCommand}"` | przycisk woła `Execute`, a `IsEnabled` bierze z `CanExecute` |
| `KeyBinding Key="Enter"` | ta sama komenda z klawiatury |
| `Converter={StaticResource BoolToVisibility}` | `bool` → `Visibility` (wbudowany `BooleanToVisibilityConverter`) |
| `d:DataContext="{d:DesignInstance Type=vm:…}"` | tylko dla edytora: IntelliSense i podkreślanie literówek; w czasie działania ignorowane |

### Krok po kroku

1. Zastąp `MainWindow.xaml` wersją tymczasową (bez własnych konwerterów, które dodasz w sekcji 6):

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

2. Uruchom F5. Wpisz `Warszawa`, naciśnij Enter albo kliknij **Szukaj**.
3. Skasuj tekst w polu i patrz na przycisk.
4. Zmień w XAML `{Binding CityName}` na `{Binding CityNam}`, uruchom, otwórz okno **Output** (View → Output, źródło Debug) i poszukaj linii `System.Windows.Data Error: 40 : BindingExpression path error`. Przywróć `CityName`.

### Sprawdź

- `Warszawa` → pasek postępu przez pół sekundy, lista z jednym wynikiem (dane z `FakeMeteoService`), po kliknięciu losowa temperatura, wiatr, opis, czas.
- `a` → trzy miasta (Warszawa, Kraków, Gdańsk). `xyzxyz` → czerwony komunikat „Nie znaleziono miasta o takiej nazwie”.
- Puste pole → przycisk **Szukaj** wyłączony.
- W oknie Output nie ma linii `System.Windows.Data Error` (poza tą z kroku 4).

### Dlaczego tak

- Każda kontrolka mówi tylko, **z czym** jest związana, nie **co robić**. Dzięki temu w sekcji 5 podmienisz ViewModel na inny i XAML nie zmieni się ani o jeden binding.
- Binding nie rzuca wyjątku, tylko loguje. Aplikacja działa, ale kontrolka jest „martwa”. Gdy coś „nie odświeża się”, okno Output to pierwsze miejsce do sprawdzenia. `d:DataContext` wyłapie literówkę już w edytorze.
- `BoolToVisibility` jest na razie w `Window.Resources`; w sekcji 6 przeniesiesz zasoby do `App.xaml`.

### Częste problemy

| Objaw | Rozwiązanie |
|---|---|
| przycisk **Szukaj** zawsze wyłączony | `CanExecute` zwraca `false`: sprawdź, czy `CityName` ma wartość początkową `"Warszawa"` i czy `IsBusy` wraca na `false` w `finally` |
| przycisk włącza się dopiero po kliknięciu gdzie indziej | brak `UpdateSourceTrigger=PropertyChanged` |
| lista pokazuje `P03WeatherForecastWPF.Client.Models.City` | brak `DisplayMemberPath="FullName"` |
| temperatura pokazuje `{0:F1} °C` dosłownie | brak `{}` przed `{0:F1}` |
| `XamlParseException: Cannot find resource named 'BoolToVisibility'` | brak `<Window.Resources>` z `BooleanToVisibilityConverter` |

---

## 4. P03: `OpenMeteoService` za interfejsem i wzorzec Options

**Cel:** prawdziwy serwis implementuje `IMeteoService`, a ustawienia wchodzą jako obiekt `AppSettings`.

### Teoria w pigułce

- **Wzorzec Options:** klasa, której właściwości odpowiadają kluczom w sekcji JSON (`AppSettings:DefaultLanguage` → `AppSettings.DefaultLanguage`). Zamiast tekstowego klucza `configuration["AppSettings:DefaultLanguage"]` dostajesz silnie typowany `IOptions<AppSettings>` z właściwością `Value`.
- Odmiany: `IOptionsSnapshot<T>` (odczyt na nowo w każdym zakresie), `IOptionsMonitor<T>` (powiadamia o zmianie pliku). Do ustawień czytanych raz wystarczy `IOptions<T>`.
- `Options.Create(obiekt)` opakowuje gotowy obiekt w `IOptions<T>` bez kontenera. Przydaje się w testach i na tym etapie.

### Krok po kroku

1. Utwórz folder `Configuration` i klasę `AppSettings.cs`:

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

2. Zmień `Services/OpenMeteoService.cs` (trzy zmiany: interfejs, typ drugiego parametru, odczyt języka):

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

3. W `MainWindow.xaml.cs` podepnij prawdziwy serwis (nadal ręcznie):

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

4. Uruchom F5 i przetestuj: `Warszawa`, `Kraków`, `xyzxyz`.
5. Zamień komentarze w konstruktorze (wersja z `FakeMeteoService`), uruchom, przywróć.

### Sprawdź

- `Warszawa` → 2 wyniki, pierwszy to Warszawa, Województwo mazowieckie (Polska); po kliknięciu prawdziwa temperatura.
- `Kraków` → na liście jest też Krakow w Wisconsin.
- `xyzxyz` → „Nie znaleziono miasta o takiej nazwie”.
- Z `FakeMeteoService` okno działa tak samo; zmieniłeś jedną linię.

### Dlaczego tak

- Logika pobierania w `OpenMeteoService` jest **identyczna** jak wczoraj. Zmienił się tylko sposób, w jaki serwis dostaje ustawienia.
- `SectionName` jako stała: nazwa sekcji jest w jednym miejscu. Wartość domyślna `"en"` działa, gdy klucza nie ma w pliku.
- Policz, ile `new` jest teraz w konstruktorze okna: `HttpClient`, `AppSettings`, `OpenMeteoService`, `MainViewModelClassic`. W sekcji 7 wszystkie znikną.

### Częste problemy

| Objaw | Rozwiązanie |
|---|---|
| `'OpenMeteoService' does not implement interface member 'GetCurrentWeatherAsync'` | sygnatury w `IMeteoService` muszą być identyczne z metodami serwisu (w tym `Task<Weather?>`) |
| `The type or namespace name 'Options' could not be found` | brakuje `using Microsoft.Extensions.Options;` albo pakietu `Microsoft.Extensions.Hosting` |
| nazwy krajów po angielsku | `DefaultLanguage` w `Options.Create` ma `"pl"`? |

---

## 5. P03: CommunityToolkit.Mvvm

**Cel:** ten sam ViewModel bez kodu powtarzalnego, generowany w czasie kompilacji.

### Teoria w pigułce

- **CommunityToolkit.Mvvm** (Microsoft) to biblioteka z `ObservableObject` (gotowy `INotifyPropertyChanged`) i **generatorem kodu** (source generator), który na podstawie atrybutów dopisuje do klasy `partial` brakujący kod.
- `[ObservableProperty]` na **właściwości częściowej** (partial property, C# 13, toolkit 8.4): Ty piszesz `public partial string CityName { get; set; }`, generator dopisuje pole, getter i setter z powiadomieniem. Do wersji 8.3 atrybut stawiało się na polu z małej litery (`private string cityName;`), a generator tworzył właściwość `CityName`; myliło to pole z właściwością. Nowa składnia wymaga .NET 9+ (masz 10).
- `[NotifyCanExecuteChangedFor(nameof(LoadCitiesCommand))]`: po zmianie właściwości komenda podnosi `CanExecuteChanged`.
- `[RelayCommand(CanExecute = nameof(CanLoadCities))]` na metodzie `LoadCitiesAsync` generuje właściwość `LoadCitiesCommand` (`IAsyncRelayCommand`); końcówka `Async` jest obcinana.
- `partial void OnSelectedCityChanged(City? value)`: „hak” wołany przez wygenerowany setter po zmianie wartości.
- `ObservableCollection<T>` sama powiadamia listę o `Add`/`Clear`, więc nie trzeba podmieniać całej kolekcji.
- Alternatywy: Prism, ReactiveUI, DevExpress MVVM. CommunityToolkit działa w WPF, MAUI, WinUI, Avalonii, dlatego używamy go do końca kursu.

### Krok po kroku

1. W folderze `ViewModels` dodaj `MainViewModel.cs` (wersja klasyczna zostaje w `Classic` do porównania):

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

2. W `MainWindow.xaml.cs` zamień `MainViewModelClassic` na `MainViewModel`:

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

3. W `MainWindow.xaml` zmień przestrzeń nazw w `xmlns:vm` i typ w `d:DataContext`:

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

4. Uruchom F5 i powtórz testy z sekcji 4.
5. Zobacz wygenerowany kod: Solution Explorer → P03 → **Dependencies → Analyzers → CommunityToolkit.Mvvm.SourceGenerators** → pliki `…MainViewModel.g.cs`. Znajdź setter `CityName` i właściwość `LoadCitiesCommand`.

### Sprawdź

- Okno działa identycznie jak w sekcji 4: `Warszawa`, `Kraków`, `xyzxyz`, puste pole.
- `MainWindow.xaml` nie zmienił żadnego bindingu (tylko `d:DataContext`).
- W wygenerowanym pliku widzisz `OnPropertyChanged`, `OnSelectedCityChanged` i `new AsyncRelayCommand(...)`: to, co napisałeś ręcznie w sekcji 2.

### Dlaczego tak

- `MainViewModelClassic` + `RelayCommand` = 146 + 36 linii; `MainViewModel` = 120 linii z komentarzami (bez nich ok. 80). Różnica rośnie z każdą właściwością, a co ważniejsze: nie da się zapomnieć `OnPropertyChanged`.
- `Cities` jako `ObservableCollection { get; }` (tylko do odczytu): podmieniasz zawartość (`Clear` + `Add`), nie obiekt. Lista nie musi odpinać się od starej kolekcji.
- `private async Task LoadCitiesAsync()` zamiast `async void`: wyjątek wraca do `await`, a `AsyncRelayCommand` sam blokuje ponowne wykonanie w trakcie.
- Wersja klasyczna zostaje w `ViewModels/Classic`: kompiluje się, nie jest używana, służy do porównania.

### Częste problemy

| Objaw | Rozwiązanie |
|---|---|
| `MVVMTK0045` / błąd przy `partial string` | wymagany C# 13+ (`net10.0` go ma) i toolkit 8.4+; sprawdź wersję pakietu |
| `'MainViewModel' does not contain a definition for 'LoadCitiesCommand'` | klasa nie jest `partial` albo metoda nie ma `[RelayCommand]`; zbuduj projekt (generator działa przy kompilacji) |
| pogoda nie pobiera się po wyborze miasta | `OnSelectedCityChanged` musi być `partial void` z dokładnie tą nazwą i typem parametru `City?` |
| przycisk nie wyłącza się przy pustym polu | brak `[NotifyCanExecuteChangedFor(nameof(LoadCitiesCommand))]` na `CityName` |

---

## 6. P03: konwertery i zasoby aplikacji

**Cel:** formatowanie i widoczność w widoku przez `IValueConverter`; zasoby wspólne w `App.xaml`.

### Teoria w pigułce

- **`IValueConverter`** ma `Convert` (ViewModel → widok) i `ConvertBack` (widok → ViewModel). Dla bindingu jednokierunkowego `ConvertBack` rzuca `NotSupportedException`: lepiej głośno niż cicho.
- Parametr `culture` to kultura z bindingu: `ToString("F1", culture)` da `12,5` na polskim Windowsie. Formatowanie **dla człowieka** może zależeć od kultury; parsowanie danych z API (wczoraj) nie.
- Konwerter jako zasób aplikacji to **jedna instancja** dostępna w każdym oknie.
- Kiedy konwerter, a kiedy właściwość w ViewModelu? Czysta prezentacja (format, kolor, widoczność) → konwerter. Logika („czy pokazać ostrzeżenie”) → `bool` w ViewModelu + `BoolToVisibility`. Trzecia droga: `DataTrigger` w stylu (bez C#, ale rozwlekle).

### Krok po kroku

1. Utwórz folder `Converters` z dwoma plikami:

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

2. Zastąp `App.xaml` (konwertery jako zasoby, dwa style niejawne; `StartupUri` zostaje do sekcji 7):

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

3. Końcowa wersja `MainWindow.xaml`: konwerter temperatury, ukrywanie pustego komunikatu, zasób usunięty z okna (jest w aplikacji), `Margin` przycisku usunięty (jest w stylu):

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

4. Uruchom F5.

### Sprawdź

- Temperatura wyświetla się jako np. `12,5 °C` (przecinek z kultury polskiej).
- Czerwony komunikat pojawia się tylko dla `xyzxyz` i **znika** po udanym wyszukaniu (bo `ErrorMessage = null` → `Collapsed`).
- Przyciski i etykiety mają odstępy ze stylów w `App.xaml`.

### Dlaczego tak

- `value is string { Length: 0 }` to wzorzec właściwości (C# 8): „napis o długości zero”.
- Zasoby w `App.xaml`, nie w `Window.Resources`: w dniu 13 dojdą kolejne okna i będą używać tych samych konwerterów.
- `xmlns:converters="clr-namespace:…Converters"` mapuje przestrzeń nazw C# na prefiks XAML; `<converters:NullToVisibilityConverter x:Key="…"/>` tworzy obiekt tej klasy.

### Częste problemy

| Objaw | Rozwiązanie |
|---|---|
| `XamlParseException: Cannot find resource named 'TemperatureToDisplay'` | konwerter nie jest w `App.xaml` albo klucz się różni (wielkość liter!) |
| `The tag 'TemperatureToDisplayConverter' does not exist in XML namespace` | brak `xmlns:converters` w `App.xaml` albo inna przestrzeń nazw w pliku `.cs` |
| temperatura `12.5 °C` z kropką | ustawienia regionalne Windows są angielskie; to poprawne zachowanie `culture` |

---

## 7. P03: Dependency Injection przez Generic Host

**Cel:** `App.xaml.cs` buduje hosta, rejestruje serwisy i tworzy okno; nigdzie w kodzie nie ma `new OpenMeteoService` ani `new MainViewModel`.

### Teoria w pigułce

- `Host.CreateApplicationBuilder()` daje budowniczego z domyślną konfiguracją: czyta `appsettings.json` z folderu aplikacji, `appsettings.{Environment}.json`, zmienne środowiskowe, User Secrets; włącza logowanie. `builder.Services` to kontener, `builder.Configuration` to konfiguracja.
- `Configure<AppSettings>(GetSection("AppSettings"))` binduje sekcję do klasy i rejestruje `IOptions<AppSettings>`.
- `AddHttpClient<IMeteoService, OpenMeteoService>` (typed client): rejestruje interfejs → implementacja i dostarcza `HttpClient` z `IHttpClientFactory`, która współdzieli połączenia i odświeża je co 2 minuty. Koniec problemu „jeden `HttpClient` na aplikację”.
- `GetRequiredService<T>()` rzuca wyjątek, gdy typ nie jest zarejestrowany; `GetService<T>()` zwraca `null`. W kodzie aplikacji używaj `GetRequiredService`.
- Jedyne miejsce, gdzie **Ty** pytasz kontener, to `OnStartup`. Reszta obiektów dostaje zależności konstruktorem. Wstrzykiwanie `IServiceProvider` do ViewModelu i wołanie `GetService` w środku to antywzorzec (service locator).
- Alternatywy: `new ServiceCollection().BuildServiceProvider()` (ten sam kontener bez konfiguracji i logowania; tak było w edycjach 26/31), Autofac, DryIoc. Standardem w .NET jest `Microsoft.Extensions.DependencyInjection`.

### Krok po kroku

1. W `App.xaml` zamień `StartupUri` na zdarzenia `Startup` i `Exit`:

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

2. Zastąp `App.xaml.cs`:

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

3. Końcowa wersja `MainWindow.xaml.cs`: ViewModel wchodzi konstruktorem:

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

4. Uruchom F5 i przetestuj jak w sekcji 4.
5. Podmiana implementacji: w `ConfigureServices` zakomentuj `AddHttpClient<…>` i odkomentuj `services.AddSingleton<IMeteoService, FakeMeteoService>();`. Uruchom. Przywróć.
6. Konfiguracja bez kompilacji: w `bin\Debug\net10.0-windows\appsettings.json` zmień `"pl"` na `"en"` i uruchom `.exe` z Eksploratora. Przywróć `pl`.
7. Brak rejestracji: zakomentuj `services.AddSingleton<MainViewModel>();`, uruchom, przeczytaj wyjątek. Przywróć.

### Sprawdź

- Okno działa jak w sekcji 6, a w kodzie nie ma `new OpenMeteoService`, `new HttpClient` ani `new MainViewModel` (Ctrl+Shift+F, żeby to potwierdzić).
- Krok 5: dane testowe bez zmiany ViewModelu i XAML.
- Krok 6: `Warsaw, Masovian Voivodeship (Poland)` bez kompilacji.
- Krok 7: `InvalidOperationException: Unable to resolve service for type 'MainViewModel' while attempting to activate 'MainWindow'`. Komunikat mówi, czego brakuje i kto tego chciał.

### Dlaczego tak

- `StartupUri` tworzyło okno konstruktorem bezparametrowym. Okno z parametrem `MainViewModel` musi tworzyć kontener, dlatego `Startup="OnStartup"` i `GetRequiredService<MainWindow>()`.
- `MainViewModel` jako singleton: jeden stan okna na aplikację. `MainWindow` jako transient: nowe okno przy każdym żądaniu.
- `OnExit` zatrzymuje hosta i wywołuje `Dispose` singletonów. Bez tego zasoby (np. logger) nie są zwalniane.
- `GetRequiredService` tylko w `OnStartup`: cała reszta grafu obiektów powstaje przez konstruktory. Dzięki temu zależności każdej klasy widać w jej sygnaturze.
- Ten sam `builder.Services`, `AddScoped`, `appsettings.json` zobaczysz jutro w `Program.cs` projektu Web API.

### Częste problemy

| Objaw | Rozwiązanie |
|---|---|
| aplikacja uruchamia się i od razu kończy, bez okna | brak `Startup="OnStartup"` w `App.xaml` albo brak `mainWindow.Show()` |
| dwa okna albo `MissingMethodException: No parameterless constructor` | `StartupUri` zostało w `App.xaml` razem z hostem; usuń je |
| `Unable to resolve service for type 'Microsoft.Extensions.Options.IOptions…'` | brak `builder.Services.Configure<AppSettings>(…)` |
| `Unable to resolve service for type 'System.Net.Http.HttpClient'` | serwis zarejestrowany przez `AddSingleton` zamiast `AddHttpClient` (albo brak pakietu `Microsoft.Extensions.Http`) |
| nazwy miast po angielsku mimo `"pl"` w projekcie | edytowałeś `appsettings.json` w `bin`, a nie w projekcie (albo odwrotnie); `PreserveNewest` kopiuje nowszy plik |

---

## 8. Zadanie: Z01AplikacjaNBP.Client

**Cel:** samodzielnie zbudować aplikację MVVM + DI według schematu z P03, na nowym API i z nową kontrolką (`DataGrid`).

### Treść zadania

Aplikacja WPF **Z01AplikacjaNBP.Client**, która:

1. Po kliknięciu **Pobierz kursy** pobiera tabelę A kursów średnich z API NBP: `https://api.nbp.pl/api/exchangerates/tables/A?format=json` i pokazuje ją w `DataGrid` z kolumnami **Kod**, **Waluta**, **Kurs średni (PLN)** (4 miejsca po przecinku), posortowaną po kodzie.
2. Nad tabelą pokazuje numer tabeli, datę i liczbę walut, np. `Tabela 196/A/NBP/2026 z dnia 09.10.2026 (32 walut)`.
3. Pod tabelą ma przelicznik: pole **Kwota w PLN** (domyślnie 100) i wynik `430,00 PLN = 98,23 EUR` dla waluty zaznaczonej w tabeli. Wynik przelicza się po każdej zmianie kwoty lub zaznaczenia.
4. Podczas pobierania pokazuje pasek postępu, a przycisk jest wyłączony. Błąd sieci to komunikat, nie wyjątek.
5. Architektura jak w P03: `Models/`, `Services/IExchangeRateService` + `NbpExchangeRateService`, `ViewModels/MainViewModel` (CommunityToolkit.Mvvm), `App.xaml.cs` z Generic Host i `AddHttpClient`, `MainWindow(MainViewModel)`.

### Wskazówki

- Otwórz adres w przeglądarce. Odpowiedź to **tablica** tabel: `[ { "table": "A", "no": "…", "effectiveDate": "2026-10-09", "rates": [ { "currency": "euro", "code": "EUR", "mid": 4.3775 }, … ] } ]`. Deserializuj do `ExchangeRateTable[]` i weź `FirstOrDefault()`.
- NBP używa camelCase; mapuj `[JsonPropertyName("effectiveDate")]`. Datę trzymaj w `DateOnly`, kurs w `decimal` (pieniądze nigdy w `double`).
- W `AddHttpClient` ustaw `client.BaseAddress = new Uri("https://api.nbp.pl/api/")`, a w serwisie użyj adresu względnego `exchangerates/tables/A?format=json`.
- `DataGrid`: `AutoGenerateColumns="False"`, `IsReadOnly="True"`, `CanUserAddRows="False"`, kolumny `DataGridTextColumn` z `Binding` i `StringFormat=N4`. `SelectedItem` działa jak w `ListBox`.
- Właściwość wyliczana (`ConvertedAmount`) nie ma settera, więc nie powiadomi sama. Użyj `[NotifyPropertyChangedFor(nameof(ConvertedAmount))]` na `AmountPln` i `SelectedRate`.
- `TextBox` zbindowany do `decimal`: gdy wpiszesz literę, WPF pokaże czerwoną ramkę i nie zapisze wartości. To wbudowana walidacja konwersji.
- Projekt: `dotnet new wpf -n Z01AplikacjaNBP.Client -o Z01AplikacjaNBP.Client -f net10.0`, `dotnet sln 32Dzien12.slnx add Z01AplikacjaNBP.Client/Z01AplikacjaNBP.Client.csproj`, te same trzy pakiety co w P03.

Daj sobie **45 minut**. Dopiero potem zajrzyj do rozwiązania.

### Rozwiązanie

Porównaj ze swoim: nie chodzi o identyczny kod, tylko o ten sam podział odpowiedzialności (model ↔ serwis ↔ ViewModel ↔ widok ↔ rejestracja).

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

### Sprawdź

- **Pobierz kursy** → tabela z 30+ walutami posortowana po kodzie (AUD, BGN, BRL, …), nad nią `Tabela …/A/NBP/… z dnia … (… walut)`.
- Zaznacz `EUR`, wpisz `430` → `430,00 PLN = 98,23 EUR` (u Ciebie aktualny kurs; sprawdź dzieląc 430 przez kurs z tabeli).
- Wpisz `abc` → czerwona ramka, wynik się nie zmienia.
- Wyłącz Wi‑Fi, kliknij **Pobierz kursy** → „Błąd połączenia: …”, aplikacja działa dalej.

### Dlaczego tak

- `decimal` dla kursu: `0.1 + 0.2` w `double` to `0.30000000000000004`; przy pieniądzach to błąd księgowy. `DateOnly` (od .NET 6) zamiast `DateTime` z godziną 00:00; `System.Text.Json` deserializuje `"2026-10-09"` bez dodatkowego kodu.
- `BaseAddress` w rejestracji, adres względny w serwisie: zmiana domeny to jedna linia w `App.xaml.cs` (w dniu 13 weźmiesz ją z `appsettings.json`).
- `ConvertedAmount` jest wyliczana; `[NotifyPropertyChangedFor]` na dwóch właściwościach wejściowych odświeża ją. W edycji 26 robiło się to ręcznie `OnPropertyChanged(nameof(…))` w setterze.
- `AmountPln / SelectedRate.Mid`: kurs to cena jednej jednostki waluty w PLN, więc dzielimy.
- Sortowanie w ViewModelu (logika prezentacji), nie w serwisie (dostęp do danych) ani w `DataGrid` (użytkownik może sortować po nagłówku, ale stan początkowy ustalasz Ty).
- `return` w `try` z `finally`: `IsBusy = false` wykona się mimo wczesnego wyjścia.

### Częste problemy

| Objaw | Rozwiązanie |
|---|---|
| okno się nie pokazuje | brak `Startup="OnStartup"` w `App.xaml` |
| `DataGrid` pusty, bez błędu | `AutoGenerateColumns="False"` bez zdefiniowanych kolumn, albo `ItemsSource` bindowany do pola zamiast właściwości |
| `JsonException: The JSON value could not be converted to ExchangeRateTable` | deserializujesz do pojedynczego obiektu, a API zwraca tablicę: użyj `ExchangeRateTable[]` |
| wynik przelicznika nie odświeża się | brak `[NotifyPropertyChangedFor(nameof(ConvertedAmount))]` |
| `Mid` wyświetla `4,3775000` albo `4.3775` | użyj `StringFormat=N4` i `decimal` |

---

## 9. Podsumowanie

Dziś nauczyłeś się:

1. że delegat to metoda w zmiennej, a `event` to delegat, który z zewnątrz można tylko subskrybować,
2. budować ViewModel z `INotifyPropertyChanged` i `ICommand` ręcznie, a potem generować ten sam kod przez CommunityToolkit.Mvvm (`[ObservableProperty]`, `[RelayCommand]`),
3. bindować tekst, listę, zaznaczenie, widoczność i komendy (`UpdateSourceTrigger`, `StringFormat`, `FallbackValue`, konwertery),
4. wstrzykiwać zależności przez interfejs i konstruktor, rejestrować je w kontenerze z cyklem życia Singleton/Transient i uruchamiać przez Generic Host z konfiguracją `IOptions`,
5. podmieniać implementację serwisu jedną linią.

Jutro (dzień 13) postawisz własne Web API w tym samym Generic Hoście i podłączysz do niego P03. `MainViewModel`, `IMeteoService` i rejestracje w `App.xaml.cs` zostają; dojdzie `IProductService` z biblioteki wspólnej `P06Shop.Shared`.

### Zapisz pracę

Solucja końcowa (Visual Studio dodało `README.md` i instrukcje do folderu **Solution Items**; możesz to pominąć):

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

```powershell
git init
git add .
git commit -m "Dzien 12: delegaty, MVVM (klasyczny + CommunityToolkit), binding, DI przez Generic Host, zadanie NBP"
```

### Zadania dodatkowe

1. P03: dodaj komendę `ClearCommand` (czyści listę, zaznaczenie i pogodę; `CanExecute` tylko, gdy lista nie jest pusta) i przycisk **Wyczyść** z `KeyBinding` na `Escape`. Podpowiedź: `[NotifyCanExecuteChangedFor]` nie zadziała dla `ObservableCollection`; wywołaj `ClearCommand.NotifyCanExecuteChanged()` po `Cities.Clear()`/`Add`.
2. P03: napisz program konsolowy, który tworzy ten sam host co `App.xaml.cs` (bez okna), pobiera `MainViewModel` z kontenera, ustawia `CityName = "Warszawa"`, wywołuje `await vm.LoadCitiesCommand.ExecuteAsync(null)` i wypisuje `Cities.Count`. To jest test ViewModelu bez WPF.
3. Z01: dodaj `ComboBox` z wyborem tabeli `A`/`B` (`exchangerates/tables/B`) zbindowany do właściwości `TableName`; serwis dostaje nazwę tabeli jako parametr.
4. Z01: przelicznik w drugą stronę (waluta → PLN) z przełącznikiem kierunku.

### Pytania do samodzielnego sprawdzenia

1. Czym różni się `event` od zwykłego pola typu delegata?
2. Co się stanie, gdy setter właściwości w ViewModelu nie wywoła `OnPropertyChanged`? Czy aplikacja się wywróci?
3. Po co `UpdateSourceTrigger=PropertyChanged` na `TextBox` z nazwą miasta?
4. Dlaczego `MainViewModel` przyjmuje `IMeteoService`, a nie `OpenMeteoService`? Podaj dwie korzyści.
5. Czym różni się `AddSingleton` od `AddTransient`? Co jest singletonem w P03, a co transientem?
6. Co generuje `[ObservableProperty]` na właściwości `partial` i dlaczego klasa musi być `partial`?
7. Dlaczego `StringFormat={}{0:F1} °C` ma `{}` na początku, a `StringFormat=N4` nie?
8. Gdzie w kodzie dnia 12 jest jedyne wywołanie `GetRequiredService` i dlaczego nie wołamy go w ViewModelu?

---

## Dodatek A: co zmieniono względem edycji 26 i 31

| Gdzie | Edycje 26/31 | Edycja 32 | Dlaczego |
|---|---|---|---|
| Pakiety | 26: .NET 8, toolkit 8.2.2, DI 8.0; 31: .NET 10, toolkit 8.4.2, DI 10.0.12, Newtonsoft | `CommunityToolkit.Mvvm 8.4.2`, `Microsoft.Extensions.Hosting 10.0.12`, `Microsoft.Extensions.Http 10.0.12`; bez `Configuration.Json` (zawarty w Hosting) i bez Newtonsoft | mniej pakietów, jeden host |
| P04Delegaty | 31: cztery przypisania `Action`/`Func`/`Predicate` | własny delegat, metoda wyższego rzędu, LINQ, multicast, `event` z `EventArgs`, mapa „gdzie to w MVVM” | zrozumienie `RelayCommand` i `PropertyChanged` |
| ViewModel klasyczny | `BaseViewModel` + pusty `IMainViewModel`, `CityName` bez powiadomienia, `async void` bez obsługi błędów | `ViewModels/Classic/MainViewModelClassic.cs` w jednym pliku, wszystkie właściwości powiadamiają, `async Task`, `IsBusy`, `ErrorMessage`, `CanExecute` | kompletny wzorzec w jednym miejscu |
| Toolkit | `[ObservableProperty]` na polu z małej litery, `[RelayCommand] public async void`, kolejne wersje V2/V3 | właściwości częściowe (C# 13), `[NotifyCanExecuteChangedFor]`, `private async Task` z `CanExecute`, `partial void On…Changed`, `ObservableCollection { get; }` | nowa składnia, `async void` wywracał aplikację |
| Binding | bez `UpdateSourceTrigger`, `FallbackValue`, `KeyBinding`, `d:DataContext` | wszystkie cztery, `ProgressBar` z `IsBusy`, komunikat z `NullToVisibility` | okno reaguje na każdą literę, IntelliSense w XAML |
| Konwertery | 26: `TemperatureToDisplayConveter` (literówka), bez `culture`, zasoby w oknie | poprawna nazwa, `culture`, `NotSupportedException`, `NullToVisibilityConverter`, zasoby w `App.xaml` | zasoby wspólne dla kolejnych okien |
| Serwis | 31: singleton tworzący `HttpClient` w `using` przy każdym zapytaniu; 26: AccuWeather z kluczem API | `AddHttpClient<IMeteoService, OpenMeteoService>`, `IOptions<AppSettings>` | `IHttpClientFactory`, bez klucza |
| DI | `new ServiceCollection()`, `GetService` (może być `null`), bez zwalniania | Generic Host, `Configure<AppSettings>`, `GetRequiredService`, `OnExit` | ten sam model co ASP.NET Core |
| Drugie okno | `SecondWindow` przez `IServiceProvider` w ViewModelu | usunięte | service locator to antywzorzec; dialogi wracają w dniu 14 |
| Zadanie NBP | 31: Newtonsoft, `HttpClient` w `using`, `double`, `DataContext = new …` w code‑behind, bez błędów i `IsBusy` | `Z01AplikacjaNBP.Client`: `System.Text.Json`, `decimal`, `DateOnly`, `BaseAddress`, Generic Host, przelicznik, `IsBusy`, `ErrorMessage`, sortowanie | ten sam schemat co P03 |
| Etapy pośrednie | brak (kod końcowy od razu) | `FakeMeteoService` + `Options.Create` w wersjach tymczasowych | projekt kompiluje się po każdej sekcji |

## Dodatek B: jak zweryfikowano tę instrukcję

- Cały kod końcowy (`P03WeatherForecastWPF.Client`, `P04Delegaty`, `Z01AplikacjaNBP.Client`) zbudowano `dotnet build 32Dzien12.slnx` z .NET SDK 10.0.401 bez błędów i ostrzeżeń. Zbudowano też każdy etap pośredni (stan po sekcjach 3, 4, 5, 6, 7) z plikami „wersja tymczasowa” w dokładnie tej treści, jaka jest w instrukcji.
- `P04Delegaty` uruchomiono z konsoli; wyjście w sekcji 1 jest skopiowane z tego uruchomienia.
- Logikę `MainViewModel` (P03) i `MainViewModel` (Z01) przetestowano w programie konsolowym z tym samym kontenerem DI co w `App.xaml.cs`: `LoadCitiesCommand` dla `Warszawa` zwraca 2 miasta, ustawienie `SelectedCity` pobiera pogodę, `xyzxyz` daje `ErrorMessage = "Nie znaleziono miasta o takiej nazwie."`, pusta nazwa wyłącza komendę (`CanExecute` zwraca `false`). NBP: tabela A z 32 walutami, `430` PLN i `EUR` dają `430,00 PLN = 98,23 EUR` (kurs 4,3775 z tabeli z 2026‑10‑09).
- Okien WPF nie uruchamiano (środowisko bez Windows). Jeśli coś w XAML zachowuje się inaczej niż w punkcie **Sprawdź**, zacznij od okna Output i tabeli **Częste problemy**.
