#nullable enable
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Input;
using QuanLySan.Data;
using QuanLySan.Models;
using QuanLySan.Services;
using QuanLySan.ViewModels.Base;

namespace QuanLySan.ViewModels
{
    // Tra cứu hội viên (BM3). Lọc theo nhiều tiêu chí, hiển thị kết quả lên DataGrid.
    public class TraCuuHoiVienViewModel : BaseViewModel
    {
        private const string TAT_CA = "Tất cả";

        private readonly IDialogService _dialog;
        private readonly HoiVienRepository _hoiVienRepo;

        // ── Bộ lọc dạng văn bản (tìm "có chứa") ──
        private string _maHoiVien = "";
        public string MaHoiVien { get => _maHoiVien; set { _maHoiVien = value; OnPropertyChanged(); } }

        private string _hoTen = "";
        public string HoTen { get => _hoTen; set { _hoTen = value; OnPropertyChanged(); } }

        private string _sdt = "";
        public string SDT { get => _sdt; set { _sdt = value; OnPropertyChanged(); } }

        private string _email = "";
        public string Email { get => _email; set { _email = value; OnPropertyChanged(); } }

        private string _ghiChu = "";
        public string GhiChu { get => _ghiChu; set { _ghiChu = value; OnPropertyChanged(); } }

        // ── Bộ lọc dạng chọn ──
        private string _gioiTinhSelected = TAT_CA;
        public string GioiTinhSelected { get => _gioiTinhSelected; set { _gioiTinhSelected = value; OnPropertyChanged(); } }

        private string _loaiHoiVienSelected = TAT_CA;
        public string LoaiHoiVienSelected { get => _loaiHoiVienSelected; set { _loaiHoiVienSelected = value; OnPropertyChanged(); } }

        // ── Bộ lọc khoảng (nhập thô, validate khi bấm Tra cứu) ──
        private string _diemTu = "";
        public string DiemTu { get => _diemTu; set { _diemTu = value; OnPropertyChanged(); } }

        private string _diemDen = "";
        public string DiemDen { get => _diemDen; set { _diemDen = value; OnPropertyChanged(); } }

        private string _ngayTu = "";
        public string NgayTu { get => _ngayTu; set { _ngayTu = value; OnPropertyChanged(); } }

        private string _ngayDen = "";
        public string NgayDen { get => _ngayDen; set { _ngayDen = value; OnPropertyChanged(); } }

        // ── Danh sách hiển thị ──
        public ObservableCollection<string> DsGioiTinh { get; } = new() { TAT_CA, "Nam", "Nữ" };
        public ObservableCollection<string> DsLoaiHoiVien { get; } = new() { TAT_CA };
        public ObservableCollection<HoiVien> KetQua { get; } = new();

        // ── Commands ──
        public ICommand TimKiemCommand { get; }
        public ICommand LamMoiCommand { get; }

        // Constructor mặc định cho View (code-behind: new TraCuuHoiVienViewModel()).
        public TraCuuHoiVienViewModel()
            : this(new DialogService(), new HoiVienRepository()) { }

        // Constructor cho phép tiêm phụ thuộc (đúng MVVM, dễ kiểm thử).
        public TraCuuHoiVienViewModel(IDialogService dialog, HoiVienRepository hoiVienRepo)
        {
            _dialog = dialog;
            _hoiVienRepo = hoiVienRepo;

            TimKiemCommand = new RelayCommand(_ => ThucHienTimKiem());
            LamMoiCommand = new RelayCommand(_ => ThucHienLamMoi());

            LoadLoaiHoiVien();
            NapKetQua(new HoiVienFilter()); // hiển thị toàn bộ khi mở màn hình
        }

        // Lấy gợi ý cho 1 ô tìm kiếm (View gọi để hiện popup). Lỗi → trả rỗng, không cản trở nhập liệu.
        public List<string> LayGoiY(string cot, string tuKhoa)
        {
            try { return _hoiVienRepo.LayGoiY(cot, tuKhoa); }
            catch { return new List<string>(); }
        }

        private void LoadLoaiHoiVien()
        {
            try
            {
                foreach (var ten in _hoiVienRepo.LoadTenLoaiHoiVien()) DsLoaiHoiVien.Add(ten);
            }
            catch
            {
                // Giữ mặc định "Tất cả" nếu không kết nối được DB
            }
        }

        private void ThucHienLamMoi()
        {
            MaHoiVien = ""; HoTen = ""; SDT = ""; Email = ""; GhiChu = "";
            GioiTinhSelected = TAT_CA;
            LoaiHoiVienSelected = TAT_CA;
            DiemTu = ""; DiemDen = ""; NgayTu = ""; NgayDen = "";
            NapKetQua(new HoiVienFilter());
        }

        private void ThucHienTimKiem()
        {
            // ===== Validate điểm tích lũy =====
            int? diemTu = null, diemDen = null;
            string diemTuText = DiemTu.Trim(), diemDenText = DiemDen.Trim();

            if (diemTuText != "")
            {
                if (!int.TryParse(diemTuText, out int dt)) { _dialog.CanhBao("Điểm tích lũy (từ) phải là số nguyên.", "Cảnh báo"); return; }
                if (dt < 0) { _dialog.CanhBao("Điểm tích lũy (từ) không được là số âm.", "Cảnh báo"); return; }
                diemTu = dt;
            }
            if (diemDenText != "")
            {
                if (!int.TryParse(diemDenText, out int dd)) { _dialog.CanhBao("Điểm tích lũy (đến) phải là số nguyên.", "Cảnh báo"); return; }
                if (dd < 0) { _dialog.CanhBao("Điểm tích lũy (đến) không được là số âm.", "Cảnh báo"); return; }
                diemDen = dd;
            }
            if (diemTu.HasValue && diemDen.HasValue && diemTu.Value > diemDen.Value)
            {
                _dialog.CanhBao("Điểm tích lũy (từ) phải nhỏ hơn hoặc bằng Điểm tích lũy (đến).", "Cảnh báo");
                return;
            }

            // ===== Validate ngày đăng ký =====
            DateTime? ngayTu = null, ngayDen = null;
            string ngayTuText = NgayTu.Trim(), ngayDenText = NgayDen.Trim();

            if (ngayTuText != "")
            {
                if (!DateTime.TryParse(ngayTuText, out DateTime nt)) { _dialog.CanhBao("Ngày đăng ký (từ) không đúng định dạng.\nVui lòng nhập theo dạng dd/MM/yyyy.", "Cảnh báo"); return; }
                ngayTu = nt;
            }
            if (ngayDenText != "")
            {
                if (!DateTime.TryParse(ngayDenText, out DateTime nd)) { _dialog.CanhBao("Ngày đăng ký (đến) không đúng định dạng.\nVui lòng nhập theo dạng dd/MM/yyyy.", "Cảnh báo"); return; }
                ngayDen = nd;
            }
            if (ngayTu.HasValue && ngayDen.HasValue && ngayTu.Value > ngayDen.Value)
            {
                _dialog.CanhBao("Ngày đăng ký (từ) phải nhỏ hơn hoặc bằng Ngày đăng ký (đến).", "Cảnh báo");
                return;
            }

            var filter = new HoiVienFilter
            {
                MaHoiVien = MaHoiVien,
                HoTen = HoTen,
                SDT = SDT,
                Email = Email,
                GhiChu = GhiChu,
                GioiTinh = GioiTinhSelected == TAT_CA ? null : GioiTinhSelected,
                TenLoaiHoiVien = LoaiHoiVienSelected == TAT_CA ? null : LoaiHoiVienSelected,
                DiemTu = diemTu,
                DiemDen = diemDen,
                NgayTu = ngayTu,
                NgayDen = ngayDen
            };
            NapKetQua(filter);
        }

        private void NapKetQua(HoiVienFilter filter)
        {
            try
            {
                KetQua.Clear();
                foreach (var hv in _hoiVienRepo.TimKiem(filter)) KetQua.Add(hv);
            }
            catch (Exception ex)
            {
                _dialog.Loi("Lỗi truy vấn dữ liệu: " + ex.Message);
            }
        }
    }
}
