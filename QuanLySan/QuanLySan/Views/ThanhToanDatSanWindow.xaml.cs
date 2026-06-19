using System;
using System.Windows;
using System.Windows.Input;

namespace QuanLySan.Views
{
    public partial class ThanhToanDatSanWindow : Window
    {
        public ThanhToanDatSanWindow()
        {
            InitializeComponent();
            // Liên kết dữ liệu với ViewModel
            this.DataContext = new QuanLySan.ViewModels.ThanhToanDatSanViewModel();
        }

        // Di chuyển cửa sổ khi nhấn giữ Header
        private void Header_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed) DragMove();
        }

        // Đóng cửa sổ
        private void BtnThoat_Click(object sender, RoutedEventArgs e) => this.Close();
    }
}
