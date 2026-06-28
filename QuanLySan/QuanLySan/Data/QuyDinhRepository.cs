using System;
using System.Collections.Generic;
using Microsoft.Data.SqlClient;
using QuanLySan.Models;

namespace QuanLySan.Data
{
    public class QuyDinhRepository
    {
        private readonly string _cs = DatabaseConfig.ConnectionString;

        public (int MucDiemMacDinh, string LoaiHoiVienMacDinh, int SoTienQuyDoi) LoadThamSoHethong()
        {
            int mucDiem = 0;
            string loaiHV = "";
            int heSo = 100000;

            using var conn = new SqlConnection(_cs);
            conn.Open();

            // Load THAMSO
            string sqlThamSo = "SELECT MucDiemTichLuyMacDinh, MaLoaiHoiVienMacDinh FROM THAMSO WHERE Id = 1";
            using (var cmd = new SqlCommand(sqlThamSo, conn))
            using (var reader = cmd.ExecuteReader())
            {
                if (reader.Read())
                {
                    mucDiem = Convert.ToInt32(reader["MucDiemTichLuyMacDinh"]);
                    loaiHV = reader["MaLoaiHoiVienMacDinh"]?.ToString() ?? "";
                }
            }

            // Load TICHDIEM
            using (var cmd = new SqlCommand("SELECT HeSoTichDiem FROM TICHDIEM WHERE Id = 1", conn))
            using (var reader = cmd.ExecuteReader())
            {
                if (reader.Read())
                {
                    heSo = Convert.ToInt32(reader["HeSoTichDiem"]);
                }
            }

            return (mucDiem, loaiHV, heSo);
        }

        public List<LoaiHoiVienQuyDinh> LoadDanhSachLoaiHoiVien()
        {
            var ds = new List<LoaiHoiVienQuyDinh>();
            using var conn = new SqlConnection(_cs);
            conn.Open();
            // Cột MucGiamGia không có trong database nên ta không query
            using var cmd = new SqlCommand("SELECT MaLoaiHoiVien, TenLoaiHoiVien, DiemToiThieu FROM LOAIHOIVIEN", conn);
            using var reader = cmd.ExecuteReader();
            int stt = 0;
            while (reader.Read())
            {
                stt++;
                ds.Add(new LoaiHoiVienQuyDinh
                {
                    STT = stt,
                    MaLoaiHoiVien = reader.GetString(0),
                    TenHang = reader.GetString(1),
                    MucDiemToiThieu = Convert.ToInt32(reader["DiemToiThieu"]),
                    MucGiamGia = 0 // Mặc định vì database không có cột này
                });
            }
            return ds;
        }


    }

    public class LoaiHoiVienQuyDinh
    {
        public int STT { get; set; }
        public string MaLoaiHoiVien { get; set; } = "";
        public string TenHang { get; set; } = "";
        public int MucDiemToiThieu { get; set; }
        public decimal MucGiamGia { get; set; }
    }
}
