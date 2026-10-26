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
