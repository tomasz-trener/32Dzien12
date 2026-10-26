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
