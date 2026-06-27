using System.Windows;
using QuanLySan.ViewModels;

namespace QuanLySan.Views
{
    // Menu chính. Logic điều hướng nằm ở MainViewModel + NavigationService (đúng MVVM).
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            DataContext = new MainViewModel();
        }
    }
}
