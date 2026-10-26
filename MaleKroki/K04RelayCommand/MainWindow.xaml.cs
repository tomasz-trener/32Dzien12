// K04RelayCommand/MainWindow.xaml.cs
using System.Windows;

namespace K04RelayCommand;

/// <summary>
/// Code-behind w MVVM: jedna linia – podpięcie ViewModelu. Żadnej logiki.
/// </summary>
public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        DataContext = new CounterViewModel();
    }
}
