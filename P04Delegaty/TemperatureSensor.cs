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
