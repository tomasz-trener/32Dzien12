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
