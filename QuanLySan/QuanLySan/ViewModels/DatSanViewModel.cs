#nullable enable
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Windows.Input;
using QuanLySan.Data;
using QuanLySan.Models;
using QuanLySan.Services;
using QuanLySan.ViewModels.Base;

namespace QuanLySan.ViewModels
{
    // Màn hình Đặt sân (Sprint 4). Áp dụng Quy định 4:
    //  - Không cho đặt sân đang bảo trì.
    //  - Các khung giờ đặt không được trùng với khung giờ đã được đặt.
    public class DatSanViewModel : BaseViewModel
    {
        private const string MA_TINHTRANG_BAOTRI = "BT";

        private readonly IDialogService _dialog;
        private readonly DanhMucRepository _danhMuc;
        private readonly DatSanRepository _datSanRepo;

        // ── Thông tin phiếu đặt (BM4) ──
        private string _maDatSan = "";
        public string MaDatSan { get => _maDatSan; set { _maDatSan = value; OnPropertyChanged(); } }

        private San? _sanSelected;
        public San? SanSelected
        {
            get => _sanSelected;
            set
            {
                _sanSelected = value;
                OnPropertyChanged();
                // Khi chọn sân mới, xóa danh sách giờ cũ
                DsGioDat.Clear();
                TongTien = 0;
                // Nạp gợi ý mã chi tiết theo sân vừa chọn
                LoadGoiYMaChiTiet();
            }
        }

        // Hội viên ứng với mã đã nhập (read-only, cập nhật trong setter của MaHoiVien)
        private HoiVien? _hoiVienSelected;
        public HoiVien? HoiVienSelected => _hoiVienSelected;

        // Tên hội viên hiển thị (read-only, tự cập nhật khi MaHoiVien thay đổi)
        public string TenHoiVienHienThi => HoiVienSelected?.HoTen ?? "";

        private string _maHoiVien = "";
        public string MaHoiVien
        {
            get => _maHoiVien;
            set
            {
                _maHoiVien = value;
                OnPropertyChanged();
                // Tự điền hội viên theo mã (FirstOrDefault trả null nếu không tìm thấy)
                _hoiVienSelected = string.IsNullOrEmpty(value)
                    ? null
                    : DsHoiVien.FirstOrDefault(h => h.MaHoiVien == value);
                OnPropertyChanged(nameof(HoiVienSelected));
                OnPropertyChanged(nameof(TenHoiVienHienThi));
            }
        }

        private DateTime? _ngayDat;
        public DateTime? NgayDat { get => _ngayDat; set { _ngayDat = value; OnPropertyChanged(); } }

        private string _ghiChu = "";
        public string GhiChu { get => _ghiChu; set { _ghiChu = value; OnPropertyChanged(); } }

        private decimal _tongTien;
        public decimal TongTien { get => _tongTien; set { _tongTien = value; OnPropertyChanged(); } }

        // Mã chi tiết đặt sân do quản lý nhập để tra cứu khung giờ
        private string _maChiTietInput = "";
        public string MaChiTietInput { get => _maChiTietInput; set { _maChiTietInput = value; OnPropertyChanged(); } }

        // ── Danh sách hiển thị ──
        public ObservableCollection<San> DsSan { get; } = new();
        public ObservableCollection<HoiVien> DsHoiVien { get; } = new();
        public ObservableCollection<GioSanItem> DsGioDat { get; } = new();
        // Gợi ý mã chi tiết đặt sân theo sân đang chọn
        public ObservableCollection<ChiTietGoiY> DsMaChiTiet { get; } = new();

        // ── Commands ──
        public ICommand ThemGioCommand { get; }
        public ICommand XoaGioCommand { get; }
        public ICommand DatSanCommand { get; }
        public ICommand HuyCommand { get; }

        // Constructor mặc định cho View (code-behind: new DatSanViewModel()).
        public DatSanViewModel()
            : this(new DialogService(), new DanhMucRepository(), new DatSanRepository()) { }

        // Constructor cho phép tiêm phụ thuộc (đúng MVVM, dễ kiểm thử).
        public DatSanViewModel(IDialogService dialog, DanhMucRepository danhMuc, DatSanRepository datSanRepo)
        {
            _dialog = dialog;
            _danhMuc = danhMuc;
            _datSanRepo = datSanRepo;

            LoadDanhMuc();

            // Tự tính lại tổng tiền khi danh sách giờ thay đổi
            DsGioDat.CollectionChanged += DsGioDat_CollectionChanged;

            ThemGioCommand = new RelayCommand(_ => ThucHienThemGio());
            XoaGioCommand = new RelayCommand(p =>
            {
                if (p is GioSanItem item)
                {
                    DsGioDat.Remove(item);
                    for (int i = 0; i < DsGioDat.Count; i++) DsGioDat[i].STT = i + 1;
                    TinhTongTien();
                }
            });

            DatSanCommand = new RelayCommand(_ => ThucHienDatSan());
            HuyCommand = new RelayCommand(_ => ThucHienHuy());

            NgayDat = DateTime.Now;
            PhatSinhMaDatSan();
        }

        // ===================== THÊM GIỜ =====================
        // Tra bảng CHITIETDATSAN theo MaSan + MaChiTiet đã nhập → hiện khung giờ lên DataGrid

        private void ThucHienThemGio()
        {
            if (SanSelected == null)
            {
                _dialog.CanhBao("Vui lòng chọn mã sân trước!", "Thiếu thông tin");
                return;
            }

            // Quy định 4: không cho đặt sân đang bảo trì
            if (SanSelected.MaTinhTrang == MA_TINHTRANG_BAOTRI)
            {
                _dialog.CanhBao("Sân này đang bảo trì, không thể đặt!", "Không hợp lệ");
                return;
            }

            if (string.IsNullOrWhiteSpace(MaChiTietInput))
            {
                _dialog.CanhBao("Vui lòng nhập Mã chi tiết đặt sân để tra cứu!", "Thiếu thông tin");
                return;
            }

            TraCuuChiTietDatSan();
        }

        /// <summary>
        /// Tra cứu khung giờ của sân theo MaChiTietInput + MaSan đã chọn và hiện lên DataGrid.
        /// GIỮ NGUYÊN các thông tin khác trên form (mã hội viên, ngày đặt, ghi chú...).
        /// </summary>
        private void TraCuuChiTietDatSan()
        {
            try
            {
                var kg = _datSanRepo.TimKhungGio(SanSelected!.MaSan, MaChiTietInput.Trim());
                if (kg == null)
                {
                    _dialog.ThongBao($"Không tìm thấy khung giờ với mã \"{MaChiTietInput}\" trên sân \"{SanSelected!.MaSan}\".", "Không tìm thấy");
                    return;
                }

                var (maChiTiet, gioBD, gioKT, loaiNgay, donGia) = kg.Value;

                // Tránh thêm trùng khung giờ đã có trên DataGrid
                if (DsGioDat.Any(g => g.MaChiTiet == maChiTiet))
                {
                    _dialog.ThongBao($"Mã chi tiết \"{maChiTiet}\" đã có trong danh sách.", "Đã tồn tại");
                    return;
                }

                // Thêm khung giờ vào DataGrid (không xóa dòng cũ, không đụng tới thông tin form)
                DsGioDat.Add(new GioSanItem
                {
                    STT = DsGioDat.Count + 1,
                    MaChiTiet = maChiTiet,
                    GioBatDau = gioBD.ToString(@"hh\:mm"),
                    GioKetThuc = gioKT.ToString(@"hh\:mm"),
                    LoaiNgay = loaiNgay,
                    DonGia = donGia
                });

                MaChiTietInput = "";   // dọn ô nhập để tiếp tục thêm mã khác
                TinhTongTien();
            }
            catch (Exception ex)
            {
                _dialog.Loi("Lỗi tra cứu chi tiết đặt sân: " + ex.Message);
            }
        }

        // ===================== NẠP DỮ LIỆU =====================

        private void LoadDanhMuc()
        {
            try
            {
                foreach (var s in _danhMuc.LoadSan()) DsSan.Add(s);
                foreach (var hv in _danhMuc.LoadHoiVien()) DsHoiVien.Add(hv);
                foreach (var (ma, ten, donGia) in _danhMuc.LoadLoaiNgay())
                {
                    if (!GioSanItem.DsLoaiNgay.Contains(ten)) GioSanItem.DsLoaiNgay.Add(ten);
                    GioSanItem.BangGiaQuyDinh[ten] = donGia;
                    TiepNhanSanViewModel.MapLoaiNgay[ten] = ma; // tái dùng ánh xạ Tên → Mã loại ngày
                }
            }
            catch (Exception ex)
            {
                _dialog.Loi("Lỗi kết nối CSDL để nạp danh mục: " + ex.Message);
            }
        }

        private void PhatSinhMaDatSan() => MaDatSan = "DS" + new Random().Next(10000, 99999).ToString();

        // Nạp gợi ý mã chi tiết của sân đang chọn (rỗng nếu chưa chọn sân)
        private void LoadGoiYMaChiTiet()
        {
            DsMaChiTiet.Clear();
            if (SanSelected == null) return;
            try
            {
                foreach (var (ma, bd, kt, loaiNgay) in _datSanRepo.LoadMaChiTietTheoSan(SanSelected.MaSan))
                {
                    DsMaChiTiet.Add(new ChiTietGoiY
                    {
                        MaChiTiet = ma,
                        HienThi = $"{bd:hh\\:mm}-{kt:hh\\:mm} ({loaiNgay})"
                    });
                }
            }
            catch (Exception ex)
            {
                _dialog.Loi("Lỗi nạp gợi ý mã chi tiết: " + ex.Message);
            }
        }

        // ===================== TÍNH TỔNG TIỀN =====================

        private void DsGioDat_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.NewItems != null)
                foreach (GioSanItem item in e.NewItems) item.PropertyChanged += GioItem_PropertyChanged;
            if (e.OldItems != null)
                foreach (GioSanItem item in e.OldItems) item.PropertyChanged -= GioItem_PropertyChanged;
            TinhTongTien();
        }

        private void GioItem_PropertyChanged(object? sender, PropertyChangedEventArgs e) => TinhTongTien();

        // Tổng tiền = Σ (đơn giá/giờ × số giờ của mỗi khung)
        private void TinhTongTien()
        {
            decimal tong = 0;
            foreach (var item in DsGioDat)
            {
                if (TryParseGio(item.GioBatDau, out TimeSpan bd) && TryParseGio(item.GioKetThuc, out TimeSpan kt) && kt > bd)
                {
                    double soGio = (kt - bd).TotalHours;
                    tong += item.DonGia * (decimal)soGio;
                }
            }
            TongTien = tong;
        }

        // ===================== ĐẶT SÂN (LƯU) =====================

        private void ThucHienDatSan()
        {
            // 1. Validate thông tin chung
            if (SanSelected == null) { _dialog.CanhBao("Vui lòng chọn mã sân!"); return; }
            if (string.IsNullOrEmpty(MaHoiVien) || HoiVienSelected == null) { _dialog.CanhBao("Vui lòng nhập mã hội viên hợp lệ!"); return; }
            if (NgayDat == null) { _dialog.CanhBao("Vui lòng chọn ngày đặt!"); return; }
            if (DsGioDat.Count == 0) { _dialog.CanhBao("Vui lòng thêm ít nhất một khung giờ đặt!"); return; }

            // 2. Quy định 4: không cho đặt sân đang bảo trì
            if (SanSelected.MaTinhTrang == MA_TINHTRANG_BAOTRI)
            {
                _dialog.CanhBao("Sân này đang bảo trì, không thể đặt!", "Không hợp lệ");
                return;
            }

            // 3. Chuẩn hóa từng khung giờ
            var khungGio = new List<(TimeSpan bd, TimeSpan kt)>();
            for (int i = 0; i < DsGioDat.Count; i++)
            {
                var item = DsGioDat[i];
                if (!TryParseGio(item.GioBatDau, out TimeSpan bd) || !TryParseGio(item.GioKetThuc, out TimeSpan kt))
                {
                    _dialog.CanhBao($"Dòng {i + 1}: Giờ không hợp lệ (định dạng HH:mm, VD 07:00).", "Sai định dạng");
                    return;
                }
                if (kt <= bd)
                {
                    _dialog.CanhBao($"Dòng {i + 1}: Giờ kết thúc phải lớn hơn giờ bắt đầu.", "Không hợp lệ");
                    return;
                }
                if (string.IsNullOrEmpty(item.LoaiNgay))
                {
                    _dialog.CanhBao($"Dòng {i + 1}: Vui lòng chọn loại ngày.", "Thiếu thông tin");
                    return;
                }
                khungGio.Add((bd, kt));
            }

            // 4. Quy định 4: các khung giờ trong cùng phiếu không được trùng nhau
            for (int i = 0; i < khungGio.Count; i++)
                for (int j = i + 1; j < khungGio.Count; j++)
                    if (BiTrung(khungGio[i].bd, khungGio[i].kt, khungGio[j].bd, khungGio[j].kt))
                    {
                        _dialog.CanhBao($"Khung giờ dòng {i + 1} và dòng {j + 1} bị trùng nhau!", "Trùng giờ");
                        return;
                    }

            DateTime ngay = NgayDat.Value.Date;
            string maSan = SanSelected.MaSan;

            try
            {
                // 5. Quy định 4: trùng với khung giờ đã được đặt trước đó (cùng sân, cùng ngày)
                for (int i = 0; i < DsGioDat.Count; i++)
                {
                    if (_datSanRepo.DemKhungGioTrung(maSan, ngay, khungGio[i].bd, khungGio[i].kt) > 0)
                    {
                        _dialog.CanhBao($"Dòng {i + 1} ({DsGioDat[i].GioBatDau}-{DsGioDat[i].GioKetThuc}) trùng với khung giờ đã được đặt!", "Trùng giờ");
                        return;
                    }
                }

                // 6. Mỗi khung giờ đã chọn = 1 phiếu đặt tham chiếu CHITIETDATSAN
                var phieuList = new List<(string, string, string, DateTime, decimal, string)>();
                for (int i = 0; i < DsGioDat.Count; i++)
                {
                    var item = DsGioDat[i];
                    // Phiếu đầu dùng mã đang hiển thị, các phiếu sau thêm hậu tố để không trùng khóa chính
                    string maDatSan = i == 0 ? MaDatSan : $"{MaDatSan}-{i}";
                    decimal tienDong = item.DonGia * (decimal)(khungGio[i].kt - khungGio[i].bd).TotalHours;
                    phieuList.Add((maDatSan, HoiVienSelected.MaHoiVien, item.MaChiTiet, ngay, tienDong, GhiChu ?? ""));
                }

                _datSanRepo.ThemPhieuDat(phieuList);
                _dialog.ThongBao($"Đặt sân thành công!\nMã đặt sân: {MaDatSan}\nTổng tiền: {TongTien:N0} VNĐ", "Thành công");
                ThucHienHuy();
            }
            catch (Exception ex)
            {
                _dialog.Loi("Lỗi lưu phiếu đặt: " + ex.Message);
            }
        }

        private void ThucHienHuy()
        {
            SanSelected = null;
            _hoiVienSelected = null;
            OnPropertyChanged(nameof(HoiVienSelected));
            OnPropertyChanged(nameof(TenHoiVienHienThi));
            _maHoiVien = "";
            OnPropertyChanged(nameof(MaHoiVien));
            GhiChu = "";
            NgayDat = DateTime.Now;
            DsGioDat.Clear();
            TongTien = 0;
            MaChiTietInput = "";
            PhatSinhMaDatSan();
        }

        // ===================== TIỆN ÍCH =====================

        // Hai khoảng [aBd, aKt) và [bBd, bKt) bị trùng khi: aBd < bKt && bBd < aKt
        private static bool BiTrung(TimeSpan aBd, TimeSpan aKt, TimeSpan bBd, TimeSpan bKt)
            => aBd < bKt && bBd < aKt;

        // Parse chuỗi giờ "HH:mm", "H:mm" hoặc số nguyên giờ thành TimeSpan hợp lệ
        private static bool TryParseGio(string input, out TimeSpan result)
        {
            result = TimeSpan.Zero;
            if (string.IsNullOrWhiteSpace(input)) return false;
            input = input.Trim();

            if (int.TryParse(input, out int gio) && gio >= 0 && gio <= 23)
            {
                result = new TimeSpan(gio, 0, 0);
                return true;
            }
            if (TimeSpan.TryParse(input, out TimeSpan ts) && ts >= TimeSpan.Zero && ts < TimeSpan.FromHours(24))
            {
                result = ts;
                return true;
            }
            return false;
        }
    }
}
