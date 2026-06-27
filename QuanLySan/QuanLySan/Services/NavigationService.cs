#nullable enable
using System.Windows;
using QuanLySan.Views;

namespace QuanLySan.Services
{
    // Mở các biểu mẫu (Window) theo khóa. Lớp hạ tầng duy nhất được phép tham chiếu lớp View.
    public class NavigationService : INavigationService
    {
        public void MoBieuMau(string key)
        {
            Window? form = key switch
            {
                "TiepNhanSan"     => new TiepNhanSanWindow(),
                "DangKyHoiVien"   => new DangKyHoiVienWindow(),
                "TraCuuHoiVien"   => new TraCuuHoiVien(),
                "DatSan"          => new DatSanWindow(),
                "ThanhToan"       => new ThanhToanDatSanWindow(),
                "BaoCaoSan"       => new DoanhThuTheoSan(),
                "BaoCaoKhachHang" => new QuanLyKhachHang.Views.DoanhThuTheoKhachHang(),
                _ => null
            };
            if (form == null) return;

            var owner = Application.Current?.MainWindow;
            if (owner != null && owner != form) form.Owner = owner;
            form.ShowDialog();
        }
    }
}
