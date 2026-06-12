#nullable enable
using System;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using Microsoft.Data.SqlClient;
using QuanLySan.Models;
using QuanLySan.ViewModels.Base;

namespace QuanLySan.ViewModels
{
    public class DangKyHoiVienViewModel : BaseViewModel
    {
        private string _connectionString = DatabaseConfig.ConnectionString;

        // Dữ liệu nhập liệu
        private string _maHoiVien = "";
        public string MaHoiVien { get => _maHoiVien; set { _maHoiVien = value; OnPropertyChanged(); } }

        private string _tenHoiVien = "";
        public string TenHoiVien { get => _tenHoiVien; set { _tenHoiVien = value; OnPropertyChanged(); } }

        private string _sdt = "";
        public string SDT { get => _sdt; set { _sdt = value; OnPropertyChanged(); } }

        private string _email = "";
        public string Email { get => _email; set { _email = value; OnPropertyChanged(); } }

        private DateTime? _ngayDangKy;
        public DateTime? NgayDangKy { get => _ngayDangKy; set { _ngayDangKy = value; OnPropertyChanged(); } }

        private string _gioiTinh = "Nam";
        public string GioiTinh { get => _gioiTinh; set { _gioiTinh = value; OnPropertyChanged(); } }

        private string _loaiHoiVien = "Đồng";
        public string LoaiHoiVien { get => _loaiHoiVien; set { _loaiHoiVien = value; OnPropertyChanged(); } }

        private string _ghiChu = "";
        public string GhiChu { get => _ghiChu; set { _ghiChu = value; OnPropertyChanged(); } }

        // Commands
        public ICommand DangKyCommand { get; }
        public ICommand LamMoiCommand { get; }

        public DangKyHoiVienViewModel()
        {
            // Ngày đăng ký mặc định là ngày hiện hành
            NgayDangKy = DateTime.Now;

            DangKyCommand = new RelayCommand(_ => ThucHienDangKy());
            LamMoiCommand = new RelayCommand(_ => ThucHienLamMoi());

            PhatSinhMaHoiVien();
        }

        // Phát sinh mã hội viên ngẫu nhiên (VD: HV10001)
        private void PhatSinhMaHoiVien() => MaHoiVien = "HV" + new Random().Next(10000, 99999).ToString();

        private void ThucHienLamMoi()
        {
            TenHoiVien = "";
            SDT = "";
            Email = "";
            GhiChu = "";
            GioiTinh = "Nam";
            LoaiHoiVien = "Đồng";
            NgayDangKy = DateTime.Now;
            PhatSinhMaHoiVien();
        }

        private void ThucHienDangKy()
        {
            // Validate dữ liệu bắt buộc
            if (string.IsNullOrWhiteSpace(TenHoiVien))
            {
                MessageBox.Show("Vui lòng nhập họ tên hội viên!", "Thiếu thông tin", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            // Họ tên không được chứa chữ số
            if (TenHoiVien.Any(char.IsDigit))
            {
                MessageBox.Show("Họ tên không được chứa chữ số!", "Không hợp lệ", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (string.IsNullOrWhiteSpace(SDT))
            {
                MessageBox.Show("Vui lòng nhập số điện thoại!", "Thiếu thông tin", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            // Số điện thoại phải đúng 10 chữ số (có thể bắt đầu bằng 0)
            if (SDT.Trim().Length != 10 || !SDT.Trim().All(char.IsDigit))
            {
                MessageBox.Show("Số điện thoại phải gồm đúng 10 chữ số!", "Không hợp lệ", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (string.IsNullOrWhiteSpace(Email))
            {
                MessageBox.Show("Vui lòng nhập Email!", "Thiếu thông tin", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            // Email phải chứa ký tự "@"
            if (!Email.Contains("@"))
            {
                MessageBox.Show("Email không hợp lệ! Email phải chứa ký tự \"@\".", "Không hợp lệ", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                using (SqlTransaction trans = conn.BeginTransaction())
                {
                    try
                    {
                        // Lưu thông tin Hội Viên thẳng vào bảng (Đơn giản và tối ưu)
                        string maLoaiHV = "DO"; // Loại Đồng mặc định
                        string sqlHV = @"INSERT INTO HOIVIEN (MaHoiVien, HoTen, SDT, Email, GioiTinh, NgayDangKyHoiVien, DiemTichLuy, MaLoaiHoiVien, GhiChu)
                                         VALUES (@Ma, @Ten, @SDT, @Email, @GioiTinh, @Ngay, 0, @Loai, @GhiChu)";
                        using (SqlCommand cmd = new SqlCommand(sqlHV, conn, trans)) {
                            cmd.Parameters.AddWithValue("@Ma", MaHoiVien);
                            cmd.Parameters.AddWithValue("@Ten", TenHoiVien);
                            cmd.Parameters.AddWithValue("@SDT", SDT);
                            cmd.Parameters.AddWithValue("@Email", Email);
                            cmd.Parameters.AddWithValue("@GioiTinh", GioiTinh);
                            cmd.Parameters.AddWithValue("@Ngay", NgayDangKy ?? DateTime.Now);
                            cmd.Parameters.AddWithValue("@Loai", maLoaiHV);
                            cmd.Parameters.AddWithValue("@GhiChu", GhiChu ?? "");
                            cmd.ExecuteNonQuery();
                        }

                        // Nếu không có lỗi gì thì Commit transaction (lưu chính thức)
                        trans.Commit();
                        
                        MessageBox.Show(
                            $"Đăng ký hội viên thành công!\n\n" +
                            $"Mã HV: {MaHoiVien}\n" +
                            $"Họ tên: {TenHoiVien}\n" +
                            $"SĐT: {SDT}\n" +
                            $"Email: {Email}",
                            "Thành công", MessageBoxButton.OK, MessageBoxImage.Information);

                        ThucHienLamMoi();
                    }
                    catch (Exception ex)
                    {
                        trans.Rollback();
                        MessageBox.Show("Lỗi lưu dữ liệu: " + ex.Message + "\n(Lưu ý: SĐT hoặc Email có thể đã tồn tại!)", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
        }
    }
}
