using System.Windows;
using QuanLySan.ViewModels;
using FontAwesome.Sharp;

namespace QuanLySan.Views
{
    // Menu chính. Logic điều hướng nằm ở MainViewModel + NavigationService (đúng MVVM).
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            
            // Đặt icon cho cửa sổ và taskbar
            var iconBrush = new System.Windows.Media.SolidColorBrush((System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#3B82F6")); // AccentBlue
            this.Icon = FontAwesome.Sharp.IconChar.Futbol.ToImageSource(iconBrush, 256);

            DataContext = new MainViewModel();
        }
    }
}
