// K05TwoWayBinding/GreetingViewModel.cs
using CommunityToolkit.Mvvm.ComponentModel;

namespace K05TwoWayBinding;

/// <summary>
/// Binding w dwie strony: TextBox zapisuje do Name, a Name i Greeting wracają do okna.
/// </summary>
public partial class GreetingViewModel : ObservableObject
{
    // Gdy zmieni się Name, okno ma też odświeżyć Greeting (właściwość wyliczaną)
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(Greeting))]
    public partial string Name { get; set; } = string.Empty;

    // Właściwość tylko do odczytu, liczona z Name. Nie ma własnego pola ani settera.
    public string Greeting =>
        string.IsNullOrWhiteSpace(Name) ? "Wpisz swoje imię." : $"Cześć, {Name.Trim()}!";
}
