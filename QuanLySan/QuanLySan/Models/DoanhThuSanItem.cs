using System.Globalization;

namespace QuanLySan.Models
{
    // 1 dòng báo cáo doanh thu theo sân (BM6.1).
    public class DoanhThuSanItem
    {
        public int STT { get; set; }
        public string TenSan { get; set; } = string.Empty;
        public decimal DoanhThu { get; set; }
        public double TyLeLapDay { get; set; } // %, 0..100

        // Chuỗi hiển thị cho DataGrid (giữ binding đơn giản trong XAML).
        public string DoanhThuText => DoanhThu.ToString("#,##0", CultureInfo.GetCultureInfo("vi-VN")) + " đ";
        public string TyLeText => TyLeLapDay.ToString("0.##", CultureInfo.InvariantCulture) + "%";
    }
}
