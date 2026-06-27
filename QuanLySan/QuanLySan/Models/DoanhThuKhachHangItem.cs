using System.Globalization;

namespace QuanLySan.Models
{
    // 1 dòng báo cáo doanh thu theo khách hàng (BM6.2).
    public class DoanhThuKhachHangItem
    {
        public int STT { get; set; }
        public string HoTen { get; set; } = string.Empty;
        public decimal DoanhThu { get; set; }
        public double TyLe { get; set; } // %, doanh thu khách / tổng doanh thu

        // Chuỗi hiển thị cho DataGrid.
        public string DoanhThuText => DoanhThu.ToString("#,##0", CultureInfo.GetCultureInfo("vi-VN")) + " đ";
        public string TyLeText => TyLe.ToString("0.##", CultureInfo.InvariantCulture) + "%";
    }
}
