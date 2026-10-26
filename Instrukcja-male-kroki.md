# Dzień 12 w małych krokach: MVVM i Dependency Injection

> **Dla kogo:** uzupełnienie głównej instrukcji dnia 12 (`Instrukcja.md`). Tam przebudowujemy od razu całą aplikację pogodową. Tu robimy to samo, ale na **dziesięciu malutkich programach**, każdy o jednym temacie. Każdy program ma 2–5 krótkich plików i daje się napisać w 10–20 minut.
>
> **Technologia:** .NET 10, C# 14, WPF, `CommunityToolkit.Mvvm 8.4.2`, `Microsoft.Extensions.DependencyInjection 10.0.12`, `Microsoft.Extensions.Hosting 10.0.12`. Visual Studio 2026 albo VS Code + CLI `dotnet`.
>
> **Gotowy kod:** folder `MaleKroki` w repozytorium (solucja `MaleKroki/MaleKroki.slnx`). Każdy projekt jest niezależny – można otworzyć dowolny i uruchomić.

## Mapa kroków

| Krok | Projekt | Jeden temat | Co z niego zostaje w P03 |
|---|---|---|---|
| 1 | `K01Binding` | binding czyta dane, ale **nie widzi zmian** | `DataContext`, `{Binding …}` |
| 2 | `K02NotifyPropertyChanged` | `INotifyPropertyChanged` napisany ręcznie | zrozumienie, co generuje toolkit |
| 3 | `K03ObservableProperty` | to samo przez `ObservableObject` + `[ObservableProperty]` | wszystkie właściwości ViewModelu |
| 4 | `K04RelayCommand` | `[RelayCommand]`, `CanExecute`, pusty code‑behind | wszystkie przyciski |
| 5 | `K05TwoWayBinding` | `TextBox` ↔ ViewModel, `UpdateSourceTrigger`, `StringFormat`, właściwość wyliczana | pole „Miasto” |
| 6 | `K06Collections` | `ObservableCollection`, `ItemsSource`, `SelectedItem` | lista miast |
| 7 | `K07AsyncCommand` | komenda asynchroniczna, `IsBusy`, pasek postępu | pobieranie pogody |
| 8 | `K08DiManual` (konsola) | zależność przez interfejs i konstruktor – bez żadnej biblioteki | `IMeteoService` |
| 9 | `K09DiContainer` (konsola) | kontener DI: rejestracja, `GetRequiredService`, Singleton a Transient | rejestracje w `App.xaml.cs` |
| 10 | `K10DiWpf` | MVVM + Generic Host + `appsettings.json` w jednym małym oknie | cały `App.xaml.cs` z P03 |

Kroki 1–7 to **MVVM**, kroki 8–10 to **DI**. Każdy krok zakłada, że poprzedni działa, ale kod jest zawsze kompletny: można zacząć od dowolnego miejsca.

## Jak korzystać z tej instrukcji

- Bloki kodu są **gotowe do skopiowania w całości**. W pierwszej linii każdego bloku jest ścieżka pliku (komentarz). Blok zastępuje całą zawartość tego pliku.
- Każdy krok ma punkt **Sprawdzamy** (co ma działać) i **Eksperyment** (co celowo zepsuć, żeby zobaczyć skutek). Eksperymenty są najważniejszą częścią – bez nich MVVM wygląda jak magia.
- Nowy projekt WPF tworzymy w Visual Studio (**Add → New Project → WPF Application**, .NET 10) albo z konsoli, w folderze `MaleKroki`:

```powershell
dotnet new sln -n MaleKroki --format slnx
dotnet new wpf -n K01Binding -o K01Binding -f net10.0
dotnet sln MaleKroki.slnx add K01Binding/K01Binding.csproj
```

- Szablon `wpf` tworzy pliki `App.xaml`, `App.xaml.cs`, `MainWindow.xaml`, `MainWindow.xaml.cs`, `AssemblyInfo.cs` i `.csproj`. W krokach podajemy tylko pliki, które zmieniamy lub dodajemy; `AssemblyInfo.cs` zostawiamy bez zmian. Gdy krok wymaga pakietu NuGet, jest to napisane przy pliku `.csproj`.
- Projekt uruchamiamy F5 (po **Set as Startup Project**) albo `dotnet run --project K01Binding`.

## Teoria na jednej stronie

**Problem.** W dniu 11 cała logika okna była w `MainWindow.xaml.cs`: metody `…_Click` czytały `txtCity.Text`, wołały serwis i wpisywały wynik do `lblTemperature.Content`. Takiego kodu nie da się przetestować bez okna i nie da się go użyć w innym oknie ani w MAUI.

**MVVM** rozdziela to na trzy części:

| Warstwa | Co to jest | W naszych krokach |
|---|---|---|
| **Model** | dane i logika biznesowa (serwisy, klasy DTO) | `Counter`, `IClock`, `Greeter` |
| **View** | okno w XAML; **nie ma** logiki, tylko bindingi | `MainWindow.xaml` |
| **ViewModel** | stan okna (właściwości) i akcje (komendy); **nie zna** kontrolek | `CounterViewModel`, `TodoViewModel`, … |

Okno i ViewModel łączą dwa mechanizmy WPF: **binding** (właściwość ViewModelu ↔ właściwość kontrolki) i **komendy** (`ICommand` ↔ przycisk). Żeby binding wiedział, że wartość się zmieniła, ViewModel musi podnieść zdarzenie `PropertyChanged` (interfejs `INotifyPropertyChanged`). CommunityToolkit.Mvvm generuje ten kod za nas.

**Dependency Injection** to zasada: klasa **nie tworzy** obiektów, których potrzebuje (`new HttpClient()`, `new OpenMeteoService()`), tylko **dostaje je konstruktorem**, najlepiej jako interfejsy. Dzięki temu można podmienić implementację (prawdziwy serwis ↔ udawany) bez zmiany klasy. **Kontener DI** to obiekt, który na podstawie rejestracji („`IClock` → `SystemClock`”) sam składa cały łańcuch zależności.

---

## Krok 1. K01Binding: binding, który nie widzi zmian

**Cel:** zobaczyć, że `{Binding}` czyta wartość z `DataContext`, ale sam z siebie nie dowie się o jej zmianie.

### Kroki

1. Tworzymy projekt WPF `K01Binding`. Pakiety nie są potrzebne.
2. Dodajemy klasę `Counter.cs`:

```csharp
// K01Binding/Counter.cs
namespace K01Binding;

/// <summary>
/// Zwykła klasa z jedną właściwością. Nie powiadamia nikogo o zmianie wartości.
/// </summary>
public class Counter
{
    public int Value { get; set; }
}
```

3. Zastępujemy `MainWindow.xaml`:

```xml
<!-- K01Binding/MainWindow.xaml -->
<Window x:Class="K01Binding.MainWindow"
        xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
        xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
        Title="K01 – binding bez powiadomień" Height="200" Width="320">
    <StackPanel Margin="16">
        <TextBlock Text="Wartość licznika:" />

        <!-- Binding: TextBlock czyta właściwość Value z obiektu w DataContext -->
        <TextBlock Text="{Binding Value}" FontSize="32" FontWeight="Bold" />

        <Button Content="+1" Click="Increment_Click" Padding="8,4" />
    </StackPanel>
</Window>
```

4. Zastępujemy `MainWindow.xaml.cs`:

```csharp
// K01Binding/MainWindow.xaml.cs
using System.Windows;

namespace K01Binding;

public partial class MainWindow : Window
{
    private readonly Counter _counter = new();

    public MainWindow()
    {
        InitializeComponent();

        // DataContext: obiekt, z którego bindingi w XAML biorą dane
        DataContext = _counter;
    }

    private void Increment_Click(object sender, RoutedEventArgs e)
    {
        _counter.Value++;

        // Tytuł okna ustawiamy ręcznie – będzie się zmieniał.
        // TextBlock z bindingiem NIE zmieni się, bo Counter nie powiadamia o zmianie.
        Title = $"Value = {_counter.Value}";
    }
}
```

### Sprawdzamy

Po uruchomieniu widać `0`. Po kliknięciu **+1** tytuł okna zmienia się na `Value = 1`, `Value = 2`…, ale duża liczba w oknie **cały czas pokazuje 0**.

### Dlaczego tak

- `DataContext = _counter` mówi: „wszystkie bindingi w tym oknie szukają właściwości w obiekcie `_counter`”. `{Binding Value}` znaczy `_counter.Value`.
- Binding odczytał `Value` **raz**, przy tworzeniu okna. Potem nikt go nie poinformował, że wartość się zmieniła. `Counter` to zwykła klasa – nie ma jak powiadomić.
- To jest dokładnie ten problem, który rozwiązuje `INotifyPropertyChanged` w kroku 2.

### Eksperyment

W `Increment_Click` dopiszmy na końcu `DataContext = null; DataContext = _counter;`. Liczba zacznie się odświeżać, bo zmusiliśmy WPF do ponownego odczytania wszystkich bindingów. Działa, ale tak się nie robi: odświeżamy całe okno, zamiast jednej właściwości. Usuwamy te dwie linie.

---

## Krok 2. K02NotifyPropertyChanged: powiadamianie ręcznie

**Cel:** napisać `INotifyPropertyChanged` własnoręcznie, żeby wiedzieć, co później wygeneruje toolkit.

### Kroki

1. Tworzymy projekt WPF `K02NotifyPropertyChanged` (albo kopiujemy K01 i zmieniamy nazwy).
2. Klasa `Counter.cs` – tym razem z interfejsem:

```csharp
// K02NotifyPropertyChanged/Counter.cs
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace K02NotifyPropertyChanged;

/// <summary>
/// Ta sama klasa, ale z INotifyPropertyChanged: po każdej zmianie Value
/// podnosi zdarzenie PropertyChanged, na które nasłuchuje binding w XAML.
/// </summary>
public class Counter : INotifyPropertyChanged
{
    private int _value;

    public int Value
    {
        get => _value;
        set
        {
            if (_value == value)
            {
                return;   // bez zmiany – nie powiadamiamy
            }

            _value = value;
            OnPropertyChanged();   // "Value się zmieniło" – binding odświeży TextBlock
        }
    }

    // Zdarzenie wymagane przez interfejs. WPF podpina się do niego przy tworzeniu bindingu.
    public event PropertyChangedEventHandler? PropertyChanged;

    // [CallerMemberName]: kompilator sam wstawia nazwę właściwości, z której wołamy metodę
    protected void OnPropertyChanged([CallerMemberName] string? propertyName = null) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}
```

3. `MainWindow.xaml` jest taki sam jak w K01 (zmienia się tylko `x:Class` i tytuł):

```xml
<!-- K02NotifyPropertyChanged/MainWindow.xaml -->
<Window x:Class="K02NotifyPropertyChanged.MainWindow"
        xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
        xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
        Title="K02 – INotifyPropertyChanged" Height="200" Width="320">
    <StackPanel Margin="16">
        <TextBlock Text="Wartość licznika:" />

        <!-- Binding: TextBlock czyta właściwość Value z obiektu w DataContext -->
        <TextBlock Text="{Binding Value}" FontSize="32" FontWeight="Bold" />

        <Button Content="+1" Click="Increment_Click" Padding="8,4" />
    </StackPanel>
</Window>
```

4. `MainWindow.xaml.cs` – w kliknięciu zmieniamy już **tylko obiekt**:

```csharp
// K02NotifyPropertyChanged/MainWindow.xaml.cs
using System.Windows;

namespace K02NotifyPropertyChanged;

public partial class MainWindow : Window
{
    private readonly Counter _counter = new();

    public MainWindow()
    {
        InitializeComponent();
        DataContext = _counter;
    }

    private void Increment_Click(object sender, RoutedEventArgs e)
    {
        // Zmieniamy tylko obiekt. TextBlock odświeży się sam dzięki PropertyChanged.
        _counter.Value++;
    }
}
```

### Sprawdzamy

Po każdym kliknięciu **+1** duża liczba rośnie. Okno nie wie nic o kontrolkach – zmieniamy `_counter.Value`, a `TextBlock` odświeża się sam.

### Dlaczego tak

- WPF przy tworzeniu bindingu sprawdza, czy obiekt w `DataContext` implementuje `INotifyPropertyChanged`. Jeśli tak, **subskrybuje** zdarzenie `PropertyChanged` (to zwykłe zdarzenie C#, jak w `P04Delegaty`).
- Setter po zmianie woła `OnPropertyChanged()`. Atrybut `[CallerMemberName]` sprawia, że kompilator sam wstawia nazwę `"Value"` – nie ma ryzyka literówki w napisie.
- `if (_value == value) return;` – bez powiadamiania, gdy nic się nie zmieniło. Oszczędza odświeżania i chroni przed pętlami przy bindingu w dwie strony (krok 5).
- To jest 25 linii na **jedną** właściwość. ViewModel z dziesięcioma właściwościami to 250 linii tego samego. Stąd krok 3.

### Eksperyment

Usuwamy linię `OnPropertyChanged();` z settera. Wracamy do zachowania z K01: liczba stoi w miejscu. Przywracamy.

---

## Krok 3. K03ObservableProperty: toolkit generuje powiadamianie

**Cel:** zastąpić 25 linii trzema przy użyciu CommunityToolkit.Mvvm i zobaczyć, co wygenerował.

### Kroki

1. Tworzymy projekt WPF `K03ObservableProperty` i dodajemy pakiet `CommunityToolkit.Mvvm` (prawy przycisk na projekcie → **Manage NuGet Packages** → wyszukaj → **Install**, wersja 8.4.2). Z konsoli: `dotnet add K03ObservableProperty package CommunityToolkit.Mvvm --version 8.4.2`. Plik projektu:

```xml
<!-- K03ObservableProperty/K03ObservableProperty.csproj -->
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
  </ItemGroup>

</Project>
```

2. Dodajemy `CounterViewModel.cs` (nazwa z końcówką `ViewModel` to konwencja – klasa ma dokładnie tę samą rolę, co `Counter` w K02):

```csharp
// K03ObservableProperty/CounterViewModel.cs
using CommunityToolkit.Mvvm.ComponentModel;

namespace K03ObservableProperty;

/// <summary>
/// To samo, co Counter z K02, ale 3 linie zamiast 25.
/// ObservableObject ma gotowe INotifyPropertyChanged,
/// a [ObservableProperty] generuje pole, getter i setter z powiadomieniem.
/// Klasa musi być partial, bo generator dopisuje do niej drugą część.
/// </summary>
public partial class CounterViewModel : ObservableObject
{
    [ObservableProperty]
    public partial int Value { get; set; }
}
```

3. `MainWindow.xaml` jak w K01/K02 (`x:Class="K03ObservableProperty.MainWindow"`):

```xml
<!-- K03ObservableProperty/MainWindow.xaml -->
<Window x:Class="K03ObservableProperty.MainWindow"
        xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
        xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
        Title="K03 – [ObservableProperty]" Height="200" Width="320">
    <StackPanel Margin="16">
        <TextBlock Text="Wartość licznika:" />

        <!-- Binding: TextBlock czyta właściwość Value z obiektu w DataContext -->
        <TextBlock Text="{Binding Value}" FontSize="32" FontWeight="Bold" />

        <Button Content="+1" Click="Increment_Click" Padding="8,4" />
    </StackPanel>
</Window>
```

4. `MainWindow.xaml.cs`:

```csharp
// K03ObservableProperty/MainWindow.xaml.cs
using System.Windows;

namespace K03ObservableProperty;

public partial class MainWindow : Window
{
    private readonly CounterViewModel _viewModel = new();

    public MainWindow()
    {
        InitializeComponent();
        DataContext = _viewModel;
    }

    private void Increment_Click(object sender, RoutedEventArgs e)
    {
        _viewModel.Value++;
    }
}
```

### Sprawdzamy

Działa tak samo jak K02. Różnica jest w kodzie: `CounterViewModel` ma 3 linie treści zamiast 25.

### Dlaczego tak

- `ObservableObject` to klasa bazowa z gotowym `INotifyPropertyChanged` (zdarzenie, `OnPropertyChanged`, `SetProperty`).
- `[ObservableProperty]` na **właściwości częściowej** (`partial`, C# 13+): my piszemy nagłówek, generator kodu w czasie kompilacji dopisuje pole `_value`, getter i setter z `if (_value == value) return;` i powiadomieniem. Dlatego klasa musi być `partial` – generator dopisuje jej drugą połowę.
- Wygenerowany kod można **zobaczyć**: w Solution Explorer rozwijamy **Dependencies → Analyzers → CommunityToolkit.Mvvm.SourceGenerators → …ObservablePropertyGenerator** → plik `K03ObservableProperty.CounterViewModel.g.cs`. Jest w nim dokładnie to, co napisaliśmy ręcznie w K02 (plus dwa „haki” `OnValueChanging`/`OnValueChanged`, których użyjemy w P03).
- W starszych wersjach toolkitu (do 8.3) atrybut stawiało się na polu `private int value;`, a właściwość `Value` powstawała „z powietrza”. W 8.4 z właściwością `partial` jest jedna nazwa i zero pól – mniej pomyłek.

### Eksperyment

Usuwamy słowo `partial` z klasy. Kompilator zgłasza błąd `MVVMTK…` z wyjaśnieniem, że klasa musi być częściowa. Przywracamy. Drugi eksperyment: usuwamy atrybut `[ObservableProperty]` i zostawiamy `public partial int Value { get; set; }` – błąd, bo właściwość częściowa musi mieć gdzieś implementację. To generator ją dostarcza.

---

## Krok 4. K04RelayCommand: przyciski bez code‑behind

**Cel:** przenieść logikę kliknięć do ViewModelu przez komendy i zostawić w `MainWindow.xaml.cs` jedną linię.

### Teoria

- `ICommand` to interfejs z dwiema metodami: `Execute` (co zrobić) i `CanExecute` (czy wolno). Przycisk z `Command="{Binding …}"` sam woła `CanExecute` i **wyłącza się**, gdy wynik to `false`.
- `[RelayCommand]` na metodzie `Increment` generuje właściwość `IncrementCommand`. Z `CanExecute = nameof(CanDecrement)` generator podpina metodę warunku.
- Przycisk nie wie, **kiedy** zapytać ponownie o `CanExecute`. Mówi mu o tym `[NotifyCanExecuteChangedFor(nameof(DecrementCommand))]` na właściwości: po zmianie `Value` komenda podnosi `CanExecuteChanged`.

### Kroki

1. Projekt WPF `K04RelayCommand` z pakietem `CommunityToolkit.Mvvm 8.4.2` (plik `.csproj` jak w K03, zmienia się tylko nazwa).
2. `CounterViewModel.cs`:

```csharp
// K04RelayCommand/CounterViewModel.cs
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace K04RelayCommand;

/// <summary>
/// Logika przycisków przeniesiona z code-behind do ViewModelu.
/// [RelayCommand] na metodzie Increment generuje właściwość IncrementCommand (ICommand),
/// którą przycisk binduje przez Command="{Binding IncrementCommand}".
/// </summary>
public partial class CounterViewModel : ObservableObject
{
    // Po zmianie Value komenda Decrement ma ponownie sprawdzić CanExecute
    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(DecrementCommand))]
    public partial int Value { get; set; }

    [RelayCommand]
    private void Increment() => Value++;

    // CanExecute: przycisk "-1" jest aktywny tylko, gdy Value > 0
    [RelayCommand(CanExecute = nameof(CanDecrement))]
    private void Decrement() => Value--;

    private bool CanDecrement() => Value > 0;

    [RelayCommand]
    private void Reset() => Value = 0;
}
```

3. `MainWindow.xaml` – zamiast `Click` jest `Command`:

```xml
<!-- K04RelayCommand/MainWindow.xaml -->
<Window x:Class="K04RelayCommand.MainWindow"
        xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
        xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
        Title="K04 – [RelayCommand]" Height="220" Width="320">
    <StackPanel Margin="16">
        <TextBlock Text="Wartość licznika:" />
        <TextBlock Text="{Binding Value}" FontSize="32" FontWeight="Bold" />

        <!-- Command zamiast Click: brak metod w code-behind -->
        <StackPanel Orientation="Horizontal">
            <Button Content="+1" Command="{Binding IncrementCommand}" Padding="12,4" Margin="0,0,8,0" />
            <Button Content="-1" Command="{Binding DecrementCommand}" Padding="12,4" Margin="0,0,8,0" />
            <Button Content="Zeruj" Command="{Binding ResetCommand}" Padding="12,4" />
        </StackPanel>
    </StackPanel>
</Window>
```

4. `MainWindow.xaml.cs` – zero logiki:

```csharp
// K04RelayCommand/MainWindow.xaml.cs
using System.Windows;

namespace K04RelayCommand;

/// <summary>
/// Code-behind w MVVM: jedna linia – podpięcie ViewModelu. Żadnej logiki.
/// </summary>
public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        DataContext = new CounterViewModel();
    }
}
```

### Sprawdzamy

- Na starcie przycisk **-1** jest szary (nieaktywny), bo `Value` = 0.
- Po **+1** przycisk **-1** staje się aktywny. Po zejściu do 0 znów gaśnie.
- **Zeruj** zawsze działa.
- W `MainWindow.xaml.cs` nie ma żadnej metody `…_Click`.

### Dlaczego tak

- Metody z `[RelayCommand]` mogą być `private` – okno i tak widzi tylko wygenerowaną publiczną właściwość `…Command`.
- Komendę da się przetestować bez okna: `vm.IncrementCommand.Execute(null); Assert(vm.Value == 1)`. Metody `…_Click` z K01 nie dało się wywołać bez WPF.
- Ten sam ViewModel zadziała w MAUI (dzień 18) – tam też są `Command` i binding.

### Eksperyment

Usuwamy linię `[NotifyCanExecuteChangedFor(nameof(DecrementCommand))]`. Po **+1** przycisk **-1** zostaje szary, mimo że `CanDecrement()` zwraca już `true` – nikt nie powiedział przyciskowi, żeby zapytał ponownie. Przywracamy.

---

## Krok 5. K05TwoWayBinding: pole tekstowe w dwie strony

**Cel:** zrozumieć binding z `TextBox` do ViewModelu, moment zapisu (`UpdateSourceTrigger`) i właściwości wyliczane.

### Teoria

- Binding ma **kierunek** (`Mode`): `OneWay` (ViewModel → okno, domyślny dla `TextBlock`), `TwoWay` (w obie strony, domyślny dla `TextBox.Text`), `OneTime` (raz).
- `TextBox` domyślnie zapisuje do ViewModelu dopiero przy **utracie fokusu** (`LostFocus`). `UpdateSourceTrigger=PropertyChanged` zapisuje po każdym znaku.
- Właściwość **wyliczana** (`Greeting`) nie ma settera ani pola. Żeby okno ją odświeżyło, ktoś musi podnieść `PropertyChanged("Greeting")` – robi to `[NotifyPropertyChangedFor(nameof(Greeting))]` na `Name`.
- `StringFormat` formatuje wartość w XAML, bez konwertera i bez C#.

### Kroki

1. Projekt WPF `K05TwoWayBinding` z pakietem `CommunityToolkit.Mvvm 8.4.2`.
2. `GreetingViewModel.cs`:

```csharp
// K05TwoWayBinding/GreetingViewModel.cs
using CommunityToolkit.Mvvm.ComponentModel;

namespace K05TwoWayBinding;

/// <summary>
/// Binding w dwie strony: TextBox zapisuje do Name, a Name i Greeting wracają do okna.
/// </summary>
public partial class GreetingViewModel : ObservableObject
{
    // Gdy zmieni się Name, okno ma też odświeżyć Greeting (właściwość wyliczaną)
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(Greeting))]
    public partial string Name { get; set; } = string.Empty;

    // Właściwość tylko do odczytu, liczona z Name. Nie ma własnego pola ani settera.
    public string Greeting =>
        string.IsNullOrWhiteSpace(Name) ? "Wpisz swoje imię." : $"Cześć, {Name.Trim()}!";
}
```

3. `MainWindow.xaml`:

```xml
<!-- K05TwoWayBinding/MainWindow.xaml -->
<Window x:Class="K05TwoWayBinding.MainWindow"
        xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
        xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
        Title="K05 – binding w dwie strony" Height="260" Width="360">
    <StackPanel Margin="16">
        <TextBlock Text="Imię (odświeża przy każdym znaku):" />
        <!-- UpdateSourceTrigger=PropertyChanged: ViewModel dostaje tekst po każdym naciśniętym klawiszu -->
        <TextBox Text="{Binding Name, UpdateSourceTrigger=PropertyChanged}" Margin="0,4,0,12" />

        <TextBlock Text="Imię (odświeża po opuszczeniu pola):" />
        <!-- Domyślnie TextBox zapisuje do ViewModelu dopiero przy utracie fokusu (LostFocus) -->
        <TextBox Text="{Binding Name}" Margin="0,4,0,12" />

        <TextBlock Text="{Binding Greeting}" FontSize="20" FontWeight="Bold" />

        <!-- StringFormat: formatowanie wartości bez kodu C# -->
        <TextBlock Text="{Binding Name.Length, StringFormat='Liczba znaków: {0}'}" Foreground="Gray" />
    </StackPanel>
</Window>
```

4. `MainWindow.xaml.cs`:

```csharp
// K05TwoWayBinding/MainWindow.xaml.cs
using System.Windows;

namespace K05TwoWayBinding;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        DataContext = new GreetingViewModel();
    }
}
```

### Sprawdzamy

- Piszemy w **pierwszym** polu: powitanie i licznik znaków zmieniają się po każdej literze. Drugie pole też się aktualizuje (ma binding do tej samej właściwości).
- Piszemy w **drugim** polu: nic się nie dzieje, dopóki nie klikniemy gdzie indziej (Tab albo klik w pierwsze pole). Wtedy wszystko się odświeża.
- Pusty tekst → „Wpisz swoje imię.”

### Dlaczego tak

- Oba `TextBox` bindują `Name` w trybie `TwoWay`. Różni je tylko **moment** zapisu do ViewModelu.
- Dla przycisku „Szukaj” z `CanExecute` (jak w P03) potrzebny jest `UpdateSourceTrigger=PropertyChanged` – inaczej przycisk odblokuje się dopiero po opuszczeniu pola, co użytkownik odbiera jako błąd.
- `Name.Length` w bindingu to **ścieżka**: binding może wchodzić w głąb właściwości (`Name.Length`, `SelectedCity.Country`).
- `StringFormat='Liczba znaków: {0}'` – apostrofy są potrzebne, bo w tekście są nawiasy klamrowe i dwukropek, które XAML inaczej potraktuje jako składnię bindingu.

### Eksperyment

Usuwamy `[NotifyPropertyChangedFor(nameof(Greeting))]`. Pole „Liczba znaków” nadal działa (binding do `Name.Length` dostaje powiadomienie o `Name`), ale **powitanie stoi w miejscu** – `Greeting` nikt nie powiadomił. Przywracamy.

---

## Krok 6. K06Collections: lista, zaznaczenie, dodawanie i usuwanie

**Cel:** zbindować kolekcję do `ListBox`, odczytać zaznaczony element w ViewModelu i uzależnić od niego komendę.

### Teoria

- `ObservableCollection<T>` to lista, która sama powiadamia kontrolkę o `Add`/`Remove`/`Clear` (interfejs `INotifyCollectionChanged`). Zwykła `List<T>` tego nie robi – `ListBox` nie zauważy dodanego elementu.
- `ItemsSource="{Binding Tasks}"` – skąd brać elementy. `SelectedItem="{Binding SelectedTask}"` – dokąd zapisać zaznaczenie (`TwoWay`: zmiana `SelectedTask` w kodzie zaznaczy element na liście).
- Właściwość kolekcji jest tylko do odczytu (`{ get; }`): **nie podmieniamy** obiektu kolekcji, tylko jej zawartość. Dzięki temu binding podpięty raz działa cały czas.

### Kroki

1. Projekt WPF `K06Collections` z pakietem `CommunityToolkit.Mvvm 8.4.2`.
2. `TodoViewModel.cs`:

```csharp
// K06Collections/TodoViewModel.cs
using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace K06Collections;

/// <summary>
/// Lista zadań: ObservableCollection sama powiadamia ListBox o Add/Remove,
/// SelectedItem z listy ląduje w SelectedTask, a komendy mają CanExecute.
/// </summary>
public partial class TodoViewModel : ObservableObject
{
    // Właściwość tylko do odczytu: podmieniamy zawartość kolekcji, nigdy sam obiekt
    public ObservableCollection<string> Tasks { get; } = ["Kupić mleko", "Oddać książkę"];

    // Tekst z pola "nowe zadanie". Po każdej zmianie przycisk Dodaj sprawdza CanExecute.
    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(AddCommand))]
    public partial string NewTask { get; set; } = string.Empty;

    // Element zaznaczony na liście (null, gdy nic nie zaznaczono)
    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(RemoveCommand))]
    public partial string? SelectedTask { get; set; }

    [RelayCommand(CanExecute = nameof(CanAdd))]
    private void Add()
    {
        Tasks.Add(NewTask.Trim());
        NewTask = string.Empty;   // czyścimy pole – TextBox odświeży się przez binding
    }

    private bool CanAdd() => !string.IsNullOrWhiteSpace(NewTask);

    [RelayCommand(CanExecute = nameof(CanRemove))]
    private void Remove()
    {
        Tasks.Remove(SelectedTask!);
        SelectedTask = null;
    }

    private bool CanRemove() => SelectedTask is not null;
}
```

3. `MainWindow.xaml`:

```xml
<!-- K06Collections/MainWindow.xaml -->
<Window x:Class="K06Collections.MainWindow"
        xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
        xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
        Title="K06 – kolekcje i zaznaczenie" Height="340" Width="380">
    <DockPanel Margin="16">

        <!-- Górny pasek: nowe zadanie + Dodaj -->
        <DockPanel DockPanel.Dock="Top" Margin="0,0,0,8">
            <Button DockPanel.Dock="Right" Content="Dodaj" Command="{Binding AddCommand}" Padding="12,4" Margin="8,0,0,0" />
            <TextBox Text="{Binding NewTask, UpdateSourceTrigger=PropertyChanged}" />
        </DockPanel>

        <!-- Dolny pasek: licznik + Usuń -->
        <DockPanel DockPanel.Dock="Bottom" Margin="0,8,0,0">
            <Button DockPanel.Dock="Right" Content="Usuń zaznaczone" Command="{Binding RemoveCommand}" Padding="12,4" />
            <TextBlock Text="{Binding Tasks.Count, StringFormat='Zadań: {0}'}" VerticalAlignment="Center" />
        </DockPanel>

        <!-- ItemsSource: skąd brać elementy; SelectedItem: dokąd zapisać zaznaczenie -->
        <ListBox ItemsSource="{Binding Tasks}"
                 SelectedItem="{Binding SelectedTask}" />
    </DockPanel>
</Window>
```

4. `MainWindow.xaml.cs`:

```csharp
// K06Collections/MainWindow.xaml.cs
using System.Windows;

namespace K06Collections;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        DataContext = new TodoViewModel();
    }
}
```

### Sprawdzamy

- Na starcie dwa zadania, **Dodaj** i **Usuń zaznaczone** szare.
- Po wpisaniu tekstu **Dodaj** się aktywuje; po kliknięciu zadanie trafia na listę, pole się czyści, licznik rośnie.
- Po kliknięciu elementu listy **Usuń zaznaczone** się aktywuje; po usunięciu zaznaczenie znika, licznik maleje.

### Dlaczego tak

- `NewTask = string.Empty` w `Add()` czyści `TextBox` **przez binding** – ViewModel nie zna kontrolki.
- `SelectedTask` jest `string?`: `null` znaczy „nic nie zaznaczono”. `CanRemove` sprawdza właśnie to.
- `[NotifyCanExecuteChangedFor]` na `NewTask` i `SelectedTask` – bez tego przyciski nie zmieniałyby stanu (jak w eksperymencie z kroku 4).
- `Tasks.Count` w `StringFormat`: `ObservableCollection` powiadamia także o zmianie `Count`, więc licznik odświeża się sam.

### Eksperyment

Zamieniamy typ `Tasks` na `List<string>` (i `= ["Kupić mleko", "Oddać książkę"]` zostaje). Projekt się kompiluje, ale po **Dodaj** lista się **nie zmienia** (element jest w kolekcji, `ListBox` o tym nie wie). Przywracamy `ObservableCollection`.

---

## Krok 7. K07AsyncCommand: komenda asynchroniczna i pasek postępu

**Cel:** uruchomić długą operację z komendy bez zamrażania okna; pokazać stan „pracuję”.

### Teoria

- Metoda z `[RelayCommand]`, która zwraca `Task`, dostaje komendę typu `IAsyncRelayCommand`. Taka komenda **sama** wyłącza przycisk na czas wykonywania (nie da się kliknąć dwa razy).
- `IsBusy` to zwykła właściwość `bool`. Łączymy ją z `Visibility` paska postępu przez wbudowany `BooleanToVisibilityConverter`, zarejestrowany jako zasób w `App.xaml`.
- `try/finally` wokół operacji gwarantuje, że `IsBusy` wróci na `false` także po wyjątku.

### Kroki

1. Projekt WPF `K07AsyncCommand` z pakietem `CommunityToolkit.Mvvm 8.4.2`.
2. `App.xaml` – dodajemy konwerter jako zasób:

```xml
<!-- K07AsyncCommand/App.xaml -->
<Application x:Class="K07AsyncCommand.App"
             xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
             xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
             StartupUri="MainWindow.xaml">
    <Application.Resources>
        <!-- Wbudowany konwerter WPF: true → Visible, false → Collapsed -->
        <BooleanToVisibilityConverter x:Key="BoolToVisibility" />
    </Application.Resources>
</Application>
```

3. `DownloadViewModel.cs`:

```csharp
// K07AsyncCommand/DownloadViewModel.cs
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace K07AsyncCommand;

/// <summary>
/// Komenda asynchroniczna: metoda zwraca Task, generator tworzy IAsyncRelayCommand.
/// Podczas wykonywania komenda sama blokuje przycisk, a IsBusy steruje paskiem postępu.
/// </summary>
public partial class DownloadViewModel : ObservableObject
{
    [ObservableProperty]
    public partial bool IsBusy { get; set; }

    [ObservableProperty]
    public partial string Result { get; set; } = "Jeszcze nic nie pobrano.";

    [ObservableProperty]
    public partial int Clicks { get; set; }

    // Końcówka "Async" jest obcinana: powstaje DownloadCommand
    [RelayCommand]
    private async Task DownloadAsync()
    {
        IsBusy = true;
        try
        {
            // Udajemy wolne zapytanie do internetu. Okno pozostaje responsywne,
            // bo await oddaje wątek UI na czas czekania.
            await Task.Delay(TimeSpan.FromSeconds(3));
            Result = $"Pobrano o {DateTime.Now:HH:mm:ss}";
        }
        finally
        {
            IsBusy = false;   // także po błędzie – pasek znika, przycisk wraca
        }
    }

    // Zwykła komenda do sprawdzenia, że okno żyje w trakcie pobierania
    [RelayCommand]
    private void Click() => Clicks++;
}
```

4. `MainWindow.xaml`:

```xml
<!-- K07AsyncCommand/MainWindow.xaml -->
<Window x:Class="K07AsyncCommand.MainWindow"
        xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
        xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
        Title="K07 – komenda asynchroniczna" Height="260" Width="380">
    <StackPanel Margin="16">
        <!-- Komenda async: przycisk jest nieaktywny, dopóki poprzednie pobieranie trwa -->
        <Button Content="Pobierz dane (3 s)" Command="{Binding DownloadCommand}" Padding="12,6" />

        <!-- Pasek widoczny tylko, gdy IsBusy = true -->
        <ProgressBar IsIndeterminate="True" Height="16" Margin="0,8"
                     Visibility="{Binding IsBusy, Converter={StaticResource BoolToVisibility}}" />

        <TextBlock Text="{Binding Result}" FontSize="16" Margin="0,8" />

        <Separator Margin="0,8" />

        <TextBlock Text="Test: klikaj w trakcie pobierania – okno nie jest zamrożone." Foreground="Gray" />
        <StackPanel Orientation="Horizontal" Margin="0,4">
            <Button Content="Klik" Command="{Binding ClickCommand}" Padding="12,4" />
            <TextBlock Text="{Binding Clicks, StringFormat='Kliknięć: {0}'}" Margin="12,0" VerticalAlignment="Center" />
        </StackPanel>
    </StackPanel>
</Window>
```

5. `MainWindow.xaml.cs`:

```csharp
// K07AsyncCommand/MainWindow.xaml.cs
using System.Windows;

namespace K07AsyncCommand;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        DataContext = new DownloadViewModel();
    }
}
```

### Sprawdzamy

- Po **Pobierz dane** przycisk gaśnie, pojawia się animowany pasek. Po 3 s pasek znika, pojawia się godzina, przycisk wraca.
- W trakcie pobierania **Klik** działa i licznik rośnie – okno nie jest zamrożone.

### Dlaczego tak

- `await Task.Delay(...)` oddaje wątek UI na czas czekania (to samo, co `await _httpClient.GetStringAsync(...)` w P03). Po `await` wracamy na wątek UI, więc `Result = …` jest bezpieczne.
- `DownloadAsync` → `DownloadCommand`: generator obcina końcówkę `Async`.
- W P03 w `finally` jest dokładnie to samo: `IsBusy = false`. A `ErrorMessage` ustawiamy w `catch`.

### Eksperyment

Zamieniamy `await Task.Delay(TimeSpan.FromSeconds(3));` na `Thread.Sleep(3000);` (i możemy usunąć `async`/`await`, zostawiając `return Task.CompletedTask;`). Okno zamarza na 3 s, **Klik** nie reaguje, pasek nawet się nie pokazuje (wątek UI nie miał kiedy go narysować). To jest kod synchroniczny z dnia 11. Przywracamy.

---

## Krok 8. K08DiManual: Dependency Injection bez żadnej biblioteki

**Cel:** zrozumieć samą zasadę DI na programie konsolowym: zależność przez interfejs, dostarczana konstruktorem.

### Teoria

- **Zależność** to obiekt, bez którego klasa nie zadziała (zegar, serwis HTTP, baza).
- Klasa, która sama robi `new SystemClock()`, jest **przyklejona** do tej implementacji: nie da się jej przetestować z inną godziną ani podmienić zegara.
- **Wstrzykiwanie** = klasa deklaruje w konstruktorze, czego potrzebuje (`IClock`), a ktoś z zewnątrz to dostarcza. Miejsce, gdzie składamy obiekty, nazywa się **composition root** (tu: `Program.cs`).

### Kroki

1. Tworzymy projekt konsolowy: `dotnet new console -n K08DiManual -o K08DiManual -f net10.0` (albo **Console App** w Visual Studio). Bez pakietów.
2. `IClock.cs` – interfejs i dwie implementacje w jednym pliku (dla krótkości):

```csharp
// K08DiManual/IClock.cs
namespace K08DiManual;

/// <summary>
/// Zależność (dependency) opisana interfejsem: "coś, co zna aktualny czas".
/// Greeter nie wie i nie musi wiedzieć, skąd ten czas pochodzi.
/// </summary>
public interface IClock
{
    DateTime Now { get; }
}

/// <summary>Prawdziwy zegar – czas z systemu.</summary>
public class SystemClock : IClock
{
    public DateTime Now => DateTime.Now;
}

/// <summary>Zegar "udawany" – zawsze ta sama godzina. Do testów i pokazów.</summary>
public class FixedClock : IClock
{
    private readonly DateTime _fixedTime;

    public FixedClock(DateTime fixedTime)
    {
        _fixedTime = fixedTime;
    }

    public DateTime Now => _fixedTime;
}
```

3. `Greeter.cs`:

```csharp
// K08DiManual/Greeter.cs
namespace K08DiManual;

/// <summary>
/// Klasa, która POTRZEBUJE zegara, ale go nie tworzy – dostaje go konstruktorem.
/// To jest wstrzykiwanie zależności (Dependency Injection) w najprostszej, ręcznej postaci.
/// </summary>
public class Greeter
{
    private readonly IClock _clock;

    // Zależność wchodzi konstruktorem. Bez zegara nie da się utworzyć Greetera.
    public Greeter(IClock clock)
    {
        _clock = clock;
    }

    public string Greet(string name)
    {
        int hour = _clock.Now.Hour;

        string timeOfDay = hour switch
        {
            < 6 => "Dobrej nocy",
            < 12 => "Dzień dobry",
            < 18 => "Miłego popołudnia",
            _ => "Dobry wieczór"
        };

        return $"{timeOfDay}, {name}! (godzina {hour})";
    }
}
```

4. `Program.cs`:

```csharp
// K08DiManual/Program.cs
// Dependency Injection "ręcznie": obiekty składamy w jednym miejscu (Program.cs),
// a klasy dostają swoje zależności konstruktorem.
using K08DiManual;

Console.OutputEncoding = System.Text.Encoding.UTF8;

// 1. Prawdziwy zegar – powitanie zależy od aktualnej godziny
IClock systemClock = new SystemClock();
var greeter = new Greeter(systemClock);
Console.WriteLine(greeter.Greet("Ola"));

// 2. Ten sam Greeter, inny zegar – bez zmiany ani jednej linii w Greeter
IClock morningClock = new FixedClock(new DateTime(2026, 1, 1, 8, 0, 0));
var morningGreeter = new Greeter(morningClock);
Console.WriteLine(morningGreeter.Greet("Ola"));

IClock eveningClock = new FixedClock(new DateTime(2026, 1, 1, 21, 0, 0));
var eveningGreeter = new Greeter(eveningClock);
Console.WriteLine(eveningGreeter.Greet("Ola"));

// Gdyby Greeter miał w środku "new SystemClock()", punkty 2 i 3 byłyby niemożliwe:
// nie dałoby się sprawdzić powitania wieczornego inaczej niż czekając do wieczora.
```

### Sprawdzamy

`dotnet run --project K08DiManual` wypisuje trzy linie: pierwsza zależy od aktualnej godziny, druga to zawsze „Dzień dobry… (godzina 8)”, trzecia „Dobry wieczór… (godzina 21)”.

### Dlaczego tak

- `Greeter` **nie wie**, czy dostał prawdziwy zegar, czy udawany. Nie musi. Dzięki temu da się go sprawdzić o każdej porze dnia – to jest test jednostkowy w najprostszej formie.
- W P03 tę samą rolę pełni `IMeteoService` z implementacjami `OpenMeteoService` (internet) i `FakeMeteoService` (bez internetu). `MainViewModel` dostaje `IMeteoService` konstruktorem i nie wie, która to wersja.
- Wada ręcznego DI: przy 20 klasach `Program.cs` to 20 linii `new` w odpowiedniej kolejności, a zmiana jednego konstruktora psuje kilka miejsc. Stąd krok 9.

### Eksperyment

Przenosimy tworzenie zegara do środka: w `Greeter` usuwamy parametr konstruktora i piszemy `private readonly IClock _clock = new SystemClock();`. `Program.cs` przestaje się kompilować (konstruktor nie przyjmuje argumentu), a po usunięciu argumentów wszystkie trzy linie pokazują tę samą godzinę – wieczornego powitania nie da się już sprawdzić. Przywracamy.

---

## Krok 9. K09DiContainer: kontener DI i cykle życia

**Cel:** zastąpić ręczne `new` rejestracją w kontenerze; zobaczyć różnicę między Singleton a Transient.

### Teoria

- **Kontener** (`ServiceProvider`) dostaje listę rejestracji: „gdy ktoś poprosi o `IClock`, utwórz `SystemClock`”. Gdy prosimy o `Greeter`, kontener patrzy na jego konstruktor, widzi `IClock`, tworzy go z rejestracji i przekazuje. Działa dla dowolnie długich łańcuchów.
- **Cykl życia** mówi, jak długo żyje obiekt: `AddSingleton` – jeden na cały program; `AddTransient` – nowy przy każdym żądaniu; `AddScoped` – jeden na „zakres” (w ASP.NET Core: na jedno żądanie HTTP; w WPF rzadko używany).
- Pakiet `Microsoft.Extensions.DependencyInjection` to ten sam kontener, którego używa ASP.NET Core, Generic Host, MAUI i Blazor.

### Kroki

1. Projekt konsolowy `K09DiContainer`, pakiet: `dotnet add K09DiContainer package Microsoft.Extensions.DependencyInjection --version 10.0.12`.

```xml
<!-- K09DiContainer/K09DiContainer.csproj -->
<Project Sdk="Microsoft.NET.Sdk">

  <PropertyGroup>
    <OutputType>Exe</OutputType>
    <TargetFramework>net10.0</TargetFramework>
    <Nullable>enable</Nullable>
    <ImplicitUsings>enable</ImplicitUsings>
  </PropertyGroup>

  <ItemGroup>
    <!-- Kontener DI Microsoftu (ten sam, którego używa ASP.NET Core i Generic Host) -->
    <PackageReference Include="Microsoft.Extensions.DependencyInjection" Version="10.0.12" />
  </ItemGroup>

</Project>
```

2. `IClock.cs` i `Greeter.cs` – te same, co w K08 (tylko `namespace K09DiContainer;`).
3. `Counter.cs`:

```csharp
// K09DiContainer/Counter.cs
namespace K09DiContainer;

/// <summary>
/// Klasa do pokazania cykli życia: każdy obiekt dostaje przy utworzeniu własny Id.
/// Jeśli dwa razy dostajemy ten sam Id, kontener zwrócił ten sam obiekt.
/// </summary>
public class Counter
{
    public Guid Id { get; } = Guid.NewGuid();

    public int Value { get; set; }
}

/// <summary>Druga klasa licznika tylko po to, żeby zarejestrować ją jako Transient.</summary>
public class TransientCounter : Counter
{
}
```

4. `Program.cs`:

```csharp
// K09DiContainer/Program.cs
// Dependency Injection z kontenerem: zamiast ręcznie pisać "new", rejestrujemy
// typy w ServiceCollection, a kontener sam składa obiekty wraz z ich zależnościami.
using K09DiContainer;
using Microsoft.Extensions.DependencyInjection;

Console.OutputEncoding = System.Text.Encoding.UTF8;

// 1. Rejestracja: "gdy ktoś poprosi o IClock, daj SystemClock"
var services = new ServiceCollection();
services.AddSingleton<IClock, SystemClock>();   // jeden zegar na całą aplikację
services.AddTransient<Greeter>();               // nowy Greeter przy każdym żądaniu

// Cykle życia do porównania:
services.AddSingleton<Counter>();                       // ten sam obiekt zawsze
services.AddTransient<TransientCounter>();              // nowy obiekt za każdym razem

// 2. Budujemy kontener
ServiceProvider provider = services.BuildServiceProvider();

// 3. Prosimy o Greeter. Kontener widzi, że konstruktor wymaga IClock,
//    znajduje rejestrację IClock → SystemClock i sam go tworzy i przekazuje.
Greeter greeter = provider.GetRequiredService<Greeter>();
Console.WriteLine(greeter.Greet("Ola"));

// 4. Singleton: dwa żądania, ten sam obiekt (ten sam Id)
Counter s1 = provider.GetRequiredService<Counter>();
Counter s2 = provider.GetRequiredService<Counter>();
s1.Value = 10;
Console.WriteLine($"Singleton: {s1.Id} / {s2.Id}  → ten sam obiekt: {ReferenceEquals(s1, s2)}, s2.Value = {s2.Value}");

// 5. Transient: dwa żądania, dwa różne obiekty (różne Id)
TransientCounter t1 = provider.GetRequiredService<TransientCounter>();
TransientCounter t2 = provider.GetRequiredService<TransientCounter>();
t1.Value = 10;
Console.WriteLine($"Transient: {t1.Id} / {t2.Id}  → ten sam obiekt: {ReferenceEquals(t1, t2)}, t2.Value = {t2.Value}");

// 6. Podmiana implementacji = jedna linia w rejestracji. Odkomentuj i uruchom ponownie:
// services.AddSingleton<IClock>(new FixedClock(new DateTime(2026, 1, 1, 21, 0, 0)));
// (musi być PRZED BuildServiceProvider; przy dwóch rejestracjach IClock wygrywa ostatnia)

// 7. Eksperyment: żądanie typu, którego nie zarejestrowano, kończy się wyjątkiem
//    "No service for type ... has been registered". Odkomentuj:
// provider.GetRequiredService<FixedClock>();
```

### Sprawdzamy

Wyjście (identyfikatory będą inne):

```
Dzień dobry, Ola! (godzina 10)
Singleton: 8a0d35ff-… / 8a0d35ff-…  → ten sam obiekt: True, s2.Value = 10
Transient: 341a4f6d-… / cd31b673-…  → ten sam obiekt: False, t2.Value = 0
```

- Singleton: dwa żądania, **ten sam** `Id`, a `Value` ustawione przez `s1` widać w `s2`.
- Transient: dwa **różne** `Id`, `t2.Value` jest 0.

### Dlaczego tak

- Nigdzie nie piszemy `new Greeter(...)` ani `new SystemClock()` – robi to kontener. Gdy do konstruktora `Greeter` dojdzie drugi parametr, zmienia się jeden plik, a nie każde miejsce tworzenia.
- `GetRequiredService<T>` rzuca wyjątek, gdy `T` nie jest zarejestrowane (eksperyment 7 w kodzie). `GetService<T>` zwróciłby `null`. W aplikacji wolimy wyjątek przy starcie niż `null` w środku działania.
- W P03: `HttpClient` i serwis są Singletonami (jeden na aplikację – tak jak w dniu 11 `static HttpClient`), `MainWindow` jest Transient (nowe okno przy każdym żądaniu).

### Eksperyment

Odkomentowujemy punkt 6 (rejestracja `FixedClock`) i uruchamiamy: pierwsza linia zmienia się na „Dobry wieczór… (godzina 21)”, choć `Greeter` ani `Program` poza jedną linią nie zmieniły się. Potem odkomentowujemy punkt 7 i czytamy komunikat wyjątku – tak wygląda najczęstszy błąd DI: „zapomniałem zarejestrować”.

---

## Krok 10. K10DiWpf: MVVM + Generic Host w jednym oknie

**Cel:** złożyć wszystko: okno z bindingiem i komendą, ViewModel z zależnościami, kontener DI z konfiguracją z `appsettings.json`. To jest `App.xaml.cs` z P03 w wersji minimalnej.

### Teoria

- **Generic Host** (`Host.CreateApplicationBuilder()`) to gotowy zestaw: kontener DI + konfiguracja (`appsettings.json`, zmienne środowiskowe, User Secrets) + logowanie. W ASP.NET Core (dzień 13) `WebApplication.CreateBuilder()` robi to samo plus serwer HTTP.
- `builder.Services.Configure<GreetingOptions>(sekcja)` wiąże sekcję JSON z klasą. Klasa, która jej potrzebuje, prosi w konstruktorze o `IOptions<GreetingOptions>` i czyta `.Value`.
- W WPF usuwamy `StartupUri` z `App.xaml` i tworzymy okno **z kontenera** w `OnStartup`. Inaczej WPF zrobiłby `new MainWindow()` bez argumentów, a nasz konstruktor wymaga ViewModelu.

### Kroki

1. Projekt WPF `K10DiWpf` z dwoma pakietami (`CommunityToolkit.Mvvm 8.4.2`, `Microsoft.Extensions.Hosting 10.0.12`) i plikiem `appsettings.json` kopiowanym do folderu wyjściowego:

```xml
<!-- K10DiWpf/K10DiWpf.csproj -->
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
    <!-- Generic Host: kontener DI + konfiguracja (appsettings.json) + IOptions -->
    <PackageReference Include="Microsoft.Extensions.Hosting" Version="10.0.12" />
  </ItemGroup>

  <ItemGroup>
    <!-- Plik konfiguracyjny kopiowany obok .exe -->
    <None Update="appsettings.json">
      <CopyToOutputDirectory>PreserveNewest</CopyToOutputDirectory>
    </None>
  </ItemGroup>

</Project>
```

2. Nowy plik `appsettings.json` w głównym folderze projektu (**Add → New Item → JSON File**; po dodaniu zaznaczamy go → **Properties → Copy to Output Directory: Copy if newer** – to właśnie wpis `PreserveNewest` w `.csproj`):

**Plik:** `K10DiWpf/appsettings.json`

```json
{
  "Greeting": {
    "UserName": "Ola",
    "Language": "pl"
  }
}
```

3. `GreetingOptions.cs` – klasa odpowiadająca sekcji JSON:

```csharp
// K10DiWpf/GreetingOptions.cs
namespace K10DiWpf;

/// <summary>
/// Klasa odwzorowująca sekcję "Greeting" z appsettings.json.
/// Nazwy właściwości = nazwy kluczy w JSON.
/// </summary>
public class GreetingOptions
{
    public const string SectionName = "Greeting";

    public string UserName { get; set; } = "Gościu";

    public string Language { get; set; } = "pl";
}
```

4. Folder `Services`, plik `IClock.cs`:

```csharp
// K10DiWpf/Services/IClock.cs
namespace K10DiWpf.Services;

public interface IClock
{
    DateTime Now { get; }
}

public class SystemClock : IClock
{
    public DateTime Now => DateTime.Now;
}

/// <summary>Zegar stały – do pokazania podmiany implementacji w App.xaml.cs.</summary>
public class FixedClock : IClock
{
    public DateTime Now => new(2026, 1, 1, 21, 0, 0);
}
```

5. Folder `ViewModels`, plik `MainViewModel.cs`:

```csharp
// K10DiWpf/ViewModels/MainViewModel.cs
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using K10DiWpf.Services;
using Microsoft.Extensions.Options;

namespace K10DiWpf.ViewModels;

/// <summary>
/// ViewModel dostaje swoje zależności konstruktorem: zegar (interfejs)
/// i ustawienia z appsettings.json (IOptions). Nie tworzy ich sam – robi to kontener.
/// </summary>
public partial class MainViewModel : ObservableObject
{
    private readonly IClock _clock;
    private readonly GreetingOptions _options;

    public MainViewModel(IClock clock, IOptions<GreetingOptions> options)
    {
        _clock = clock;
        _options = options.Value;   // IOptions<T>.Value to gotowy obiekt z wartościami z JSON

        Refresh();
    }

    [ObservableProperty]
    public partial string Message { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string ClockInfo { get; set; } = string.Empty;

    [RelayCommand]
    private void Refresh()
    {
        DateTime now = _clock.Now;

        string timeOfDay = _options.Language == "en"
            ? (now.Hour < 12 ? "Good morning" : "Good evening")
            : (now.Hour < 12 ? "Dzień dobry" : "Dobry wieczór");

        Message = $"{timeOfDay}, {_options.UserName}!";
        ClockInfo = $"Zegar: {_clock.GetType().Name}, czas: {now:HH:mm:ss}";
    }
}
```

6. `MainWindow.xaml`:

```xml
<!-- K10DiWpf/MainWindow.xaml -->
<Window x:Class="K10DiWpf.MainWindow"
        xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
        xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
        Title="K10 – MVVM + DI (Generic Host)" Height="220" Width="420">
    <StackPanel Margin="16">
        <TextBlock Text="{Binding Message}" FontSize="24" FontWeight="Bold" />
        <TextBlock Text="{Binding ClockInfo}" Foreground="Gray" Margin="0,8" />
        <Button Content="Odśwież" Command="{Binding RefreshCommand}" Padding="12,4" HorizontalAlignment="Left" />
    </StackPanel>
</Window>
```

7. `MainWindow.xaml.cs` – okno przyjmuje ViewModel konstruktorem:

```csharp
// K10DiWpf/MainWindow.xaml.cs
using System.Windows;
using K10DiWpf.ViewModels;

namespace K10DiWpf;

/// <summary>
/// Okno dostaje ViewModel konstruktorem. Nikt nie pisze "new MainViewModel()" –
/// robi to kontener, bo tylko on wie, skąd wziąć IClock i IOptions.
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

8. `App.xaml` – **bez** `StartupUri`, za to `Startup` i `Exit`:

```xml
<!-- K10DiWpf/App.xaml -->
<Application x:Class="K10DiWpf.App"
             xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
             xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
             Startup="OnStartup"
             Exit="OnExit">
    <!-- Brak StartupUri: okno tworzy kontener DI w App.xaml.cs -->
    <Application.Resources>
    </Application.Resources>
</Application>
```

9. `App.xaml.cs` – Generic Host:

```csharp
// K10DiWpf/App.xaml.cs
using System.Windows;
using K10DiWpf.Services;
using K10DiWpf.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace K10DiWpf;

/// <summary>
/// Generic Host = kontener DI + konfiguracja (appsettings.json) + logowanie, w jednym.
/// Dokładnie ten sam mechanizm działa w ASP.NET Core (dzień 13).
/// </summary>
public partial class App : Application
{
    private readonly IHost _host;

    public App()
    {
        HostApplicationBuilder builder = Host.CreateApplicationBuilder();

        // Konfiguracja: sekcja "Greeting" z appsettings.json → IOptions<GreetingOptions>
        builder.Services.Configure<GreetingOptions>(
            builder.Configuration.GetSection(GreetingOptions.SectionName));

        // Serwisy
        builder.Services.AddSingleton<IClock, SystemClock>();
        // Podmiana implementacji – jedna linia (zakomentuj SystemClock, odkomentuj FixedClock):
        // builder.Services.AddSingleton<IClock, FixedClock>();

        // ViewModel i okno
        builder.Services.AddSingleton<MainViewModel>();
        builder.Services.AddTransient<MainWindow>();

        _host = builder.Build();
    }

    private void OnStartup(object sender, StartupEventArgs e)
    {
        _host.Start();

        // Kontener tworzy MainWindow → MainViewModel → IClock + IOptions<GreetingOptions>
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

### Sprawdzamy

- Okno pokazuje „Dzień dobry, Ola!” albo „Dobry wieczór, Ola!” (zależnie od godziny) i linię „Zegar: SystemClock, czas: …”. **Odśwież** aktualizuje czas.
- Zmieniamy w `appsettings.json` `"UserName": "Ola"` na własne imię i `"Language"` na `"en"` → po ponownym uruchomieniu „Good morning, …!”. Można to zrobić także w pliku w `bin/Debug/net10.0-windows/` bez kompilacji.
- W `App.xaml.cs` zamieniamy rejestrację na `FixedClock` → „Dobry wieczór, Ola!” i „Zegar: FixedClock, czas: 21:00:00”. Ani ViewModel, ani okno się nie zmieniły.

### Dlaczego tak

Łańcuch, który składa kontener po `GetRequiredService<MainWindow>()`:

```
MainWindow(MainViewModel)
└── MainViewModel(IClock, IOptions<GreetingOptions>)
    ├── IClock → SystemClock               (AddSingleton)
    └── IOptions<GreetingOptions>          (Configure<…> z sekcji "Greeting")
```

- Gdybyśmy zostawili `StartupUri="MainWindow.xaml"`, WPF próbowałby wywołać `new MainWindow()` i zgłosił błąd o braku konstruktora bez parametrów. Dlatego okno tworzymy sami w `OnStartup`.
- `MainViewModel` jako Singleton: stan okna (tekst, lista) przeżywa zamknięcie i ponowne otwarcie okna. `MainWindow` jako Transient: każde `GetRequiredService<MainWindow>()` to nowe okno.
- `IOptions<T>` zamiast `IConfiguration` i tekstowych kluczy `"Greeting:UserName"`: literówka w nazwie właściwości to błąd kompilacji, literówka w kluczu tekstowym to `null` w trakcie działania.
- Dokładnie ten sam `App.xaml.cs` (z `AddHttpClient<IMeteoService, OpenMeteoService>` zamiast `AddSingleton<IClock, SystemClock>`) jest w P03. Po tym kroku jego czytanie nie powinno już sprawiać trudności.

### Eksperyment

Usuwamy linię `builder.Services.AddSingleton<IClock, SystemClock>();`. Projekt się kompiluje, ale przy starcie dostajemy `InvalidOperationException: Unable to resolve service for type 'K10DiWpf.Services.IClock' while attempting to activate 'K10DiWpf.ViewModels.MainViewModel'`. Komunikat mówi dokładnie: **czego** brakuje i **komu**. Przywracamy.

---

## Podsumowanie: od K01 do P03

| Mechanizm | Mały krok | W P03 |
|---|---|---|
| `DataContext` + `{Binding}` | K01 | `DataContext = viewModel` w `MainWindow.xaml.cs` |
| `INotifyPropertyChanged` | K02 (ręcznie), K03 (toolkit) | `MainViewModel : ObservableObject`, `[ObservableProperty]` |
| komendy, `CanExecute` | K04 | `LoadCitiesCommand`, `CanLoadCities` |
| `TextBox` w dwie strony, `UpdateSourceTrigger` | K05 | pole „Miasto” |
| `ObservableCollection`, `SelectedItem` | K06 | `Cities`, `SelectedCity`, `OnSelectedCityChanged` |
| komenda `async`, `IsBusy` | K07 | `LoadCitiesAsync`, `ProgressBar` |
| interfejs + konstruktor | K08 | `IMeteoService` w `MainViewModel` |
| kontener, Singleton/Transient | K09 | rejestracje w `ConfigureServices` |
| Generic Host, `IOptions`, okno z kontenera | K10 | `App.xaml.cs`, `AppSettings` |

Jeśli któryś krok nie jest jasny, warto wrócić do jego **eksperymentu**: zepsuć, zobaczyć objaw, naprawić. Objawy z eksperymentów (liczba stoi w miejscu, szary przycisk, lista się nie odświeża, `Unable to resolve service`) to dokładnie te błędy, które pojawiają się potem w P03 i w zadaniu Z01.

## Zadania do samodzielnego wykonania

1. **K04:** dodajemy właściwość `Step` (krok zmiany) z `TextBox` w oknie; **+1**/**-1** zmieniają `Value` o `Step`. Podpowiedź: `UpdateSourceTrigger=PropertyChanged` z kroku 5.
2. **K06:** zadania jako klasa `TodoItem { string Title; bool IsDone; }` zamiast `string`; w `ListBox` `CheckBox` zbindowany do `IsDone` (potrzebny `ItemTemplate` z `DataTemplate`).
3. **K07:** dodajemy przycisk **Anuluj** – `[RelayCommand(IncludeCancelCommand = true)]` na metodzie z parametrem `CancellationToken`, przekazanym do `Task.Delay`.
4. **K10:** dodajemy serwis `IGreetingService` z metodą `Greet(string name, DateTime now)` i przenosimy do niego logikę „Dzień dobry/Dobry wieczór” z ViewModelu; rejestrujemy go w kontenerze; ViewModel dostaje go konstruktorem.
