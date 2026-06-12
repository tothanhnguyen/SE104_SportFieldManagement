#nullable enable
using System;
using System.Collections.Generic;
using Microsoft.Data.SqlClient;
using QuanLySan.Models;

namespace QuanLySan.Data
{
    // Tra cứu khung giờ & lưu phiếu đặt sân (Đặt sân - Sprint 4).
    public class DatSanRepository
    {
        private readonly string _connectionString = DatabaseConfig.ConnectionString;

        // Tìm 1 khung giờ của sân theo Mã chi tiết + Mã sân. Đơn giá lấy theo LOAINGAY (Quy định 1).
        // Trả null nếu không tìm thấy.
        public (string MaChiTiet, TimeSpan GioBatDau, TimeSpan GioKetThuc, string LoaiNgay, decimal DonGia)? TimKhungGio(string maSan, string maChiTiet)
        {
            string sql = @"SELECT ct.MaChiTiet, ct.GioBatDau, ct.GioKetThuc, ln.TenLoaiNgay, ln.DonGiaNgay
                           FROM CHITIETDATSAN ct
                           LEFT JOIN LOAINGAY ln ON ct.MaLoaiNgay = ln.MaLoaiNgay
                           WHERE ct.MaChiTiet = @MaChiTiet AND ct.MaSan = @MaSan";
            using var conn = new SqlConnection(_connectionString);
            conn.Open();
            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@MaChiTiet", maChiTiet);
            cmd.Parameters.AddWithValue("@MaSan", maSan);
            using var reader = cmd.ExecuteReader();
            if (!reader.Read()) return null;
            return (
                reader["MaChiTiet"]?.ToString() ?? "",
                reader.GetTimeSpan(reader.GetOrdinal("GioBatDau")),
                reader.GetTimeSpan(reader.GetOrdinal("GioKetThuc")),
                reader.IsDBNull(reader.GetOrdinal("TenLoaiNgay")) ? "" : reader.GetString(reader.GetOrdinal("TenLoaiNgay")),
                reader.IsDBNull(reader.GetOrdinal("DonGiaNgay")) ? 0 : reader.GetDecimal(reader.GetOrdinal("DonGiaNgay"))
            );
        }

        // Quy định 4: đếm số khung giờ đã được đặt bị trùng giờ (cùng sân, cùng ngày) với [bd, kt).
        public int DemKhungGioTrung(string maSan, DateTime ngay, TimeSpan bd, TimeSpan kt)
        {
            string sql = @"SELECT COUNT(*)
                           FROM DATSAN d
                           JOIN CHITIETDATSAN ct ON d.MaChiTiet = ct.MaChiTiet
                           WHERE ct.MaSan = @MaSan AND d.NgayDat = @Ngay
                                 AND ct.GioBatDau < @KT AND @BD < ct.GioKetThuc";
            using var conn = new SqlConnection(_connectionString);
            conn.Open();
            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@MaSan", maSan);
            cmd.Parameters.AddWithValue("@Ngay", ngay);
            cmd.Parameters.AddWithValue("@BD", bd);
            cmd.Parameters.AddWithValue("@KT", kt);
            return Convert.ToInt32(cmd.ExecuteScalar());
        }

        // Lưu các phiếu đặt sân (mỗi khung giờ = 1 phiếu DATSAN tham chiếu MaChiTiet) trong 1 transaction.
        public void ThemPhieuDat(
            IReadOnlyList<(string MaDatSan, string MaHoiVien, string MaChiTiet, DateTime NgayDat, decimal TongTien, string GhiChu)> phieuList)
        {
            using var conn = new SqlConnection(_connectionString);
            conn.Open();
            using var trans = conn.BeginTransaction();
            try
            {
                foreach (var p in phieuList)
                {
                    string sql = @"INSERT INTO DATSAN (MaDatSan, MaHoiVien, MaChiTiet, NgayDat, TongTien, GhiChu)
                                   VALUES (@Ma, @MaHV, @MaCT, @Ngay, @Tong, @GC)";
                    using var cmd = new SqlCommand(sql, conn, trans);
                    cmd.Parameters.AddWithValue("@Ma", p.MaDatSan);
                    cmd.Parameters.AddWithValue("@MaHV", p.MaHoiVien);
                    cmd.Parameters.AddWithValue("@MaCT", p.MaChiTiet);
                    cmd.Parameters.AddWithValue("@Ngay", p.NgayDat);
                    cmd.Parameters.AddWithValue("@Tong", p.TongTien);
                    cmd.Parameters.AddWithValue("@GC", p.GhiChu ?? "");
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
