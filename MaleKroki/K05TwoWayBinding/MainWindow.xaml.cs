// K05TwoWayBinding/MainWindow.xaml.cs
using System.Windows;

namespace K05TwoWayBinding;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        DataContext = new GreetingViewModel();
    }
}
