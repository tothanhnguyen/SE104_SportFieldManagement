#nullable enable
using System;
using System.Windows;
using System.Windows.Input;
using Microsoft.Data.SqlClient;
using QuanLySan.ViewModels.Base;

namespace QuanLySan.ViewModels
{
    public class DangKyHoiVienViewModel : BaseViewModel
    {
        private string _connectionString = @"Server= localhost;Database=QLSanTheThao;Trusted_Connection=True;TrustServerCertificate=True;";

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
            if (string.IsNullOrWhiteSpace(SDT))
            {
                MessageBox.Show("Vui lòng nhập số điện thoại!", "Thiếu thông tin", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // TODO: Thêm bảng HOIVIEN vào Database và implement logic lưu
            MessageBox.Show(
                $"Đăng ký hội viên thành công!\n\n" +
                $"Mã HV: {MaHoiVien}\n" +
                $"Họ tên: {TenHoiVien}\n" +
                $"SĐT: {SDT}\n" +
                $"Email: {Email}\n" +
                $"Giới tính: {GioiTinh}\n" +
                $"Loại HV: {LoaiHoiVien}",
                "Thành công", MessageBoxButton.OK, MessageBoxImage.Information);

            ThucHienLamMoi();
        }
    }
}
