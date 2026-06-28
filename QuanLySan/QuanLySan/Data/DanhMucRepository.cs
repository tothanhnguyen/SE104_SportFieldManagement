#nullable enable
using System.Collections.Generic;
using Microsoft.Data.SqlClient;
using QuanLySan.Models;
using QuanLySan.Utils;

namespace QuanLySan.Data
{
    // Truy vấn các bảng danh mục dùng chung (chỉ đọc). Tách toàn bộ SQL ra khỏi ViewModel.
    public class DanhMucRepository
    {
        private readonly string _connectionString = DatabaseConfig.ConnectionString;

        public List<(string Ma, string Ten)> LoadLoaiSan()
            => LoadMaTen("SELECT MaLoaiSan, TenLoaiSan FROM LOAISAN");

        public List<(string Ma, string Ten)> LoadTinhTrang()
            => LoadMaTen("SELECT MaTinhTrang, TenTinhTrang FROM TINHTRANG");

        public List<(string Ma, string Ten, decimal DonGia)> LoadLoaiNgay()
        {
            var ds = new List<(string, string, decimal)>();
            using var conn = new SqlConnection(_connectionString);
            DbHelper.OpenConnection(conn);
            using var cmd = new SqlCommand("SELECT MaLoaiNgay, TenLoaiNgay, DonGiaNgay FROM LOAINGAY", conn);
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
                ds.Add((reader.GetString(0), reader.GetString(1), reader.GetDecimal(2)));
            return ds;
        }

        public List<San> LoadSan()
        {
            var ds = new List<San>();
            using var conn = new SqlConnection(_connectionString);
            DbHelper.OpenConnection(conn);
            using var cmd = new SqlCommand("SELECT MaSan, TenSan, MaTinhTrang FROM SAN", conn);
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
                ds.Add(new San
                {
                    MaSan = reader["MaSan"].ToString() ?? "",
                    TenSan = reader["TenSan"].ToString() ?? "",
                    MaTinhTrang = reader["MaTinhTrang"].ToString() ?? ""
                });
            return ds;
        }

        public List<HoiVien> LoadHoiVien()
        {
            var ds = new List<HoiVien>();
            using var conn = new SqlConnection(_connectionString);
            DbHelper.OpenConnection(conn);
            using var cmd = new SqlCommand("SELECT MaHoiVien, HoTen FROM HOIVIEN", conn);
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
                ds.Add(new HoiVien
                {
                    MaHoiVien = reader["MaHoiVien"].ToString() ?? "",
                    HoTen = reader["HoTen"].ToString() ?? ""
                });
            return ds;
        }

        // Tiện ích chung cho các bảng danh mục dạng (Mã, Tên)
        private List<(string Ma, string Ten)> LoadMaTen(string sql)
        {
            var ds = new List<(string, string)>();
            using var conn = new SqlConnection(_connectionString);
            DbHelper.OpenConnection(conn);
            using var cmd = new SqlCommand(sql, conn);
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
                ds.Add((reader.GetString(0), reader.GetString(1)));
            return ds;
        }
    }
}
