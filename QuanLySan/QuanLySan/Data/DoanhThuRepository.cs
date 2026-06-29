#nullable enable
using System;
using System.Collections.Generic;
using Microsoft.Data.SqlClient;
using QuanLySan.Models;
using QuanLySan.Utils;

namespace QuanLySan.Data
{
    // Truy vấn báo cáo doanh thu theo tháng/năm (Sprint 6.1 & 6.2). Chỉ đọc.
    public class DoanhThuRepository
    {
        private readonly string _connectionString = DatabaseConfig.ConnectionString;

        // Lấy khoảng thời gian hợp lệ cho báo cáo (Từ ngày đăng ký sớm nhất hoặc ngày đặt sân sớm nhất đến hiện tại)
        public (DateTime MinDate, DateTime MaxDate) GetMinMaxReportDate()
        {
            const string sql = @"
                SELECT 
                    MIN(MinDate) AS EarliestDate
                FROM (
                    SELECT MIN(NgayDangKyHoiVien) AS MinDate FROM HOIVIEN
                    UNION ALL
                    SELECT MIN(NgayDat) AS MinDate FROM DATSAN
                ) AS T";
            
            DateTime minDate = DateTime.Now;
            try
            {
                using var conn = new SqlConnection(_connectionString);
                DbHelper.OpenConnection(conn);
                using var cmd = new SqlCommand(sql, conn);
                var result = cmd.ExecuteScalar();
                if (result != DBNull.Value && result != null)
                {
                    minDate = Convert.ToDateTime(result);
                }
            }
            catch { /* Ignore and fallback to Now */ }
            
            // Nếu minDate > Now (do dữ liệu fake tương lai), fallback
            if (minDate > DateTime.Now) minDate = DateTime.Now;
            
            return (minDate, DateTime.Now);
        }

        // BM6.1: Doanh thu theo sân. Trả về tất cả sân, kèm doanh thu & tỷ lệ lấp đầy khung giờ trong tháng.
        // Tỷ lệ lấp đầy = số khung giờ riêng biệt đã đặt / tổng số khung giờ của sân × 100%.
        public List<DoanhThuSanItem> BaoCaoTheoSan(int thang, int nam)
        {
            const string sql = @"
                SELECT s.MaSan, s.TenSan,
                       (SELECT COUNT(*) FROM CHITIETDATSAN c WHERE c.MaSan = s.MaSan) AS TongKhung,
                       ISNULL(rev.DoanhThu, 0)   AS DoanhThu,
                       ISNULL(book.SoKhungDat, 0) AS SoKhungDat
                FROM SAN s
                LEFT JOIN (
                    SELECT ct.MaSan, SUM(d.TongTien) AS DoanhThu
                    FROM DATSAN d JOIN CHITIETDATSAN ct ON d.MaChiTiet = ct.MaChiTiet
                    WHERE MONTH(d.NgayDat) = @Thang AND YEAR(d.NgayDat) = @Nam
                    GROUP BY ct.MaSan
                ) rev ON rev.MaSan = s.MaSan
                LEFT JOIN (
                    SELECT ct.MaSan, COUNT(DISTINCT d.MaChiTiet) AS SoKhungDat
                    FROM DATSAN d JOIN CHITIETDATSAN ct ON d.MaChiTiet = ct.MaChiTiet
                    WHERE MONTH(d.NgayDat) = @Thang AND YEAR(d.NgayDat) = @Nam
                    GROUP BY ct.MaSan
                ) book ON book.MaSan = s.MaSan
                ORDER BY s.TenSan";

            var ds = new List<DoanhThuSanItem>();
            using var conn = new SqlConnection(_connectionString);
            DbHelper.OpenConnection(conn);
            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@Thang", thang);
            cmd.Parameters.AddWithValue("@Nam", nam);
            using var reader = cmd.ExecuteReader();
            int stt = 1;
            while (reader.Read())
            {
                int tongKhung = Convert.ToInt32(reader["TongKhung"]);
                int soKhungDat = Convert.ToInt32(reader["SoKhungDat"]);
                ds.Add(new DoanhThuSanItem
                {
                    STT = stt++,
                    TenSan = reader["TenSan"]?.ToString() ?? "",
                    DoanhThu = reader["DoanhThu"] != DBNull.Value ? Convert.ToDecimal(reader["DoanhThu"]) : 0,
                    TyLeLapDay = tongKhung > 0 ? soKhungDat * 100.0 / tongKhung : 0
                });
            }
            return ds;
        }

        // BM6.2: Doanh thu theo khách hàng. Chỉ khách có phát sinh doanh thu trong tháng.
        // Tỷ lệ = doanh thu khách / tổng doanh thu tất cả khách × 100%.
        public List<DoanhThuKhachHangItem> BaoCaoTheoKhachHang(int thang, int nam)
        {
            const string sql = @"
                SELECT hv.MaHoiVien, hv.HoTen, SUM(d.TongTien) AS DoanhThu
                FROM DATSAN d JOIN HOIVIEN hv ON d.MaHoiVien = hv.MaHoiVien
                WHERE MONTH(d.NgayDat) = @Thang AND YEAR(d.NgayDat) = @Nam
                GROUP BY hv.MaHoiVien, hv.HoTen
                HAVING SUM(d.TongTien) > 0
                ORDER BY SUM(d.TongTien) DESC";

            // Đọc thô trước rồi tính tổng & tỷ lệ (cần tổng để chia).
            var tho = new List<(string HoTen, decimal DoanhThu)>();
            decimal tong = 0;
            using (var conn = new SqlConnection(_connectionString))
            {
                DbHelper.OpenConnection(conn);
                using var cmd = new SqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@Thang", thang);
                cmd.Parameters.AddWithValue("@Nam", nam);
                using var reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    decimal dt = reader["DoanhThu"] != DBNull.Value ? Convert.ToDecimal(reader["DoanhThu"]) : 0;
                    tho.Add((reader["HoTen"]?.ToString() ?? "", dt));
                    tong += dt;
                }
            }

            var ds = new List<DoanhThuKhachHangItem>();
            int stt = 1;
            foreach (var (hoTen, doanhThu) in tho)
            {
                ds.Add(new DoanhThuKhachHangItem
                {
                    STT = stt++,
                    HoTen = hoTen,
                    DoanhThu = doanhThu,
                    TyLe = tong > 0 ? (double)(doanhThu / tong) * 100.0 : 0
                });
            }
            return ds;
        }
    }
}
