#nullable enable
using System;
using System.Collections.Generic;
using Microsoft.Data.SqlClient;
using QuanLySan.Models;
using QuanLySan.Utils;

namespace QuanLySan.Data
{
    // Điều kiện lọc cho màn hình Tra cứu hội viên (BM3). Trường null/rỗng = bỏ qua.
    public class HoiVienFilter
    {
        public string? MaHoiVien { get; set; }
        public string? HoTen { get; set; }
        public string? SDT { get; set; }
        public string? Email { get; set; }
        public string? GhiChu { get; set; }
        public string? GioiTinh { get; set; }        // so khớp chính xác
        public string? TenLoaiHoiVien { get; set; }  // so khớp chính xác
        public int? DiemTu { get; set; }
        public int? DiemDen { get; set; }
        public DateTime? NgayTu { get; set; }
        public DateTime? NgayDen { get; set; }
    }

    // Truy cập dữ liệu HOIVIEN: thêm mới, tra cứu, gợi ý, danh mục loại hội viên.
    public class HoiVienRepository
    {
        private readonly string _connectionString = DatabaseConfig.ConnectionString;

        // Ném ngoại lệ nếu SDT/Email trùng (ràng buộc UNIQUE) để ViewModel hiển thị lỗi.
        public void ThemHoiVien(HoiVien hv, string maLoaiHoiVien)
        {
            using var conn = new SqlConnection(_connectionString);
            DbHelper.OpenConnection(conn);
            using var trans = conn.BeginTransaction();
            try
            {
                string sql = @"INSERT INTO HOIVIEN (MaHoiVien, HoTen, SDT, Email, GioiTinh, NgayDangKyHoiVien, DiemTichLuy, MaLoaiHoiVien, GhiChu)
                               VALUES (@Ma, @Ten, @SDT, @Email, @GioiTinh, @Ngay, 0, @Loai, @GhiChu)";
                using var cmd = new SqlCommand(sql, conn, trans);
                cmd.Parameters.AddWithValue("@Ma", hv.MaHoiVien);
                cmd.Parameters.AddWithValue("@Ten", hv.HoTen);
                cmd.Parameters.AddWithValue("@SDT", hv.SDT);
                cmd.Parameters.AddWithValue("@Email", hv.Email);
                cmd.Parameters.AddWithValue("@GioiTinh", hv.GioiTinh);
                cmd.Parameters.AddWithValue("@Ngay", hv.NgayDangKy ?? DateTime.Now);
                cmd.Parameters.AddWithValue("@Loai", maLoaiHoiVien);
                cmd.Parameters.AddWithValue("@GhiChu", hv.GhiChu ?? "");
                cmd.ExecuteNonQuery();
                trans.Commit();
            }
            catch
            {
                trans.Rollback();
                throw;
            }
        }

        // Tra cứu hội viên theo bộ lọc (text dùng LIKE, giới tính/loại so khớp, điểm & ngày theo khoảng).
        public List<HoiVien> TimKiem(HoiVienFilter f)
        {
            var dieuKien = new List<string>();
            var thamSo = new List<SqlParameter>();

            ThemDieuKienChua(dieuKien, thamSo, "MaHoiVien", f.MaHoiVien);
            ThemDieuKienChua(dieuKien, thamSo, "HoTen", f.HoTen);
            ThemDieuKienChua(dieuKien, thamSo, "SDT", f.SDT);
            ThemDieuKienChua(dieuKien, thamSo, "Email", f.Email);
            ThemDieuKienChua(dieuKien, thamSo, "GhiChu", f.GhiChu);

            if (!string.IsNullOrEmpty(f.GioiTinh))
            {
                dieuKien.Add("hv.GioiTinh = @GioiTinh");
                thamSo.Add(new SqlParameter("@GioiTinh", f.GioiTinh));
            }
            if (!string.IsNullOrEmpty(f.TenLoaiHoiVien))
            {
                dieuKien.Add("lhv.TenLoaiHoiVien = @TenLoai");
                thamSo.Add(new SqlParameter("@TenLoai", f.TenLoaiHoiVien));
            }
            if (f.DiemTu.HasValue) { dieuKien.Add("hv.DiemTichLuy >= @DiemTu"); thamSo.Add(new SqlParameter("@DiemTu", f.DiemTu.Value)); }
            if (f.DiemDen.HasValue) { dieuKien.Add("hv.DiemTichLuy <= @DiemDen"); thamSo.Add(new SqlParameter("@DiemDen", f.DiemDen.Value)); }
            if (f.NgayTu.HasValue) { dieuKien.Add("hv.NgayDangKyHoiVien >= @NgayTu"); thamSo.Add(new SqlParameter("@NgayTu", f.NgayTu.Value.Date)); }
            if (f.NgayDen.HasValue) { dieuKien.Add("hv.NgayDangKyHoiVien <= @NgayDen"); thamSo.Add(new SqlParameter("@NgayDen", f.NgayDen.Value.Date)); }

            string where = dieuKien.Count > 0 ? " WHERE " + string.Join(" AND ", dieuKien) : "";
            string sql = @"SELECT hv.MaHoiVien, hv.HoTen, hv.SDT, hv.Email, hv.GioiTinh,
                                  hv.NgayDangKyHoiVien, hv.DiemTichLuy, hv.GhiChu, lhv.TenLoaiHoiVien
                           FROM HOIVIEN hv
                           LEFT JOIN LOAIHOIVIEN lhv ON hv.MaLoaiHoiVien = lhv.MaLoaiHoiVien"
                         + where + " ORDER BY hv.HoTen";

            var ds = new List<HoiVien>();
            using var conn = new SqlConnection(_connectionString);
            DbHelper.OpenConnection(conn);
            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddRange(thamSo.ToArray());
            using var reader = cmd.ExecuteReader();
            int stt = 1;
            while (reader.Read())
            {
                ds.Add(new HoiVien
                {
                    STT = stt++,
                    MaHoiVien = reader["MaHoiVien"].ToString() ?? "",
                    HoTen = reader["HoTen"].ToString() ?? "",
                    SDT = reader["SDT"].ToString() ?? "",
                    Email = reader["Email"].ToString() ?? "",
                    GioiTinh = reader["GioiTinh"].ToString() ?? "",
                    NgayDangKy = reader["NgayDangKyHoiVien"] as DateTime?,
                    DiemTichLuy = reader["DiemTichLuy"] != DBNull.Value ? Convert.ToInt32(reader["DiemTichLuy"]) : 0,
                    GhiChu = reader["GhiChu"].ToString() ?? "",
                    TenLoaiHoiVien = reader["TenLoaiHoiVien"].ToString() ?? ""
                });
            }
            return ds;
        }

        // Gợi ý <có chứa> (TOP 8) cho 1 cột văn bản. Whitelist tên cột để tránh SQL injection.
        public List<string> LayGoiY(string cot, string tuKhoa)
        {
            var ketQua = new List<string>();
            var cotHopLe = new HashSet<string> { "MaHoiVien", "HoTen", "SDT", "Email", "GhiChu" };
            if (!cotHopLe.Contains(cot)) return ketQua;

            string sql = $@"SELECT DISTINCT TOP 8 {cot} FROM HOIVIEN
                            WHERE {cot} IS NOT NULL AND {cot} LIKE @tk
                            ORDER BY {cot}";
            using var conn = new SqlConnection(_connectionString);
            DbHelper.OpenConnection(conn);
            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@tk", "%" + tuKhoa + "%");
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                string giaTri = reader[0].ToString() ?? "";
                if (giaTri != "") ketQua.Add(giaTri);
            }
            return ketQua;
        }

        // Danh sách tên loại hội viên (đổ vào ComboBox lọc).
        public List<string> LoadTenLoaiHoiVien()
        {
            var ds = new List<string>();
            using var conn = new SqlConnection(_connectionString);
            DbHelper.OpenConnection(conn);
            using var cmd = new SqlCommand("SELECT TenLoaiHoiVien FROM LOAIHOIVIEN ORDER BY MaLoaiHoiVien", conn);
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                string ten = reader["TenLoaiHoiVien"]?.ToString() ?? "";
                if (ten != "") ds.Add(ten);
            }
            return ds;
        }

        // Thêm 1 điều kiện "có chứa" (LIKE %...%) nếu ô nhập có giá trị
        private static void ThemDieuKienChua(List<string> dieuKien, List<SqlParameter> thamSo, string cot, string? giaTri)
        {
            if (string.IsNullOrWhiteSpace(giaTri)) return;
            dieuKien.Add($"hv.{cot} LIKE @{cot}");
            thamSo.Add(new SqlParameter("@" + cot, "%" + giaTri.Trim() + "%"));
        }
    }
}
