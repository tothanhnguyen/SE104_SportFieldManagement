using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace QuanLySan.Views
{
    public partial class DatSanWindow : Window
    {
        public DatSanWindow()
        {
            InitializeComponent();
            // Liên kết dữ liệu với ViewModel
            this.DataContext = new QuanLySan.ViewModels.DatSanViewModel();
        }

        // Di chuyển cửa sổ khi nhấn giữ Header
        private void Header_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed) DragMove();
        }

        // Đóng cửa sổ
        private void BtnThoat_Click(object sender, RoutedEventArgs e) => this.Close();

        // Validate định dạng giờ khi rời ô edit trong DataGrid
        private void DgGioDat_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
        {
            if (e.EditAction == DataGridEditAction.Cancel) return;

            if (e.EditingElement is TextBox tb)
            {
                string header = (e.Column.Header ?? "").ToString()!;
                string value = tb.Text.Trim();

                if ((header == "Giờ bắt đầu" || header == "Giờ kết thúc")
                    && !string.IsNullOrEmpty(value) && !TimeSpan.TryParse(value, out _))
                {
                    MessageBox.Show("Vui lòng nhập giờ theo định dạng HH:mm (VD: 07:00)",
                        "Sai định dạng", MessageBoxButton.OK, MessageBoxImage.Warning);
                    e.Cancel = true;
                }
            }
        }
    }
}
