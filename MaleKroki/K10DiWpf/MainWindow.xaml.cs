// K10DiWpf/MainWindow.xaml.cs
using System.Windows;
using K10DiWpf.ViewModels;

namespace K10DiWpf;

/// <summary>
/// Okno dostaje ViewModel konstruktorem. Nikt nie pisze "new MainViewModel()" –
/// robi to kontener, bo tylko on wie, skąd wziąć IClock i IOptions.
/// </summary>
public partial class MainWindow : Window
{
    public MainWindow(MainViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }
}
