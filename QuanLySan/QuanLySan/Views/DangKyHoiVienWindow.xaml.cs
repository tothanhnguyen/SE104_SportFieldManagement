using System.Windows;
using System.Windows.Input;

namespace QuanLySan.Views
{
    public partial class DangKyHoiVienWindow : Window
    {
        public DangKyHoiVienWindow()
        {
            InitializeComponent();
        }

        // Kéo thả cửa sổ khi click vào Header
        private void Header_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                DragMove();
            }
        }

        // Tắt cửa sổ
        private void BtnThoat_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
