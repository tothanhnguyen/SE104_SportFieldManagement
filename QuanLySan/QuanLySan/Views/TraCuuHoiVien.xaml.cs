#nullable enable
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
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
            NapLoaiHoiVien();
            NapDuLieu("", null); // hiển thị toàn bộ hội viên khi mở màn hình
        }

        // ====================== SỰ KIỆN NÚT ======================

        private void BtnThoat_Click(object sender, RoutedEventArgs e) => Close();

        private void Header_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left) DragMove();
        }

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
            // ===== VALIDATE INPUT =====

            // Validate điểm tích lũy
            string diemTuText = txtDiemTu.Text.Trim();
            string diemDenText = txtDiemDen.Text.Trim();

            if (diemTuText != "" && !int.TryParse(diemTuText, out _))
            {
                MessageBox.Show("Điểm tích lũy (từ) phải là số nguyên.", "Cảnh báo",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                txtDiemTu.Focus();
                return;
            }
            if (diemDenText != "" && !int.TryParse(diemDenText, out _))
            {
                MessageBox.Show("Điểm tích lũy (đến) phải là số nguyên.", "Cảnh báo",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                txtDiemDen.Focus();
                return;
            }

            int? diemTuVal = int.TryParse(diemTuText, out int dtv) ? dtv : null;
            int? diemDenVal = int.TryParse(diemDenText, out int ddv) ? ddv : null;

            if (diemTuVal.HasValue && diemTuVal.Value < 0)
            {
                MessageBox.Show("Điểm tích lũy (từ) không được là số âm.", "Cảnh báo",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                txtDiemTu.Focus();
                return;
            }
            if (diemDenVal.HasValue && diemDenVal.Value < 0)
            {
                MessageBox.Show("Điểm tích lũy (đến) không được là số âm.", "Cảnh báo",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                txtDiemDen.Focus();
                return;
            }
            if (diemTuVal.HasValue && diemDenVal.HasValue && diemTuVal.Value > diemDenVal.Value)
            {
                MessageBox.Show("Điểm tích lũy (từ) phải nhỏ hơn hoặc bằng Điểm tích lũy (đến).", "Cảnh báo",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                txtDiemTu.Focus();
                return;
            }

            // Validate ngày đăng ký
            string ngayTuText = txtNgayTu.Text.Trim();
            string ngayDenText = txtNgayDen.Text.Trim();

            if (ngayTuText != "" && !DateTime.TryParse(ngayTuText, out _))
            {
                MessageBox.Show("Ngày đăng ký (từ) không đúng định dạng.\nVui lòng nhập theo dạng dd/MM/yyyy.", "Cảnh báo",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                txtNgayTu.Focus();
                return;
            }
            if (ngayDenText != "" && !DateTime.TryParse(ngayDenText, out _))
            {
                MessageBox.Show("Ngày đăng ký (đến) không đúng định dạng.\nVui lòng nhập theo dạng dd/MM/yyyy.", "Cảnh báo",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                txtNgayDen.Focus();
                return;
            }

            DateTime? ngayTuVal = DateTime.TryParse(ngayTuText, out DateTime ntv) ? ntv : null;
            DateTime? ngayDenVal = DateTime.TryParse(ngayDenText, out DateTime ndv) ? ndv : null;

            if (ngayTuVal.HasValue && ngayDenVal.HasValue && ngayTuVal.Value > ngayDenVal.Value)
            {
                MessageBox.Show("Ngày đăng ký (từ) phải nhỏ hơn hoặc bằng Ngày đăng ký (đến).", "Cảnh báo",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                txtNgayTu.Focus();
                return;
            }

            // ===== BUILD QUERY =====
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

            // Điểm tích lũy: khoảng từ - đến (đã validate ở trên)
            if (diemTuVal.HasValue)
            {
                dieuKien.Add("hv.DiemTichLuy >= @DiemTu");
                thamSo.Add(new SqlParameter("@DiemTu", diemTuVal.Value));
            }
            if (diemDenVal.HasValue)
            {
                dieuKien.Add("hv.DiemTichLuy <= @DiemDen");
                thamSo.Add(new SqlParameter("@DiemDen", diemDenVal.Value));
            }

            // Ngày đăng ký: khoảng từ - đến (đã validate ở trên)
            if (ngayTuVal.HasValue)
            {
                dieuKien.Add("hv.NgayDangKyHoiVien >= @NgayTu");
                thamSo.Add(new SqlParameter("@NgayTu", ngayTuVal.Value.Date));
            }
            if (ngayDenVal.HasValue)
            {
                dieuKien.Add("hv.NgayDangKyHoiVien <= @NgayDen");
                thamSo.Add(new SqlParameter("@NgayDen", ngayDenVal.Value.Date));
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

        // ================ NẠP LOẠI HỘI VIÊN TỪ DB ================

        private void NapLoaiHoiVien()
        {
            cboLoaiHoiVien.Items.Clear();
            cboLoaiHoiVien.Items.Add(new ComboBoxItem { Content = "Tất cả" });
            try
            {
                using var conn = new SqlConnection(_connectionString);
                conn.Open();
                using var cmd = new SqlCommand(
                    "SELECT TenLoaiHoiVien FROM LOAIHOIVIEN ORDER BY MaLoaiHoiVien", conn);
                using var reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    string ten = reader["TenLoaiHoiVien"]?.ToString() ?? "";
                    if (ten != "") cboLoaiHoiVien.Items.Add(new ComboBoxItem { Content = ten });
                }
            }
            catch
            {
                // Giữ mặc định "Tất cả" nếu không kết nối được DB
            }
            cboLoaiHoiVien.SelectedIndex = 0;
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
