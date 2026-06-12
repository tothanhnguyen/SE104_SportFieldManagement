#nullable enable
using System;
using System.Collections.Generic;
using Microsoft.Data.SqlClient;
using QuanLySan.Models;

namespace QuanLySan.Data
{
    // Lưu sân mới và các khung giờ của sân vào CHITIETDATSAN (Tiếp nhận sân - Sprint 3).
    public class SanRepository
    {
        private readonly string _connectionString = DatabaseConfig.ConnectionString;

        // Lưu SAN và toàn bộ khung giờ trong cùng 1 transaction (lỗi thì rollback hết).
        public void ThemSan(
            San san,
            IReadOnlyList<(string MaChiTiet, TimeSpan GioBatDau, TimeSpan GioKetThuc, string MaLoaiNgay)> khungGio)
        {
            using var conn = new SqlConnection(_connectionString);
            conn.Open();
            using var trans = conn.BeginTransaction();
            try
            {
                string sqlSan = @"INSERT INTO SAN (MaSan, TenSan, DiaChi, GhiChu, MaLoaiSan, MaTinhTrang)
                                  VALUES (@Ma, @Ten, @DC, @GC, @MLS, @MTT)";
                using (var cmd = new SqlCommand(sqlSan, conn, trans))
                {
                    cmd.Parameters.AddWithValue("@Ma", san.MaSan);
                    cmd.Parameters.AddWithValue("@Ten", san.TenSan);
                    cmd.Parameters.AddWithValue("@DC", san.DiaChi ?? "");
                    cmd.Parameters.AddWithValue("@GC", san.GhiChu ?? "");
                    cmd.Parameters.AddWithValue("@MLS", san.MaLoaiSan);
                    cmd.Parameters.AddWithValue("@MTT", san.MaTinhTrang);
                    cmd.ExecuteNonQuery();
                }

                foreach (var kg in khungGio)
                {
                    string sqlCt = @"INSERT INTO CHITIETDATSAN (MaChiTiet, MaSan, GioBatDau, GioKetThuc, MaLoaiNgay)
                                     VALUES (@MaCT, @MaSan, @BD, @KT, @MLN)";
                    using var cmd = new SqlCommand(sqlCt, conn, trans);
                    cmd.Parameters.AddWithValue("@MaCT", kg.MaChiTiet);
                    cmd.Parameters.AddWithValue("@MaSan", san.MaSan);
                    cmd.Parameters.AddWithValue("@BD", kg.GioBatDau);
                    cmd.Parameters.AddWithValue("@KT", kg.GioKetThuc);
                    cmd.Parameters.AddWithValue("@MLN", kg.MaLoaiNgay);
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
    }
}
