#nullable enable
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Input;
using QuanLySan.Data;
using QuanLySan.Services;
using QuanLySan.ViewModels.Base;

namespace QuanLySan.ViewModels
{
    /// <summary>
    /// ViewModel màn hình "Thanh toán đặt sân" (BM5 - Sprint 5).
    /// Quy định 5:
    ///  - Hội viên Bạc/Vàng/Kim cương giảm 3%/5%/10% Tổng tiền.
    ///  - Sau thanh toán, cộng điểm tích lũy = floor(Số tiền phải trả / HeSoTichDiem).
    /// </summary>
    public class ThanhToanDatSanViewModel : BaseViewModel
    {
        private readonly IDialogService _dialog;
        private readonly ThanhToanRepository _repo;
        private readonly double _heSoTichDiem;

        // Tỷ lệ giảm giá theo mã loại hội viên (Quy định 5).
        private static readonly Dictionary<string, decimal> _tyLeGiam = new()
        {
            ["BA"] = 0.03m, // Bạc
            ["VA"] = 0.05m, // Vàng
            ["KC"] = 0.10m, // Kim cương
        };

        private string _maLoaiHoiVien = ""; // hạng hội viên đang chọn (để tính giảm giá)

        // ── Danh sách hiển thị ──
        public ObservableCollection<string> DsMaPhieuDat { get; } = new();
        public ObservableCollection<string> DsMaHoiVien { get; } = new();

        // ── Thông tin phiếu thanh toán (BM5) ──
        private string _maPhieuDatSelected = "";
        public string MaPhieuDatSelected
        {
            get => _maPhieuDatSelected;
            set { _maPhieuDatSelected = value; OnPropertyChanged(); LoadThongTinPhieuDat(); }
        }

        private string _tenSan = "";
        public string TenSan { get => _tenSan; set { _tenSan = value; OnPropertyChanged(); } }

        private string _maHoiVienSelected = "";
        public string MaHoiVienSelected
        {
            get => _maHoiVienSelected;
            set { _maHoiVienSelected = value; OnPropertyChanged(); LoadThongTinHoiVien(); }
        }

        private string _tenHoiVien = "";
        public string TenHoiVien { get => _tenHoiVien; set { _tenHoiVien = value; OnPropertyChanged(); } }

        private DateTime? _ngayThanhToan;
        public DateTime? NgayThanhToan { get => _ngayThanhToan; set { _ngayThanhToan = value; OnPropertyChanged(); } }

        private decimal _tongTien;
        public decimal TongTien { get => _tongTien; set { _tongTien = value; OnPropertyChanged(); TinhTien(); } }

        // Giảm giá tự động theo hạng (read-only trên UI).
        private decimal _giamGia;
        public decimal GiamGia { get => _giamGia; set { _giamGia = value; OnPropertyChanged(); } }

        private decimal _soTienPhaiTra;
        public decimal SoTienPhaiTra { get => _soTienPhaiTra; set { _soTienPhaiTra = value; OnPropertyChanged(); } }

        // ── Commands ──
        public ICommand ThanhToanCommand { get; }
        public ICommand HuyCommand { get; }

        public ThanhToanDatSanViewModel() : this(new DialogService(), new ThanhToanRepository()) { }

        public ThanhToanDatSanViewModel(IDialogService dialog, ThanhToanRepository repo)
        {
            _dialog = dialog;
            _repo = repo;

            try { _heSoTichDiem = _repo.LayHeSoTichDiem(); }
            catch { _heSoTichDiem = 100000; }

            LoadDanhMuc();
            NgayThanhToan = DateTime.Now;

            ThanhToanCommand = new RelayCommand(_ => ThucHienThanhToan());
            HuyCommand = new RelayCommand(_ => ThucHienHuy());
        }

        // ===================== NẠP DỮ LIỆU =====================

        private void LoadDanhMuc()
        {
            try
            {
                foreach (var ma in _repo.LoadMaDatSan()) DsMaPhieuDat.Add(ma);
                foreach (var ma in _repo.LoadMaHoiVien()) DsMaHoiVien.Add(ma);
            }
            catch (Exception ex)
            {
                _dialog.Loi("Lỗi kết nối CSDL để nạp danh mục: " + ex.Message);
            }
        }

        private void LoadThongTinPhieuDat()
        {
            if (string.IsNullOrEmpty(MaPhieuDatSelected))
            {
                TenSan = ""; TongTien = 0;
                return;
            }
            try
            {
                var tt = _repo.LayThongTinPhieuDat(MaPhieuDatSelected);
                if (tt == null) { TenSan = ""; TongTien = 0; return; }

                TenSan = tt.Value.TenSan;
                TongTien = tt.Value.TongTien;          // kéo theo TinhTien()
                MaHoiVienSelected = tt.Value.MaHoiVien; // kéo theo LoadThongTinHoiVien()
            }
            catch (Exception ex)
            {
                _dialog.Loi("Lỗi khi tải thông tin phiếu đặt: " + ex.Message);
            }
        }

        private void LoadThongTinHoiVien()
        {
            if (string.IsNullOrEmpty(MaHoiVienSelected))
            {
                TenHoiVien = ""; _maLoaiHoiVien = ""; TinhTien();
                return;
            }
            try
            {
                var hv = _repo.LayThongTinHoiVien(MaHoiVienSelected);
                TenHoiVien = hv?.HoTen ?? "";
                _maLoaiHoiVien = hv?.MaLoaiHoiVien ?? "";
                TinhTien();
            }
            catch (Exception ex)
            {
                _dialog.Loi("Lỗi khi tải thông tin hội viên: " + ex.Message);
            }
        }

        // ===================== TÍNH TIỀN (Quy định 5) =====================

        // Giảm giá theo hạng + số tiền phải trả.
        private void TinhTien()
        {
            decimal tyLe = _tyLeGiam.TryGetValue(_maLoaiHoiVien, out var r) ? r : 0m;
            GiamGia = Math.Round(TongTien * tyLe);
            SoTienPhaiTra = TongTien - GiamGia;
        }

        // ===================== THANH TOÁN =====================

        private void ThucHienThanhToan()
        {
            if (string.IsNullOrEmpty(MaPhieuDatSelected)) { _dialog.CanhBao("Vui lòng chọn mã phiếu đặt!", "Thiếu thông tin"); return; }
            if (string.IsNullOrEmpty(MaHoiVienSelected)) { _dialog.CanhBao("Vui lòng chọn mã hội viên!", "Thiếu thông tin"); return; }
            if (NgayThanhToan == null) { _dialog.CanhBao("Vui lòng chọn ngày thanh toán!", "Thiếu thông tin"); return; }
            if (TongTien <= 0) { _dialog.CanhBao("Tổng tiền phải lớn hơn 0!", "Không hợp lệ"); return; }

            try
            {
                // Tích điểm: floor(Số tiền phải trả / Hệ số tích điểm)
                int diem = _heSoTichDiem > 0 ? (int)Math.Floor(SoTienPhaiTra / (decimal)_heSoTichDiem) : 0;
                if (diem > 0) _repo.CongDiem(MaHoiVienSelected, diem);

                _dialog.ThongBao(
                    $"Thanh toán thành công!\n" +
                    $"Mã phiếu: {MaPhieuDatSelected}\n" +
                    $"Giảm giá: {GiamGia:N0} VNĐ\n" +
                    $"Số tiền phải trả: {SoTienPhaiTra:N0} VNĐ\n" +
                    $"Điểm tích lũy được cộng: {diem}",
                    "Thành công");
                ThucHienHuy();
            }
            catch (Exception ex)
            {
                _dialog.Loi("Lỗi thanh toán: " + ex.Message);
            }
        }

        private void ThucHienHuy()
        {
            MaPhieuDatSelected = "";
            TenSan = "";
            MaHoiVienSelected = "";
            TenHoiVien = "";
            _maLoaiHoiVien = "";
            NgayThanhToan = DateTime.Now;
            TongTien = 0;
            GiamGia = 0;
            SoTienPhaiTra = 0;
        }
    }
}
