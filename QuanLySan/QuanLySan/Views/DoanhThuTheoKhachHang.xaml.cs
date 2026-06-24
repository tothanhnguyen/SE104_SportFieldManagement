using System.Windows;
using System.Windows.Input; // Quan trọng: Thêm thư viện này để sử dụng MouseButtonEventArgs

namespace QuanLyKhachHang.Views
{
    public partial class DoanhThuTheoKhachHang : Window
    {
        public DoanhThuTheoKhachHang()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        // Xử lý sự kiện kéo thả
        private void Border_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                DragMove();
            }
        }
    }
}