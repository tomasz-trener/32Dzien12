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
