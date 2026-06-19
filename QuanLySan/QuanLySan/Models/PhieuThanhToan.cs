#nullable enable
using System;

namespace QuanLySan.Models
{
    /// <summary>
    /// Mô hình hoá đơn thanh toán đặt sân (Sprint 4 - BM5)
    /// </summary>
    public class PhieuThanhToan
    {
        public string MaPhieuDat { get; set; } = "";
        public string TenSan { get; set; } = "";
        public string MaHoiVien { get; set; } = "";
        public string TenHoiVien { get; set; } = "";
        public DateTime NgayThanhToan { get; set; }
        public decimal TongTien { get; set; }
        public decimal GiamGia { get; set; }
        public decimal SoTienPhaiTra { get; set; }
    }
}
