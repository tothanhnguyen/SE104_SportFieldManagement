#nullable enable
using System;
using System.Linq;
using System.Windows.Input;
using QuanLySan.Data;
using QuanLySan.Models;
using QuanLySan.Services;
using QuanLySan.ViewModels.Base;

namespace QuanLySan.ViewModels
{
    // Đăng ký hội viên (BM2). Quy định 2: hội viên mới có điểm 0, loại "Đồng".
    public class DangKyHoiVienViewModel : BaseViewModel
    {
        private const string MA_LOAI_HOIVIEN_MACDINH = "DO"; // Loại Đồng mặc định

        private readonly IDialogService _dialog;
        private readonly HoiVienRepository _hoiVienRepo;

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

        // Constructor mặc định cho View (code-behind: new DangKyHoiVienViewModel()).
        public DangKyHoiVienViewModel()
            : this(new DialogService(), new HoiVienRepository()) { }

        // Constructor cho phép tiêm phụ thuộc (đúng MVVM, dễ kiểm thử).
        public DangKyHoiVienViewModel(IDialogService dialog, HoiVienRepository hoiVienRepo)
        {
            _dialog = dialog;
            _hoiVienRepo = hoiVienRepo;

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
            // Validate dữ liệu bắt buộc (logic trình bày → giữ ở ViewModel)
            if (string.IsNullOrWhiteSpace(TenHoiVien)) { _dialog.CanhBao("Vui lòng nhập họ tên hội viên!", "Thiếu thông tin"); return; }
            if (TenHoiVien.Any(char.IsDigit)) { _dialog.CanhBao("Họ tên không được chứa chữ số!", "Không hợp lệ"); return; }

            if (string.IsNullOrWhiteSpace(SDT)) { _dialog.CanhBao("Vui lòng nhập số điện thoại!", "Thiếu thông tin"); return; }
            if (SDT.Trim().Length != 10 || !SDT.Trim().All(char.IsDigit)) { _dialog.CanhBao("Số điện thoại phải gồm đúng 10 chữ số!", "Không hợp lệ"); return; }

            if (string.IsNullOrWhiteSpace(Email)) { _dialog.CanhBao("Vui lòng nhập Email!", "Thiếu thông tin"); return; }
            if (!Email.Contains("@")) { _dialog.CanhBao("Email không hợp lệ! Email phải chứa ký tự \"@\".", "Không hợp lệ"); return; }

            var hv = new HoiVien
            {
                MaHoiVien = MaHoiVien,
                HoTen = TenHoiVien,
                SDT = SDT,
                Email = Email,
                GioiTinh = GioiTinh,
                NgayDangKy = NgayDangKy,
                GhiChu = GhiChu ?? ""
            };

            try
            {
                _hoiVienRepo.ThemHoiVien(hv, MA_LOAI_HOIVIEN_MACDINH);
                _dialog.ThongBao(
                    $"Đăng ký hội viên thành công!\n\n" +
                    $"Mã HV: {MaHoiVien}\n" +
                    $"Họ tên: {TenHoiVien}\n" +
                    $"SĐT: {SDT}\n" +
                    $"Email: {Email}",
                    "Thành công");
                ThucHienLamMoi();
            }
            catch (Exception ex)
            {
                _dialog.Loi("Lỗi lưu dữ liệu: " + ex.Message + "\n(Lưu ý: SĐT hoặc Email có thể đã tồn tại!)");
            }
        }
    }
}
