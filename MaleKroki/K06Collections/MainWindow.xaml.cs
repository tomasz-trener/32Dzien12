// K06Collections/MainWindow.xaml.cs
using System.Windows;

namespace K06Collections;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        DataContext = new TodoViewModel();
    }
}
