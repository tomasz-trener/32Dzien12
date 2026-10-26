// K01Binding/MainWindow.xaml.cs
using System.Windows;

namespace K01Binding;

public partial class MainWindow : Window
{
    private readonly Counter _counter = new();

    public MainWindow()
    {
        InitializeComponent();

        // DataContext: obiekt, z którego bindingi w XAML biorą dane
        DataContext = _counter;
    }

    private void Increment_Click(object sender, RoutedEventArgs e)
    {
        _counter.Value++;

        // Tytuł okna ustawiamy ręcznie – będzie się zmieniał.
        // TextBlock z bindingiem NIE zmieni się, bo Counter nie powiadamia o zmianie.
        Title = $"Value = {_counter.Value}";
    }
}
