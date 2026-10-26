// K07AsyncCommand/MainWindow.xaml.cs
using System.Windows;

namespace K07AsyncCommand;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        DataContext = new DownloadViewModel();
    }
}
