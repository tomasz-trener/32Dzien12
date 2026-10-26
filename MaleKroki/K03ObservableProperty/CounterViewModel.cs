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
