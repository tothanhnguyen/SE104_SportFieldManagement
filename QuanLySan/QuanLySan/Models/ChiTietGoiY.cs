namespace QuanLySan.Models
{
    // Gợi ý 1 mã chi tiết đặt sân khi nhập (dropdown màn hình Đặt sân).
    public class ChiTietGoiY
    {
        public string MaChiTiet { get; set; } = "";
        // Chuỗi mô tả khung giờ + loại ngày, hiển thị kèm trong dropdown
        public string HienThi { get; set; } = "";
    }
}
