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
