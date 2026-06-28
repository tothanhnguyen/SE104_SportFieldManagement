#nullable enable
using System;
using System.Collections.Generic;
using Microsoft.Data.SqlClient;
using QuanLySan.Models;
using QuanLySan.Utils;

namespace QuanLySan.Data
{
    /// <summary>
    /// Truy vấn dữ liệu tổng hợp cho màn hình Dashboard chính.
    /// </summary>
    public class MainDashboardRepository
    {
        private readonly string _cs = DatabaseConfig.ConnectionString;

        // ── Thống kê tổng quan ──

        public int DemTongSan()
        {
            using var conn = new SqlConnection(_cs);
            DbHelper.OpenConnection(conn);
            using var cmd = new SqlCommand("SELECT COUNT(*) FROM SAN", conn);
            return Convert.ToInt32(cmd.ExecuteScalar());
        }

        public int DemSanHoatDong()
        {
            using var conn = new SqlConnection(_cs);
            DbHelper.OpenConnection(conn);
            using var cmd = new SqlCommand("SELECT COUNT(*) FROM SAN WHERE MaTinhTrang = 'HD'", conn);
            return Convert.ToInt32(cmd.ExecuteScalar());
        }

        public int DemSanBaoTri()
        {
            using var conn = new SqlConnection(_cs);
            DbHelper.OpenConnection(conn);
            using var cmd = new SqlCommand("SELECT COUNT(*) FROM SAN WHERE MaTinhTrang = 'BT'", conn);
            return Convert.ToInt32(cmd.ExecuteScalar());
        }

        public int DemTongHoiVien()
        {
            using var conn = new SqlConnection(_cs);
            DbHelper.OpenConnection(conn);
            using var cmd = new SqlCommand("SELECT COUNT(*) FROM HOIVIEN", conn);
            return Convert.ToInt32(cmd.ExecuteScalar());
        }

        public int DemDatSanHomNay()
        {
            using var conn = new SqlConnection(_cs);
            DbHelper.OpenConnection(conn);
            using var cmd = new SqlCommand("SELECT COUNT(*) FROM DATSAN WHERE NgayDat = @Today", conn);
            cmd.Parameters.AddWithValue("@Today", DateTime.Today);
            return Convert.ToInt32(cmd.ExecuteScalar());
        }

        public int DemDatSanThangNay()
        {
            using var conn = new SqlConnection(_cs);
            DbHelper.OpenConnection(conn);
            using var cmd = new SqlCommand(
                "SELECT COUNT(*) FROM DATSAN WHERE MONTH(NgayDat) = @M AND YEAR(NgayDat) = @Y", conn);
            cmd.Parameters.AddWithValue("@M", DateTime.Today.Month);
            cmd.Parameters.AddWithValue("@Y", DateTime.Today.Year);
            return Convert.ToInt32(cmd.ExecuteScalar());
        }

        public decimal TinhDoanhThuThangNay()
        {
            using var conn = new SqlConnection(_cs);
            DbHelper.OpenConnection(conn);
            using var cmd = new SqlCommand(
                @"SELECT ISNULL(SUM(TongTien), 0) FROM DATSAN
                  WHERE MONTH(NgayDat) = @M AND YEAR(NgayDat) = @Y", conn);
            cmd.Parameters.AddWithValue("@M", DateTime.Today.Month);
            cmd.Parameters.AddWithValue("@Y", DateTime.Today.Year);
            var result = cmd.ExecuteScalar();
            return result == null || result == DBNull.Value ? 0 : Convert.ToDecimal(result);
        }

        // ── Danh sách sân đầy đủ thông tin cho DataGrid ──

        public List<SanDashboardItem> LoadDanhSachSan()
        {
            var ds = new List<SanDashboardItem>();
            string sql = @"SELECT s.MaSan, s.TenSan, s.DiaChi, s.GhiChu,
                                  ls.TenLoaiSan, tt.TenTinhTrang, s.MaTinhTrang,
                                  (SELECT COUNT(*) FROM CHITIETDATSAN ct WHERE ct.MaSan = s.MaSan) AS SoKhungGio
                           FROM SAN s
                           LEFT JOIN LOAISAN ls ON s.MaLoaiSan = ls.MaLoaiSan
                           LEFT JOIN TINHTRANG tt ON s.MaTinhTrang = tt.MaTinhTrang
                           ORDER BY s.MaSan";
            using var conn = new SqlConnection(_cs);
            DbHelper.OpenConnection(conn);
            using var cmd = new SqlCommand(sql, conn);
            using var reader = cmd.ExecuteReader();
            int stt = 0;
            while (reader.Read())
            {
                stt++;
                ds.Add(new SanDashboardItem
                {
                    STT = stt,
                    MaSan = reader["MaSan"]?.ToString() ?? "",
                    TenSan = reader["TenSan"]?.ToString() ?? "",
                    DiaChi = reader["DiaChi"]?.ToString() ?? "",
                    GhiChu = reader["GhiChu"]?.ToString() ?? "",
                    TenLoaiSan = reader["TenLoaiSan"]?.ToString() ?? "",
                    TenTinhTrang = reader["TenTinhTrang"]?.ToString() ?? "",
                    MaTinhTrang = reader["MaTinhTrang"]?.ToString() ?? "",
                    SoKhungGio = Convert.ToInt32(reader["SoKhungGio"])
                });
            }
            return ds;
        }

        // ── Cập nhật sân ──

        public void CapNhatSan(string maSan, string tenSan, string diaChi, string ghiChu)
        {
            using var conn = new SqlConnection(_cs);
            DbHelper.OpenConnection(conn);
            using var cmd = new SqlCommand(
                "UPDATE SAN SET TenSan = @Ten, DiaChi = @DC, GhiChu = @GC WHERE MaSan = @Ma", conn);
            cmd.Parameters.AddWithValue("@Ma", maSan);
            cmd.Parameters.AddWithValue("@Ten", tenSan);
            cmd.Parameters.AddWithValue("@DC", diaChi);
            cmd.Parameters.AddWithValue("@GC", ghiChu);
            cmd.ExecuteNonQuery();
        }

        // ── Xóa sân (cascade: xóa chi tiết đặt sân, đặt sân liên quan trước) ──

        public void XoaSan(string maSan)
        {
            using var conn = new SqlConnection(_cs);
            DbHelper.OpenConnection(conn);
            using var trans = conn.BeginTransaction();
            try
            {
                // Xóa DATSAN tham chiếu đến CHITIETDATSAN của sân này
                using (var cmd = new SqlCommand(
                    @"DELETE FROM DATSAN WHERE MaChiTiet IN
                      (SELECT MaChiTiet FROM CHITIETDATSAN WHERE MaSan = @Ma)", conn, trans))
                {
                    cmd.Parameters.AddWithValue("@Ma", maSan);
                    cmd.ExecuteNonQuery();
                }
                // Xóa CHITIETDATSAN của sân
                using (var cmd = new SqlCommand("DELETE FROM CHITIETDATSAN WHERE MaSan = @Ma", conn, trans))
                {
                    cmd.Parameters.AddWithValue("@Ma", maSan);
                    cmd.ExecuteNonQuery();
                }
                // Xóa SAN
                using (var cmd = new SqlCommand("DELETE FROM SAN WHERE MaSan = @Ma", conn, trans))
                {
                    cmd.Parameters.AddWithValue("@Ma", maSan);
                    cmd.ExecuteNonQuery();
                }
                trans.Commit();
            }
            catch
            {
                trans.Rollback();
                throw;
            }
        }

        // ── Đếm đặt sân 7 ngày gần nhất (cho biểu đồ mini) ──

        public List<(DateTime Ngay, int SoLuong)> ThongKeDatSan7Ngay()
        {
            var ds = new List<(DateTime, int)>();
            using var conn = new SqlConnection(_cs);
            DbHelper.OpenConnection(conn);
            // Lấy 7 ngày gần nhất kể từ hôm nay
            for (int i = 6; i >= 0; i--)
            {
                var ngay = DateTime.Today.AddDays(-i);
                using var cmd = new SqlCommand("SELECT COUNT(*) FROM DATSAN WHERE NgayDat = @D", conn);
                cmd.Parameters.AddWithValue("@D", ngay);
                ds.Add((ngay, Convert.ToInt32(cmd.ExecuteScalar())));
            }
            return ds;
        }
    }

    /// <summary>
    /// Model hiển thị 1 dòng sân trên Dashboard DataGrid.
    /// </summary>
    public class SanDashboardItem
    {
        public int STT { get; set; }
        public string MaSan { get; set; } = "";
        public string TenSan { get; set; } = "";
        public string DiaChi { get; set; } = "";
        public string GhiChu { get; set; } = "";
        public string TenLoaiSan { get; set; } = "";
        public string TenTinhTrang { get; set; } = "";
        public string MaTinhTrang { get; set; } = "";
        public int SoKhungGio { get; set; }
        public bool IsHoatDong => MaTinhTrang == "HD";
    }
}
