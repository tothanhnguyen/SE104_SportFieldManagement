using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using QuanLySan.Models;
using QuanLySan.ViewModels;

namespace QuanLySan.Views
{
    public partial class DatSanWindow : Window
    {
        public DatSanWindow()
        {
            InitializeComponent();
            // Liên kết dữ liệu với ViewModel
            this.DataContext = new DatSanViewModel();
        }

        // Di chuyển cửa sổ khi nhấn giữ Header
        private void Header_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed) DragMove();
        }

        // Đóng cửa sổ
        private void BtnThoat_Click(object sender, RoutedEventArgs e) => this.Close();

        // Chuột phải → Xóa dòng trong DataGrid
        private void MenuXoaDong_Click(object sender, RoutedEventArgs e)
        {
            if (dgGioDat.SelectedItem is GioSanItem item)
            {
                var vm = this.DataContext as DatSanViewModel;
                if (vm != null && vm.XoaGioCommand.CanExecute(item))
                {
                    vm.XoaGioCommand.Execute(item);
                }
            }
            else
            {
                MessageBox.Show("Vui lòng chọn một dòng để xóa!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }
    }
}
