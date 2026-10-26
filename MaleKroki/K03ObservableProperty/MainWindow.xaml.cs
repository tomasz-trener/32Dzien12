// K03ObservableProperty/MainWindow.xaml.cs
using System.Windows;

namespace K03ObservableProperty;

public partial class MainWindow : Window
{
    private readonly CounterViewModel _viewModel = new();

    public MainWindow()
    {
        InitializeComponent();
        DataContext = _viewModel;
    }

    private void Increment_Click(object sender, RoutedEventArgs e)
    {
        _viewModel.Value++;
    }
}
