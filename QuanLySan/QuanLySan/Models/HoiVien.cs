using System;

namespace QuanLySan.Models
{
    // Mô hình hiển thị 1 dòng hội viên trong lưới tra cứu (BM3)
    public class HoiVien
    {
        public int STT { get; set; }
        public string MaHoiVien { get; set; } = "";
        public string HoTen { get; set; } = "";
        public string SDT { get; set; } = "";
        public string Email { get; set; } = "";
        public string GioiTinh { get; set; } = "";
        public DateTime? NgayDangKy { get; set; }
        public int DiemTichLuy { get; set; }
        public string GhiChu { get; set; } = "";
        public string TenLoaiHoiVien { get; set; } = "";
    }
}
