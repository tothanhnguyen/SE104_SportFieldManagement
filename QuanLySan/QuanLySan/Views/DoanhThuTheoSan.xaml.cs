using System.Windows;
using System.Windows.Input; // Đảm bảo có thư viện này cho sự kiện Mouse
using QuanLySan.ViewModels;

namespace QuanLySan.Views
{
    public partial class DoanhThuTheoSan : Window
    {
        public DoanhThuTheoSan()
        {
            InitializeComponent();
            DataContext = new DoanhThuTheoSanViewModel();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        // Hàm này cho phép kéo thả cửa sổ khi nhấn giữ chuột trái trên thanh tiêu đề
        private void Border_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                DragMove();
            }
        }
    }
}