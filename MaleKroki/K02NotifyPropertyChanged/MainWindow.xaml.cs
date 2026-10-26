// K02NotifyPropertyChanged/MainWindow.xaml.cs
using System.Windows;

namespace K02NotifyPropertyChanged;

public partial class MainWindow : Window
{
    private readonly Counter _counter = new();

    public MainWindow()
    {
        InitializeComponent();
        DataContext = _counter;
    }

    private void Increment_Click(object sender, RoutedEventArgs e)
    {
        // Zmieniamy tylko obiekt. TextBlock odświeży się sam dzięki PropertyChanged.
        _counter.Value++;
    }
}
