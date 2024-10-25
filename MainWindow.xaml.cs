using System.Windows;
using IdCode.ViewModels;

namespace IdCode;

/// <summary>
///     Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window {
    private readonly UpcACodeViewModel _upcACodeViewModel;

    public MainWindow() {
        InitializeComponent();
        _upcACodeViewModel = new UpcACodeViewModel();
        MainGrid.DataContext = _upcACodeViewModel;
    }
}