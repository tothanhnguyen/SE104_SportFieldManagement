#nullable enable
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using QuanLySan.Data;
using QuanLySan.Models;
using QuanLySan.Services;
using QuanLySan.ViewModels.Base;

namespace QuanLySan.ViewModels
{
    public class SuaSanViewModel : BaseViewModel
    {
        private readonly IDialogService _dialog;
        private readonly DanhMucRepository _danhMuc;
        private readonly SanRepository _sanRepo;
        private readonly Window _window;

        private string _maSan = "";
        public string MaSan { get => _maSan; set { _maSan = value; OnPropertyChanged(); } }

        private string _tenSan = "";
        public string TenSan { get => _tenSan; set { _tenSan = value; OnPropertyChanged(); } }

        private string _diaChi = "";
        public string DiaChi { get => _diaChi; set { _diaChi = value; OnPropertyChanged(); } }

        private string _ghiChu = "";
        public string GhiChu { get => _ghiChu; set { _ghiChu = value; OnPropertyChanged(); } }

        private string? _loaiSanSelected;
        public string? LoaiSanSelected { get => _loaiSanSelected; set { _loaiSanSelected = value; OnPropertyChanged(); } }

        private string? _tinhTrangSelected;
        public string? TinhTrangSelected { get => _tinhTrangSelected; set { _tinhTrangSelected = value; OnPropertyChanged(); } }

        public ObservableCollection<GioSanItem> DsGioSan { get; set; } = new();
        public ObservableCollection<string> DsLoaiSan { get; set; } = new();
        public ObservableCollection<string> DsTinhTrang { get; set; } = new();

        private readonly Dictionary<string, string> _mapLoaiSan = new();
        private readonly Dictionary<string, string> _mapTinhTrang = new();
        public static Dictionary<string, string> MapLoaiNgay = new();

        public ICommand ThemGioCommand { get; }
        public ICommand LuuCommand { get; }
        public ICommand HuyCommand { get; }
        public ICommand XoaGioCommand { get; }

        private readonly List<string> _deletedItems = new();

        public SuaSanViewModel(string maSan, Window window)
            : this(maSan, window, new DialogService(), new DanhMucRepository(), new SanRepository()) { }

        public SuaSanViewModel(string maSan, Window window, IDialogService dialog, DanhMucRepository danhMuc, SanRepository sanRepo)
        {
            _window = window;
            _dialog = dialog;
            _danhMuc = danhMuc;
            _sanRepo = sanRepo;

            MaSan = maSan;

            LoadDanhMuc();
            LoadThongTinSan();

            ThemGioCommand = new RelayCommand(_ =>
            {
                DsGioSan.Add(new GioSanItem
                {
                    STT = DsGioSan.Count + 1,
                    GioBatDau = "07:00",
                    GioKetThuc = "08:00",
                    LoaiNgay = GioSanItem.DsLoaiNgay.Count > 0 ? GioSanItem.DsLoaiNgay[0] : "",
                    MaChiTiet = "", // Khung giờ mới
                    IsBooked = false
                });
            });

            XoaGioCommand = new RelayCommand(p =>
            {
                if (p is GioSanItem item)
                {
                    if (item.IsBooked)
                    {
                        _dialog.CanhBao("Khung giờ này đã có người đặt, không thể xóa!");
                        return;
                    }
                    if (_dialog.XacNhan("Xác nhận xoá khung giờ ?", "Xác nhận xóa"))
                    {
                        if (!string.IsNullOrEmpty(item.MaChiTiet))
                        {
                            try
                            {
                                _sanRepo.XoaKhungGio(item.MaChiTiet);
                            }
                            catch (Exception ex)
                            {
                                _dialog.Loi("Lỗi xóa dưới cơ sở dữ liệu: " + ex.Message);
                                return;
                            }
                        }
                        DsGioSan.Remove(item);
                        // Cập nhật lại STT
                        for (int i = 0; i < DsGioSan.Count; i++) DsGioSan[i].STT = i + 1;
                    }
                }
            });

            LuuCommand = new RelayCommand(_ => ThucHienLuu());
            HuyCommand = new RelayCommand(_ => _window.Close());
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
                _dialog.Loi("Lỗi nạp danh mục: " + ex.Message);
            }
        }

        private void LoadThongTinSan()
        {
            try
            {
                var san = _sanRepo.GetSanInfo(MaSan);
                if (san != null)
                {
                    TenSan = san.TenSan;
                    DiaChi = san.DiaChi;
                    GhiChu = san.GhiChu;
                    
                    LoaiSanSelected = _mapLoaiSan.FirstOrDefault(x => x.Value == san.MaLoaiSan).Key;
                    TinhTrangSelected = _mapTinhTrang.FirstOrDefault(x => x.Value == san.MaTinhTrang).Key;
                }

                var khungGios = _sanRepo.GetKhungGioSan(MaSan);
                DsGioSan.Clear();
                _deletedItems.Clear();
                int stt = 1;
                foreach (var kg in khungGios)
                {
                    var tenLoaiNgay = MapLoaiNgay.FirstOrDefault(x => x.Value == kg.MaLoaiNgay).Key;
                    DsGioSan.Add(new GioSanItem
                    {
                        STT = stt++,
                        MaChiTiet = kg.MaChiTiet,
                        GioBatDau = kg.GioBatDau.ToString(@"hh\:mm"),
                        GioKetThuc = kg.GioKetThuc.ToString(@"hh\:mm"),
                        LoaiNgay = tenLoaiNgay ?? "",
                        DonGia = GioSanItem.BangGiaQuyDinh.GetValueOrDefault(tenLoaiNgay ?? "", 0),
                        IsBooked = kg.IsBooked
                    });
                }
            }
            catch (Exception ex)
            {
                _dialog.Loi("Lỗi tải dữ liệu sân: " + ex.Message);
            }
        }

        private void ThucHienLuu()
        {
            if (string.IsNullOrWhiteSpace(TenSan)) { _dialog.CanhBao("Vui lòng nhập tên sân!"); return; }
            if (string.IsNullOrWhiteSpace(LoaiSanSelected)) { _dialog.CanhBao("Vui lòng chọn Mã loại sân!"); return; }
            if (string.IsNullOrWhiteSpace(TinhTrangSelected)) { _dialog.CanhBao("Vui lòng chọn Tình trạng!"); return; }

            var cacKhung = new List<(string id, TimeSpan bd, TimeSpan kt, string mln)>();
            for (int i = 0; i < DsGioSan.Count; i++)
            {
                var item = DsGioSan[i];
                if (!TryParseGio(item.GioBatDau, out TimeSpan bd))
                {
                    _dialog.CanhBao($"Dòng {i + 1}: Giờ bắt đầu không hợp lệ.", "Sai định dạng");
                    return;
                }
                if (!TryParseGio(item.GioKetThuc, out TimeSpan kt))
                {
                    _dialog.CanhBao($"Dòng {i + 1}: Giờ kết thúc không hợp lệ.", "Sai định dạng");
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
                cacKhung.Add((item.MaChiTiet, bd, kt, MapLoaiNgay[item.LoaiNgay]));
            }

            for (int i = 0; i < cacKhung.Count; i++)
                for (int j = i + 1; j < cacKhung.Count; j++)
                    if (cacKhung[i].bd < cacKhung[j].kt && cacKhung[j].bd < cacKhung[i].kt)
                    {
                        _dialog.CanhBao($"Khung giờ dòng {i + 1} và dòng {j + 1} bị chồng lấn nhau!", "Trùng giờ");
                        return;
                    }

            var san = new San
            {
                MaSan = MaSan,
                TenSan = TenSan,
                DiaChi = DiaChi ?? "",
                GhiChu = GhiChu ?? "",
                MaLoaiSan = _mapLoaiSan[LoaiSanSelected!],
                MaTinhTrang = _mapTinhTrang[TinhTrangSelected!]
            };

            var updateItems = cacKhung.Where(k => !string.IsNullOrEmpty(k.id)).ToList();
            var newItems = cacKhung.Where(k => string.IsNullOrEmpty(k.id)).ToList();

            try
            {
                _sanRepo.CapNhatSan(san, updateItems, newItems, _deletedItems);
                _dialog.ThongBao("Cập nhật thông tin sân thành công!");
                _window.DialogResult = true;
                _window.Close();
            }
            catch (Exception ex)
            {
                _dialog.Loi("Lỗi cập nhật dữ liệu: " + ex.Message);
            }
        }

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
