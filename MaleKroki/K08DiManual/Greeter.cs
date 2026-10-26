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
