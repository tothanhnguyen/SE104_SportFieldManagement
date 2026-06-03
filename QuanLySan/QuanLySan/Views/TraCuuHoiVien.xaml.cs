#nullable enable
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Data.SqlClient;
using QuanLySan.Models;

namespace QuanLySan.Views
{
    /// <summary>
    /// Màn hình Tra cứu hội viên (Sprint 3).
    /// </summary>
    public partial class TraCuuHoiVien : Window
    {
        private readonly string _connectionString = DatabaseConfig.ConnectionString;
        private readonly ObservableCollection<HoiVien> _ketQua = new();
        private bool _dangApDungGoiY; // chặn đệ quy TextChanged khi gán từ gợi ý

        public TraCuuHoiVien()
        {
            InitializeComponent();
            dgHoiVien.ItemsSource = _ketQua;
            NapDuLieu("", null); // hiển thị toàn bộ hội viên khi mở màn hình
        }

        // ====================== SỰ KIỆN NÚT ======================

        private void BtnThoat_Click(object sender, RoutedEventArgs e) => Close();

        private void BtnTimKiem_Click(object sender, RoutedEventArgs e) => ThucHienTimKiem();

        private void BtnLamMoi_Click(object sender, RoutedEventArgs e)
        {
            txtMaHoiVien.Text = "";
            txtTenHoiVien.Text = "";
            txtSDT.Text = "";
            txtEmail.Text = "";
            txtGhiChu.Text = "";
            cboGioiTinh.SelectedIndex = 0;
            cboLoaiHoiVien.SelectedIndex = 0;
            txtDiemTu.Text = "";
            txtDiemDen.Text = "";
            txtNgayTu.Text = "";
            txtNgayDen.Text = "";
            popupGoiY.IsOpen = false;
            NapDuLieu("", null);
        }

        // ================ TRUY VẤN CHO TỪNG Ô TÌM KIẾM ================

        private void ThucHienTimKiem()
        {
            var dieuKien = new List<string>();
            var thamSo = new List<SqlParameter>();

            // Các ô văn bản: tìm kiếm dạng <có chứa> (LIKE %...%)
            ThemDieuKienChua(dieuKien, thamSo, "MaHoiVien", txtMaHoiVien.Text);
            ThemDieuKienChua(dieuKien, thamSo, "HoTen", txtTenHoiVien.Text);
            ThemDieuKienChua(dieuKien, thamSo, "SDT", txtSDT.Text);
            ThemDieuKienChua(dieuKien, thamSo, "Email", txtEmail.Text);
            ThemDieuKienChua(dieuKien, thamSo, "GhiChu", txtGhiChu.Text);

            // Giới tính: so khớp chính xác
            string gioiTinh = LayNoiDungCombo(cboGioiTinh);
            if (gioiTinh != "" && gioiTinh != "Tất cả")
            {
                dieuKien.Add("hv.GioiTinh = @GioiTinh");
                thamSo.Add(new SqlParameter("@GioiTinh", gioiTinh));
            }

            // Loại hội viên: so khớp theo tên loại
            string loaiHV = LayNoiDungCombo(cboLoaiHoiVien);
            if (loaiHV != "" && loaiHV != "Tất cả")
            {
                dieuKien.Add("lhv.TenLoaiHoiVien = @TenLoai");
                thamSo.Add(new SqlParameter("@TenLoai", loaiHV));
            }

            // Điểm tích lũy: khoảng từ - đến
            if (int.TryParse(txtDiemTu.Text.Trim(), out int diemTu))
            {
                dieuKien.Add("hv.DiemTichLuy >= @DiemTu");
                thamSo.Add(new SqlParameter("@DiemTu", diemTu));
            }
            if (int.TryParse(txtDiemDen.Text.Trim(), out int diemDen))
            {
                dieuKien.Add("hv.DiemTichLuy <= @DiemDen");
                thamSo.Add(new SqlParameter("@DiemDen", diemDen));
            }

            // Ngày đăng ký: khoảng từ - đến
            if (DateTime.TryParse(txtNgayTu.Text.Trim(), out DateTime ngayTu))
            {
                dieuKien.Add("hv.NgayDangKyHoiVien >= @NgayTu");
                thamSo.Add(new SqlParameter("@NgayTu", ngayTu.Date));
            }
            if (DateTime.TryParse(txtNgayDen.Text.Trim(), out DateTime ngayDen))
            {
                dieuKien.Add("hv.NgayDangKyHoiVien <= @NgayDen");
                thamSo.Add(new SqlParameter("@NgayDen", ngayDen.Date));
            }

            string where = dieuKien.Count > 0 ? " WHERE " + string.Join(" AND ", dieuKien) : "";
            NapDuLieu(where, thamSo);
        }

        // Thêm 1 điều kiện "có chứa" (LIKE %...%) nếu ô nhập có giá trị
        private static void ThemDieuKienChua(List<string> dieuKien, List<SqlParameter> thamSo, string cot, string giaTri)
        {
            if (string.IsNullOrWhiteSpace(giaTri)) return;
            dieuKien.Add($"hv.{cot} LIKE @{cot}");
            thamSo.Add(new SqlParameter("@" + cot, "%" + giaTri.Trim() + "%"));
        }

        private static string LayNoiDungCombo(ComboBox cbo)
            => (cbo.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? "";

        // Thực thi truy vấn và đổ kết quả lên DataGrid
        private void NapDuLieu(string where, List<SqlParameter>? thamSo)
        {
            string sql = @"SELECT hv.MaHoiVien, hv.HoTen, hv.SDT, hv.Email, hv.GioiTinh,
                                  hv.NgayDangKyHoiVien, hv.DiemTichLuy, hv.GhiChu,
                                  lhv.TenLoaiHoiVien
                           FROM HOIVIEN hv
                           LEFT JOIN LOAIHOIVIEN lhv ON hv.MaLoaiHoiVien = lhv.MaLoaiHoiVien"
                         + where + " ORDER BY hv.HoTen";
            try
            {
                _ketQua.Clear();
                using var conn = new SqlConnection(_connectionString);
                conn.Open();
                using var cmd = new SqlCommand(sql, conn);
                if (thamSo != null) cmd.Parameters.AddRange(thamSo.ToArray());

                using var reader = cmd.ExecuteReader();
                int stt = 1;
                while (reader.Read())
                {
                    _ketQua.Add(new HoiVien
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
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi truy vấn dữ liệu: " + ex.Message, "Lỗi",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // ================ GỢI Ý TÌM KIẾM <CÓ CHỨA> ================

        // Khi gõ vào ô văn bản: lấy gợi ý từ DB theo kiểu LIKE %...% và hiển thị popup
        private void OnSearchBoxTextChanged(object sender, TextChangedEventArgs e)
        {
            if (!IsInitialized || _dangApDungGoiY) return; // bỏ qua khi đang khởi tạo giao diện
            if (sender is not TextBox tb) return;

            string cot = tb.Tag?.ToString() ?? "";
            string tuKhoa = tb.Text.Trim();

            if (cot == "" || tuKhoa.Length == 0)
            {
                popupGoiY.IsOpen = false;
                return;
            }

            var goiY = LayGoiY(cot, tuKhoa);
            if (goiY.Count == 0)
            {
                popupGoiY.IsOpen = false;
                return;
            }

            lstGoiY.ItemsSource = goiY;
            popupGoiY.PlacementTarget = tb;
            popupGoiY.Width = tb.ActualWidth;
            popupGoiY.IsOpen = true;
        }

        // Truy vấn danh sách gợi ý phân biệt theo cột (TOP 8 giá trị có chứa từ khóa)
        private List<string> LayGoiY(string cot, string tuKhoa)
        {
            var ketQua = new List<string>();

            // Danh sách cột hợp lệ (whitelist) -> tránh SQL injection khi ghép tên cột
            var cotHopLe = new HashSet<string> { "MaHoiVien", "HoTen", "SDT", "Email", "GhiChu" };
            if (!cotHopLe.Contains(cot)) return ketQua;

            string sql = $@"SELECT DISTINCT TOP 8 {cot} FROM HOIVIEN
                            WHERE {cot} IS NOT NULL AND {cot} LIKE @tk
                            ORDER BY {cot}";
            try
            {
                using var conn = new SqlConnection(_connectionString);
                conn.Open();
                using var cmd = new SqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@tk", "%" + tuKhoa + "%");

                using var reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    string giaTri = reader[0].ToString() ?? "";
                    if (giaTri != "") ketQua.Add(giaTri);
                }
            }
            catch
            {
                // Bỏ qua lỗi gợi ý để không cản trở việc nhập liệu
            }
            return ketQua;
        }

        // Chọn 1 gợi ý -> điền vào ô đang nhập và đóng popup
        private void LstGoiY_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (lstGoiY.SelectedItem is not string chon) return;
            if (popupGoiY.PlacementTarget is TextBox tb)
            {
                _dangApDungGoiY = true;
                tb.Text = chon;
                tb.CaretIndex = tb.Text.Length;
                _dangApDungGoiY = false;
            }
            popupGoiY.IsOpen = false;
        }
    }
}
