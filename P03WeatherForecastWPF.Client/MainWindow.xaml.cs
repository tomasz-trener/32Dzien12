using System.Windows;
using P03WeatherForecastWPF.Client.ViewModels;

namespace P03WeatherForecastWPF.Client;

/// <summary>
/// Code-behind okna w MVVM jest prawie pusty: jedyne zadanie to
/// podpięcie ViewModelu jako DataContext. Cała logika jest w MainViewModel.
/// ViewModel wchodzi konstruktorem – dostarcza go kontener DI (App.xaml.cs).
/// </summary>
public partial class MainWindow : Window
{
    public MainWindow(MainViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }
}
