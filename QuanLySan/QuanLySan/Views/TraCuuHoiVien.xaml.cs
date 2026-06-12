#nullable enable
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using QuanLySan.ViewModels;

namespace QuanLySan.Views
{
    /// <summary>
    /// Màn hình Tra cứu hội viên (Sprint 3).
    /// Toàn bộ logic & truy vấn nằm ở TraCuuHoiVienViewModel; code-behind chỉ giữ thao tác
    /// thuần View: kéo cửa sổ, đóng, và cơ chế hiển thị popup gợi ý.
    /// </summary>
    public partial class TraCuuHoiVien : Window
    {
        private readonly TraCuuHoiVienViewModel _vm;
        private bool _dangApDungGoiY; // chặn đệ quy TextChanged khi gán từ gợi ý

        public TraCuuHoiVien()
        {
            InitializeComponent();
            _vm = new TraCuuHoiVienViewModel();
            DataContext = _vm;
        }

        private void BtnThoat_Click(object sender, RoutedEventArgs e) => Close();

        private void Header_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left) DragMove();
        }

        // ===== Popup gợi ý <có chứa> (thuần View; dữ liệu lấy từ ViewModel) =====

        private void OnSearchBoxTextChanged(object sender, TextChangedEventArgs e)
        {
            if (!IsInitialized || _dangApDungGoiY) return;
            if (sender is not TextBox tb) return;

            string cot = tb.Tag?.ToString() ?? "";
            string tuKhoa = tb.Text.Trim();
            if (cot == "" || tuKhoa.Length == 0) { popupGoiY.IsOpen = false; return; }

            var goiY = _vm.LayGoiY(cot, tuKhoa);
            if (goiY.Count == 0) { popupGoiY.IsOpen = false; return; }

            lstGoiY.ItemsSource = goiY;
            popupGoiY.PlacementTarget = tb;
            popupGoiY.Width = tb.ActualWidth;
            popupGoiY.IsOpen = true;
        }

        private void LstGoiY_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (lstGoiY.SelectedItem is not string chon) return;
            if (popupGoiY.PlacementTarget is TextBox tb)
            {
                _dangApDungGoiY = true;
                tb.Text = chon;           // binding TwoWay tự cập nhật ViewModel
                tb.CaretIndex = tb.Text.Length;
                _dangApDungGoiY = false;
            }
            popupGoiY.IsOpen = false;
        }
    }
}
