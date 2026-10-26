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
