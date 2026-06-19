#nullable enable
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using Microsoft.Data.SqlClient;
using QuanLySan.Models;
using QuanLySan.ViewModels.Base;

namespace QuanLySan.ViewModels
{
    /// <summary>
    /// ViewModel cho màn hình "Thanh toán đặt sân" (Sprint 4 - BM5)
    /// </summary>
    public class ThanhToanDatSanViewModel : BaseViewModel
    {
        private readonly string _connectionString = DatabaseConfig.ConnectionString;

        // ── Danh sách hiển thị ──
        public ObservableCollection<string> DsMaPhieuDat { get; } = new();
        public ObservableCollection<string> DsMaHoiVien { get; } = new();

        // ── Thông tin phiếu thanh toán (BM5) ──
        private string _maPhieuDatSelected = "";
        public string MaPhieuDatSelected
        {
            get => _maPhieuDatSelected;
            set
            {
                _maPhieuDatSelected = value;
                OnPropertyChanged();
                LoadThongTinPhieuDat();
            }
        }

        private string _tenSan = "";
        public string TenSan
        {
            get => _tenSan;
            set { _tenSan = value; OnPropertyChanged(); }
        }

        private string _maHoiVienSelected = "";
        public string MaHoiVienSelected
        {
            get => _maHoiVienSelected;
            set
            {
                _maHoiVienSelected = value;
                OnPropertyChanged();
                LoadThongTinHoiVien();
            }
        }

        private string _tenHoiVien = "";
        public string TenHoiVien
        {
            get => _tenHoiVien;
            set { _tenHoiVien = value; OnPropertyChanged(); }
        }

        private DateTime? _ngayThanhToan;
        public DateTime? NgayThanhToan
        {
            get => _ngayThanhToan;
            set { _ngayThanhToan = value; OnPropertyChanged(); }
        }

        private decimal _tongTien;
        public decimal TongTien
        {
            get => _tongTien;
            set { _tongTien = value; OnPropertyChanged(); TinhSoTienPhaiTra(); }
        }

        private decimal _giamGia;
        public decimal GiamGia
        {
            get => _giamGia;
            set { _giamGia = value; OnPropertyChanged(); TinhSoTienPhaiTra(); }
        }

        private decimal _soTienPhaiTra;
        public decimal SoTienPhaiTra
        {
            get => _soTienPhaiTra;
            set { _soTienPhaiTra = value; OnPropertyChanged(); }
        }

        // ── Commands ──
        public ICommand ThanhToanCommand { get; }
        public ICommand HuyCommand { get; }

        public ThanhToanDatSanViewModel()
        {
            LoadDanhMucTuDatabase();
            NgayThanhToan = DateTime.Now;

            ThanhToanCommand = new RelayCommand(_ => ThucHienThanhToan());
            HuyCommand = new RelayCommand(_ => ThucHienHuy());
        }

        // ===================== NẠP DỮ LIỆU =====================

        private void LoadDanhMucTuDatabase()
        {
            try
            {
                using var conn = new SqlConnection(_connectionString);
                conn.Open();

                // Danh sách các mã phiếu đặt chưa thanh toán (từ PHIEUDATSAN)
                using (var cmd = new SqlCommand(@"
                    SELECT DISTINCT MaPhieuDat 
                    FROM PHIEUDATSAN 
                    ORDER BY MaPhieuDat", conn))
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        DsMaPhieuDat.Add(reader["MaPhieuDat"].ToString() ?? "");
                    }
                }

                // Danh sách mã hội viên
                using (var cmd = new SqlCommand("SELECT MaHoiVien FROM HOIVIEN ORDER BY MaHoiVien", conn))
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        DsMaHoiVien.Add(reader["MaHoiVien"].ToString() ?? "");
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi kết nối CSDL để nạp danh mục: " + ex.Message);
            }
        }

        private void LoadThongTinPhieuDat()
        {
            if (string.IsNullOrEmpty(MaPhieuDatSelected))
            {
                TenSan = "";
                TongTien = 0;
                return;
            }

            try
            {
                using var conn = new SqlConnection(_connectionString);
                conn.Open();

                // Lấy thông tin phiếu đặt sân (TenSan, TongTien)
                string sql = @"
                    SELECT p.MaSan, s.TenSan, p.TongTien, p.MaHoiVien
                    FROM PHIEUDATSAN p
                    JOIN SAN s ON p.MaSan = s.MaSan
                    WHERE p.MaPhieuDat = @Ma";

                using (var cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@Ma", MaPhieuDatSelected);
                    using var reader = cmd.ExecuteReader();
                    if (reader.Read())
                    {
                        TenSan = reader["TenSan"].ToString() ?? "";
                        TongTien = Convert.ToDecimal(reader["TongTien"]);
                        string maHV = reader["MaHoiVien"].ToString() ?? "";
                        MaHoiVienSelected = maHV;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi khi tải thông tin phiếu đặt: " + ex.Message);
            }
        }

        private void LoadThongTinHoiVien()
        {
            if (string.IsNullOrEmpty(MaHoiVienSelected))
            {
                TenHoiVien = "";
                return;
            }

            try
            {
                using var conn = new SqlConnection(_connectionString);
                conn.Open();

                using (var cmd = new SqlCommand("SELECT HoTen FROM HOIVIEN WHERE MaHoiVien = @Ma", conn))
                {
                    cmd.Parameters.AddWithValue("@Ma", MaHoiVienSelected);
                    var result = cmd.ExecuteScalar();
                    TenHoiVien = result?.ToString() ?? "";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi khi tải thông tin hội viên: " + ex.Message);
            }
        }

        // ===================== TÍNH TOÁN =====================

        private void TinhSoTienPhaiTra()
        {
            SoTienPhaiTra = TongTien - GiamGia;
        }

        // ===================== THANH TOÁN =====================

        private void ThucHienThanhToan()
        {
            // Validate thông tin chung
            if (string.IsNullOrEmpty(MaPhieuDatSelected))
            {
                MessageBox.Show("Vui lòng chọn mã phiếu đặt!", "Thiếu thông tin", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            if (string.IsNullOrEmpty(MaHoiVienSelected))
            {
                MessageBox.Show("Vui lòng chọn mã hội viên!", "Thiếu thông tin", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            if (NgayThanhToan == null)
            {
                MessageBox.Show("Vui lòng chọn ngày thanh toán!", "Thiếu thông tin", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            if (TongTien <= 0)
            {
                MessageBox.Show("Tổng tiền phải lớn hơn 0!", "Không hợp lệ", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            if (GiamGia < 0 || GiamGia > TongTien)
            {
                MessageBox.Show("Giảm giá không hợp lệ (phải từ 0 đến tổng tiền)!", "Không hợp lệ", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            using var connection = new SqlConnection(_connectionString);
            connection.Open();
            using var trans = connection.BeginTransaction();
            try
            {
                DateTime ngay = NgayThanhToan.Value.Date;

                // Lưu hoặc cập nhật thông tin thanh toán vào HOADON hoặc bảng tương tự
                // (Giả định có bảng HOADON với cấu trúc: MaHoaDon, MaPhieuDat, NgayThanhToan, TongTien, GiamGia, SoTienPhaiTra)
                string sql = @"
                    INSERT INTO HOADON (MaPhieuDat, NgayThanhToan, GiamGia, SoTienPhaiTra)
                    VALUES (@MaPhieu, @Ngay, @GiamGia, @SoTienPhaiTra)";

                using (var cmd = new SqlCommand(sql, connection, trans))
                {
                    cmd.Parameters.AddWithValue("@MaPhieu", MaPhieuDatSelected);
                    cmd.Parameters.AddWithValue("@Ngay", ngay);
                    cmd.Parameters.AddWithValue("@GiamGia", GiamGia);
                    cmd.Parameters.AddWithValue("@SoTienPhaiTra", SoTienPhaiTra);
                    cmd.ExecuteNonQuery();
                }

                trans.Commit();
                MessageBox.Show(
                    $"Thanh toán thành công!\n" +
                    $"Mã phiếu: {MaPhieuDatSelected}\n" +
                    $"Số tiền phải trả: {SoTienPhaiTra:N0} VNĐ",
                    "Thành công", MessageBoxButton.OK, MessageBoxImage.Information);
                ThucHienHuy();
            }
            catch (Exception ex)
            {
                trans.Rollback();
                MessageBox.Show("Lỗi thanh toán: " + ex.Message, "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ThucHienHuy()
        {
            MaPhieuDatSelected = "";
            TenSan = "";
            MaHoiVienSelected = "";
            TenHoiVien = "";
            NgayThanhToan = DateTime.Now;
            TongTien = 0;
            GiamGia = 0;
            SoTienPhaiTra = 0;
        }
    }
}
