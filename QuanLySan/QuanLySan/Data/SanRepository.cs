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
        public San? GetSanInfo(string maSan)
        {
            using var conn = new SqlConnection(_connectionString);
            conn.Open();
            string sql = "SELECT MaSan, TenSan, DiaChi, GhiChu, MaLoaiSan, MaTinhTrang FROM SAN WHERE MaSan = @Ma";
            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@Ma", maSan);
            using var reader = cmd.ExecuteReader();
            if (reader.Read())
            {
                return new San
                {
                    MaSan = reader["MaSan"].ToString()!,
                    TenSan = reader["TenSan"].ToString()!,
                    DiaChi = reader["DiaChi"].ToString()!,
                    GhiChu = reader["GhiChu"].ToString()!,
                    MaLoaiSan = reader["MaLoaiSan"].ToString()!,
                    MaTinhTrang = reader["MaTinhTrang"].ToString()!
                };
            }
            return null;
        }

        public List<(string MaChiTiet, TimeSpan GioBatDau, TimeSpan GioKetThuc, string MaLoaiNgay, bool IsBooked)> GetKhungGioSan(string maSan)
        {
            var list = new List<(string, TimeSpan, TimeSpan, string, bool)>();
            using var conn = new SqlConnection(_connectionString);
            conn.Open();
            string sql = @"SELECT ct.MaChiTiet, ct.GioBatDau, ct.GioKetThuc, ct.MaLoaiNgay,
                                  CAST(CASE WHEN EXISTS(SELECT 1 FROM DATSAN d WHERE d.MaChiTiet = ct.MaChiTiet) THEN 1 ELSE 0 END AS BIT) AS IsBooked
                           FROM CHITIETDATSAN ct 
                           WHERE ct.MaSan = @Ma 
                           ORDER BY ct.GioBatDau";
            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@Ma", maSan);
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                list.Add((
                    reader["MaChiTiet"].ToString()!,
                    (TimeSpan)reader["GioBatDau"],
                    (TimeSpan)reader["GioKetThuc"],
                    reader["MaLoaiNgay"].ToString()!,
                    (bool)reader["IsBooked"]
                ));
            }
            return list;
        }

        public void CapNhatSan(
            San san,
            IReadOnlyList<(string id, TimeSpan bd, TimeSpan kt, string mln)> updateItems,
            IReadOnlyList<(string id, TimeSpan bd, TimeSpan kt, string mln)> newItems,
            IReadOnlyList<string> deletedItems)
        {
            using var conn = new SqlConnection(_connectionString);
            conn.Open();
            using var trans = conn.BeginTransaction();
            try
            {
                string sqlSan = @"UPDATE SAN 
                                  SET TenSan = @Ten, DiaChi = @DC, GhiChu = @GC, MaLoaiSan = @MLS, MaTinhTrang = @MTT 
                                  WHERE MaSan = @Ma";
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

                foreach (var kg in updateItems)
                {
                    string sqlCt = @"UPDATE CHITIETDATSAN 
                                     SET GioBatDau = @BD, GioKetThuc = @KT, MaLoaiNgay = @MLN 
                                     WHERE MaChiTiet = @MaCT";
                    using var cmd = new SqlCommand(sqlCt, conn, trans);
                    cmd.Parameters.AddWithValue("@MaCT", kg.id);
                    cmd.Parameters.AddWithValue("@BD", kg.bd);
                    cmd.Parameters.AddWithValue("@KT", kg.kt);
                    cmd.Parameters.AddWithValue("@MLN", kg.mln);
                    cmd.ExecuteNonQuery();
                }

                // Generates new IDs for new slots based on existing count
                int c = updateItems.Count + deletedItems.Count + 1;
                foreach (var kg in newItems)
                {
                    // Ensure unique MaChiTiet by just using a random or incrementing logic.
                    // Wait, using an increment based on count could lead to collisions if there were deleted items.
                    // It's safer to use a new Guid substring or random, or max count.
                    // But to keep it simple, I'll use a random number or milliseconds to avoid collision.
                    string maChiTiet = $"{san.MaSan}-CT{Guid.NewGuid().ToString().Substring(0,4).ToUpper()}";
                    string sqlCt = @"INSERT INTO CHITIETDATSAN (MaChiTiet, MaSan, GioBatDau, GioKetThuc, MaLoaiNgay)
                                     VALUES (@MaCT, @MaSan, @BD, @KT, @MLN)";
                    using var cmd = new SqlCommand(sqlCt, conn, trans);
                    cmd.Parameters.AddWithValue("@MaCT", maChiTiet);
                    cmd.Parameters.AddWithValue("@MaSan", san.MaSan);
                    cmd.Parameters.AddWithValue("@BD", kg.bd);
                    cmd.Parameters.AddWithValue("@KT", kg.kt);
                    cmd.Parameters.AddWithValue("@MLN", kg.mln);
                    cmd.ExecuteNonQuery();
                }

                foreach (var id in deletedItems)
                {
                    string sqlDel = "DELETE FROM CHITIETDATSAN WHERE MaChiTiet = @MaCT";
                    using var cmd = new SqlCommand(sqlDel, conn, trans);
                    cmd.Parameters.AddWithValue("@MaCT", id);
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

        public void XoaKhungGio(string maChiTiet)
        {
            using var conn = new SqlConnection(_connectionString);
            conn.Open();
            string sql = "DELETE FROM CHITIETDATSAN WHERE MaChiTiet = @MaCT";
            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@MaCT", maChiTiet);
            cmd.ExecuteNonQuery();
        }
    }
}
