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
    // Tiếp nhận thông tin sân (BM1). Lưu sân + các khung giờ vào CHITIETDATSAN.
    public class TiepNhanSanViewModel : BaseViewModel
    {
        private readonly IDialogService _dialog;
        private readonly DanhMucRepository _danhMuc;
        private readonly SanRepository _sanRepo;

        // Dữ liệu nhập liệu
        private string _maSan = "";
        public string MaSan { get => _maSan; set { _maSan = value; OnPropertyChanged(); } }

        private string _tenSan = "";
        public string TenSan { get => _tenSan; set { _tenSan = value; OnPropertyChanged(); } }

        private string _diaChi = "";
        public string DiaChi { get => _diaChi; set { _diaChi = value; OnPropertyChanged(); } }

        private string _ghiChu = "";
        public string GhiChu { get => _ghiChu; set { _ghiChu = value; OnPropertyChanged(); } }

        // Binding cho mục đang được chọn trên ComboBox
        private string? _loaiSanSelected;
        public string? LoaiSanSelected { get => _loaiSanSelected; set { _loaiSanSelected = value; OnPropertyChanged(); } }

        private string? _tinhTrangSelected;
        public string? TinhTrangSelected { get => _tinhTrangSelected; set { _tinhTrangSelected = value; OnPropertyChanged(); } }

        // Danh sách hiển thị ra giao diện
        public ObservableCollection<GioSanItem> DsGioSan { get; set; } = new();
        public ObservableCollection<string> DsLoaiSan { get; set; } = new();
        public ObservableCollection<string> DsTinhTrang { get; set; } = new();

        // Dictionary ánh xạ Tên → Mã khi lưu xuống CSDL
        private readonly Dictionary<string, string> _mapLoaiSan = new();
        private readonly Dictionary<string, string> _mapTinhTrang = new();
        public static Dictionary<string, string> MapLoaiNgay = new();

        // Commands
        public ICommand ThemGioCommand { get; }
        public ICommand LuuCommand { get; }
        public ICommand HuyCommand { get; }

        // Constructor mặc định cho View (code-behind: new TiepNhanSanViewModel()).
        public TiepNhanSanViewModel()
            : this(new DialogService(), new DanhMucRepository(), new SanRepository()) { }

        // Constructor cho phép tiêm phụ thuộc (đúng MVVM, dễ kiểm thử).
        public TiepNhanSanViewModel(IDialogService dialog, DanhMucRepository danhMuc, SanRepository sanRepo)
        {
            _dialog = dialog;
            _danhMuc = danhMuc;
            _sanRepo = sanRepo;

            LoadDanhMuc();

            ThemGioCommand = new RelayCommand(_ =>
            {
                DsGioSan.Add(new GioSanItem
                {
                    STT = DsGioSan.Count + 1,
                    GioBatDau = "07:00",
                    GioKetThuc = "08:00",
                    LoaiNgay = GioSanItem.DsLoaiNgay.Count > 0 ? GioSanItem.DsLoaiNgay[0] : ""
                });
            });

            LuuCommand = new RelayCommand(_ => ThucHienLuu());
            HuyCommand = new RelayCommand(_ => ThucHienHuy());

            PhatSinhMaSan();
        }

        private void LoadDanhMuc()
        {
            try
            {
                foreach (var (ma, ten) in _danhMuc.LoadLoaiSan())
                {
                    DsLoaiSan.Add(ten);
                    _mapLoaiSan[ten] = ma;
                }
                foreach (var (ma, ten) in _danhMuc.LoadTinhTrang())
                {
                    DsTinhTrang.Add(ten);
                    _mapTinhTrang[ten] = ma;
                }
                foreach (var (ma, ten, donGia) in _danhMuc.LoadLoaiNgay())
                {
                    if (!GioSanItem.DsLoaiNgay.Contains(ten)) GioSanItem.DsLoaiNgay.Add(ten);
                    GioSanItem.BangGiaQuyDinh[ten] = donGia;
                    MapLoaiNgay[ten] = ma;
                }
            }
            catch (Exception ex)
            {
                _dialog.Loi("Lỗi kết nối CSDL để nạp danh mục: " + ex.Message);
            }
        }

        // Phát sinh mã sân (VD: S24599)
        private void PhatSinhMaSan() => MaSan = "S" + new Random().Next(10000, 99999).ToString();

        private void ThucHienHuy()
        {
            TenSan = ""; DiaChi = ""; GhiChu = "";
            LoaiSanSelected = null;
            TinhTrangSelected = null;
            DsGioSan.Clear();
            PhatSinhMaSan();
        }

        private void ThucHienLuu()
        {
            if (string.IsNullOrWhiteSpace(TenSan)) { _dialog.CanhBao("Vui lòng nhập tên sân!"); return; }
            if (string.IsNullOrWhiteSpace(LoaiSanSelected)) { _dialog.CanhBao("Vui lòng chọn Mã loại sân!"); return; }
            if (string.IsNullOrWhiteSpace(TinhTrangSelected)) { _dialog.CanhBao("Vui lòng chọn Tình trạng!"); return; }

            // Validate giờ sân trước khi lưu (Quy định 1: các khung giờ không chồng lấn lên nhau)
            var cacKhung = new List<(TimeSpan bd, TimeSpan kt)>();
            for (int i = 0; i < DsGioSan.Count; i++)
            {
                var item = DsGioSan[i];
                if (!TryParseGio(item.GioBatDau, out TimeSpan bd))
                {
                    _dialog.CanhBao($"Dòng {i + 1}: Giờ bắt đầu \"{item.GioBatDau}\" không hợp lệ.\nVui lòng nhập theo định dạng HH:mm (VD: 07:00)", "Sai định dạng");
                    return;
                }
                if (!TryParseGio(item.GioKetThuc, out TimeSpan kt))
                {
                    _dialog.CanhBao($"Dòng {i + 1}: Giờ kết thúc \"{item.GioKetThuc}\" không hợp lệ.\nVui lòng nhập theo định dạng HH:mm (VD: 08:00)", "Sai định dạng");
                    return;
                }
                if (kt <= bd)
                {
                    _dialog.CanhBao($"Dòng {i + 1}: Giờ kết thúc phải lớn hơn giờ bắt đầu.", "Không hợp lệ");
                    return;
                }
                if (string.IsNullOrEmpty(item.LoaiNgay))
                {
                    _dialog.CanhBao($"Dòng {i + 1}: Vui lòng chọn Loại ngày.", "Thiếu thông tin");
                    return;
                }
                cacKhung.Add((bd, kt));
            }

            // Quy định 1: các khung giờ không được chồng lấn lên nhau
            for (int i = 0; i < cacKhung.Count; i++)
                for (int j = i + 1; j < cacKhung.Count; j++)
                    if (cacKhung[i].bd < cacKhung[j].kt && cacKhung[j].bd < cacKhung[i].kt)
                    {
                        _dialog.CanhBao($"Khung giờ dòng {i + 1} và dòng {j + 1} bị chồng lấn nhau!", "Trùng giờ");
                        return;
                    }

            // Dựng dữ liệu để lưu
            var san = new San
            {
                MaSan = MaSan,
                TenSan = TenSan,
                DiaChi = DiaChi ?? "",
                GhiChu = GhiChu ?? "",
                MaLoaiSan = _mapLoaiSan[LoaiSanSelected!],
                MaTinhTrang = _mapTinhTrang[TinhTrangSelected!]
            };

            var khungGio = new List<(string, TimeSpan, TimeSpan, string)>();
            for (int i = 0; i < DsGioSan.Count; i++)
            {
                string maChiTiet = $"{MaSan}-CT{(i + 1):D2}";
                khungGio.Add((maChiTiet, cacKhung[i].bd, cacKhung[i].kt, MapLoaiNgay[DsGioSan[i].LoaiNgay]));
            }

            try
            {
                _sanRepo.ThemSan(san, khungGio);
                _dialog.ThongBao("Lưu thành công dữ liệu xuống cơ sở dữ liệu!");
                ThucHienHuy();
            }
            catch (Exception ex)
            {
                _dialog.Loi("Lỗi lưu dữ liệu: " + ex.Message);
            }
        }

        /// <summary>
        /// Parse chuỗi giờ "HH:mm", "H:mm", "HH" hoặc số nguyên (giờ) thành TimeSpan hợp lệ (00:00–23:59).
        /// </summary>
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
