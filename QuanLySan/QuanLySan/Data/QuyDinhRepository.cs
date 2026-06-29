using System;
using System.Collections.Generic;
using Microsoft.Data.SqlClient;
using QuanLySan.Models;
using QuanLySan.Utils;

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
            DbHelper.OpenConnection(conn);

            // Load THAMSO
            string sqlThamSo = "SELECT MucDiemTichLuyMacDinh, MaLoaiHoiVienMacDinh FROM THAMSO WHERE AccountId = @AccountId";
            using (var cmd = new SqlCommand(sqlThamSo, conn))
            {
                cmd.Parameters.AddWithValue("@AccountId", AppSession.CurrentAccountId);
                using (var reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        mucDiem = Convert.ToInt32(reader["MucDiemTichLuyMacDinh"]);
                        loaiHV = reader["MaLoaiHoiVienMacDinh"]?.ToString() ?? "";
                    }
                }
            }

            // Load TICHDIEM
            using (var cmd = new SqlCommand("SELECT HeSoTichDiem FROM TICHDIEM WHERE AccountId = @AccountId", conn))
            {
                cmd.Parameters.AddWithValue("@AccountId", AppSession.CurrentAccountId);
                using (var reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        heSo = Convert.ToInt32(reader["HeSoTichDiem"]);
                    }
                }
            }

            return (mucDiem, loaiHV, heSo);
        }

        public List<LoaiHoiVienQuyDinh> LoadDanhSachLoaiHoiVien()
        {
            var ds = new List<LoaiHoiVienQuyDinh>();
            using var conn = new SqlConnection(_cs);
            DbHelper.OpenConnection(conn);
            using var cmd = new SqlCommand("SELECT MaLoaiHoiVien, TenLoaiHoiVien, DiemToiThieu, MucGiamGia FROM LOAIHOIVIEN", conn);
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
                    MucGiamGia = reader["MucGiamGia"] != DBNull.Value ? Convert.ToDecimal(reader["MucGiamGia"]) : 0m
                });
            }
            return ds;
        }

        public void CapNhatQuyDinh(int mucDiemMacDinh, string loaiHoiVienMacDinh, int soTienQuyDoi, IEnumerable<LoaiHoiVienQuyDinh> dsHang)
        {
            using var conn = new SqlConnection(_cs);
            DbHelper.OpenConnection(conn);
            using var trans = conn.BeginTransaction();
            try
            {
                string sqlThamSo = "UPDATE THAMSO SET MucDiemTichLuyMacDinh = @MucDiem, MaLoaiHoiVienMacDinh = @MaLoai WHERE AccountId = @AccountId";
                using (var cmd = new SqlCommand(sqlThamSo, conn, trans))
                {
                    cmd.Parameters.AddWithValue("@MucDiem", mucDiemMacDinh);
                    cmd.Parameters.AddWithValue("@MaLoai", loaiHoiVienMacDinh);
                    cmd.Parameters.AddWithValue("@AccountId", AppSession.CurrentAccountId);
                    cmd.ExecuteNonQuery();
                }

                string sqlTichDiem = "UPDATE TICHDIEM SET HeSoTichDiem = @HeSo WHERE AccountId = @AccountId";
                using (var cmd = new SqlCommand(sqlTichDiem, conn, trans))
                {
                    cmd.Parameters.AddWithValue("@HeSo", soTienQuyDoi);
                    cmd.Parameters.AddWithValue("@AccountId", AppSession.CurrentAccountId);
                    cmd.ExecuteNonQuery();
                }

                foreach (var hang in dsHang)
                {
                    string sqlHang = "UPDATE LOAIHOIVIEN SET DiemToiThieu = @Diem, MucGiamGia = @MucGiam WHERE MaLoaiHoiVien = @Ma";
                    using var cmd = new SqlCommand(sqlHang, conn, trans);
                    cmd.Parameters.AddWithValue("@Diem", hang.MucDiemToiThieu);
                    cmd.Parameters.AddWithValue("@MucGiam", hang.MucGiamGia);
                    cmd.Parameters.AddWithValue("@Ma", hang.MaLoaiHoiVien);
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

        public int KiemTraHoiVienDangDungHang(string maLoaiHoiVien)
        {
            using var conn = new SqlConnection(_cs);
            DbHelper.OpenConnection(conn);
            using var cmd = new SqlCommand("SELECT COUNT(*) FROM HOIVIEN WHERE MaLoaiHoiVien = @Ma", conn);
            cmd.Parameters.AddWithValue("@Ma", maLoaiHoiVien);
            return Convert.ToInt32(cmd.ExecuteScalar());
        }

        public void XoaLoaiHoiVien(string maLoaiHoiVien)
        {
            using var conn = new SqlConnection(_cs);
            DbHelper.OpenConnection(conn);
            using var cmd = new SqlCommand("DELETE FROM LOAIHOIVIEN WHERE MaLoaiHoiVien = @Ma", conn);
            cmd.Parameters.AddWithValue("@Ma", maLoaiHoiVien);
            cmd.ExecuteNonQuery();
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
