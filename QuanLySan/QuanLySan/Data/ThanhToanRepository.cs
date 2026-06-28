#nullable enable
using System;
using System.Collections.Generic;
using Microsoft.Data.SqlClient;
using QuanLySan.Models;
using QuanLySan.Utils;

namespace QuanLySan.Data
{
    // Thanh toán đặt sân (BM5 - Sprint 5). Đọc phiếu từ DATSAN, lấy hệ số tích điểm, cộng điểm hội viên.
    public class ThanhToanRepository
    {
        private readonly string _connectionString = DatabaseConfig.ConnectionString;

        // Danh sách mã phiếu đặt sân (mỗi dòng DATSAN là 1 phiếu).
        public List<string> LoadMaDatSan()
        {
            var ds = new List<string>();
            using var conn = new SqlConnection(_connectionString);
            DbHelper.OpenConnection(conn);
            using var cmd = new SqlCommand("SELECT MaDatSan FROM DATSAN ORDER BY MaDatSan", conn);
            using var reader = cmd.ExecuteReader();
            while (reader.Read()) ds.Add(reader["MaDatSan"]?.ToString() ?? "");
            return ds;
        }

        // Thông tin 1 phiếu đặt: tên sân, tổng tiền, mã hội viên. Null nếu không tìm thấy.
        public (string TenSan, decimal TongTien, string MaHoiVien)? LayThongTinPhieuDat(string maDatSan)
        {
            const string sql = @"SELECT s.TenSan, d.TongTien, d.MaHoiVien
                                 FROM DATSAN d
                                 JOIN CHITIETDATSAN ct ON d.MaChiTiet = ct.MaChiTiet
                                 JOIN SAN s ON ct.MaSan = s.MaSan
                                 WHERE d.MaDatSan = @Ma";
            using var conn = new SqlConnection(_connectionString);
            DbHelper.OpenConnection(conn);
            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@Ma", maDatSan);
            using var reader = cmd.ExecuteReader();
            if (!reader.Read()) return null;
            return (
                reader["TenSan"]?.ToString() ?? "",
                reader["TongTien"] != DBNull.Value ? Convert.ToDecimal(reader["TongTien"]) : 0,
                reader["MaHoiVien"]?.ToString() ?? ""
            );
        }

        // Họ tên + mã loại hội viên (dùng để tính giảm giá theo hạng). Null nếu không tìm thấy.
        public (string HoTen, string MaLoaiHoiVien)? LayThongTinHoiVien(string maHoiVien)
        {
            using var conn = new SqlConnection(_connectionString);
            DbHelper.OpenConnection(conn);
            using var cmd = new SqlCommand("SELECT HoTen, MaLoaiHoiVien FROM HOIVIEN WHERE MaHoiVien = @Ma", conn);
            cmd.Parameters.AddWithValue("@Ma", maHoiVien);
            using var reader = cmd.ExecuteReader();
            if (!reader.Read()) return null;
            return (reader["HoTen"]?.ToString() ?? "", reader["MaLoaiHoiVien"]?.ToString() ?? "");
        }

        // Danh sách mã hội viên (đổ vào ComboBox).
        public List<string> LoadMaHoiVien()
        {
            var ds = new List<string>();
            using var conn = new SqlConnection(_connectionString);
            DbHelper.OpenConnection(conn);
            using var cmd = new SqlCommand("SELECT MaHoiVien FROM HOIVIEN ORDER BY MaHoiVien", conn);
            using var reader = cmd.ExecuteReader();
            while (reader.Read()) ds.Add(reader["MaHoiVien"]?.ToString() ?? "");
            return ds;
        }

        // Hệ số tích điểm (số tiền tương ứng 1 điểm) từ bảng TICHDIEM. Mặc định 100000 nếu chưa cấu hình.
        public double LayHeSoTichDiem()
        {
            using var conn = new SqlConnection(_connectionString);
            DbHelper.OpenConnection(conn);
            using var cmd = new SqlCommand("SELECT TOP 1 HeSoTichDiem FROM TICHDIEM ORDER BY Id", conn);
            var result = cmd.ExecuteScalar();
            return (result != null && result != DBNull.Value) ? Convert.ToDouble(result) : 100000;
        }

        // Cộng điểm tích lũy cho hội viên sau khi thanh toán.
        public void CongDiem(string maHoiVien, int diem)
        {
            using var conn = new SqlConnection(_connectionString);
            DbHelper.OpenConnection(conn);
            using var cmd = new SqlCommand(
                "UPDATE HOIVIEN SET DiemTichLuy = ISNULL(DiemTichLuy, 0) + @Diem WHERE MaHoiVien = @Ma", conn);
            cmd.Parameters.AddWithValue("@Diem", diem);
            cmd.Parameters.AddWithValue("@Ma", maHoiVien);
            cmd.ExecuteNonQuery();
        }
    }
}
