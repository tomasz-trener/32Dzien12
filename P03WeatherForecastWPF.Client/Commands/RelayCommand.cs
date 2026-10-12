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
