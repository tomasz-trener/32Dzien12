// K02NotifyPropertyChanged/Counter.cs
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace K02NotifyPropertyChanged;

/// <summary>
/// Ta sama klasa, ale z INotifyPropertyChanged: po każdej zmianie Value
/// podnosi zdarzenie PropertyChanged, na które nasłuchuje binding w XAML.
/// </summary>
public class Counter : INotifyPropertyChanged
{
    private int _value;

    public int Value
    {
        get => _value;
        set
        {
            if (_value == value)
            {
                return;   // bez zmiany – nie powiadamiamy
            }

            _value = value;
            OnPropertyChanged();   // "Value się zmieniło" – binding odświeży TextBlock
        }
    }

    // Zdarzenie wymagane przez interfejs. WPF podpina się do niego przy tworzeniu bindingu.
    public event PropertyChangedEventHandler? PropertyChanged;

    // [CallerMemberName]: kompilator sam wstawia nazwę właściwości, z której wołamy metodę
    protected void OnPropertyChanged([CallerMemberName] string? propertyName = null) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}
