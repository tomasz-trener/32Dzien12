using System.Windows;
using Z01AplikacjaNBP.Client.ViewModels;

namespace Z01AplikacjaNBP.Client;

public partial class MainWindow : Window
{
    public MainWindow(MainViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }
}
