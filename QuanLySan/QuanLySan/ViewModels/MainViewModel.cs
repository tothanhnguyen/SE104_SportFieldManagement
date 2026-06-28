#nullable enable
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using QuanLySan.Data;
using QuanLySan.Services;
using QuanLySan.ViewModels.Base;

namespace QuanLySan.ViewModels
{
    /// <summary>
    /// ViewModel cho màn hình Dashboard chính – hiển thị thống kê tổng quan,
    /// danh sách sân có Sửa/Xóa, và điều hướng mở biểu mẫu.
    /// </summary>
    public class MainViewModel : BaseViewModel
    {
        private readonly INavigationService _nav;
        private readonly MainDashboardRepository _repo;

        // ══════════════════════════════════════════════
        //  THỐNG KÊ TỔNG QUAN (các thẻ card)
        // ══════════════════════════════════════════════

        private int _tongSan;
        public int TongSan { get => _tongSan; set { _tongSan = value; OnPropertyChanged(); } }

        private int _sanHoatDong;
        public int SanHoatDong { get => _sanHoatDong; set { _sanHoatDong = value; OnPropertyChanged(); } }

        private int _sanBaoTri;
        public int SanBaoTri { get => _sanBaoTri; set { _sanBaoTri = value; OnPropertyChanged(); } }

        private int _tongHoiVien;
        public int TongHoiVien { get => _tongHoiVien; set { _tongHoiVien = value; OnPropertyChanged(); } }

        private int _datSanHomNay;
        public int DatSanHomNay { get => _datSanHomNay; set { _datSanHomNay = value; OnPropertyChanged(); } }

        private int _datSanThangNay;
        public int DatSanThangNay { get => _datSanThangNay; set { _datSanThangNay = value; OnPropertyChanged(); } }

        private decimal _doanhThuThangNay;
        public decimal DoanhThuThangNay { get => _doanhThuThangNay; set { _doanhThuThangNay = value; OnPropertyChanged(); } }

        // Ngày giờ hiện tại (hiển thị trên header)
        private string _ngayGioHienTai = "";
        public string NgayGioHienTai { get => _ngayGioHienTai; set { _ngayGioHienTai = value; OnPropertyChanged(); } }

        private string _thangNamHienTai = "";
        public string ThangNamHienTai { get => _thangNamHienTai; set { _thangNamHienTai = value; OnPropertyChanged(); } }

        // ══════════════════════════════════════════════
        //  DANH SÁCH SÂN (DataGrid)
        // ══════════════════════════════════════════════

        public ObservableCollection<SanDashboardItem> DsSan { get; } = new();

        private SanDashboardItem? _sanSelected;
        public SanDashboardItem? SanSelected
        {
            get => _sanSelected;
            set { _sanSelected = value; OnPropertyChanged(); }
        }

        // ══════════════════════════════════════════════
        //  THỐNG KÊ 7 NGÀY (biểu đồ bar đơn giản)
        // ══════════════════════════════════════════════

        public ObservableCollection<DatSan7NgayItem> ThongKe7Ngay { get; } = new();

        // ══════════════════════════════════════════════
        //  COMMANDS
        // ══════════════════════════════════════════════

        public ICommand MoBieuMauCommand { get; }
        public ICommand RefreshCommand { get; }
        public ICommand SuaSanCommand { get; }
        public ICommand XoaSanCommand { get; }

        // ══════════════════════════════════════════════
        //  CONSTRUCTORS
        // ══════════════════════════════════════════════

        public MainViewModel() : this(new NavigationService(), new MainDashboardRepository()) { }

        public MainViewModel(INavigationService nav, MainDashboardRepository repo)
        {
            _nav = nav;
            _repo = repo;

            MoBieuMauCommand = new RelayCommand(p =>
            {
                if (p is string key) _nav.MoBieuMau(key);
                // Sau khi đóng biểu mẫu con, refresh lại dashboard
                LoadAll();
            });

            RefreshCommand = new RelayCommand(_ => LoadAll());

            SuaSanCommand = new RelayCommand(p =>
            {
                if (p is SanDashboardItem item) ThucHienSuaSan(item);
            });

            XoaSanCommand = new RelayCommand(p =>
            {
                if (p is SanDashboardItem item) ThucHienXoaSan(item);
            });

            LoadAll();
        }

        // ══════════════════════════════════════════════
        //  NẠP DỮ LIỆU
        // ══════════════════════════════════════════════

        public void LoadAll()
        {
            try
            {
                NgayGioHienTai = DateTime.Now.ToString("dddd, dd/MM/yyyy");
                ThangNamHienTai = $"Tháng {DateTime.Today.Month}/{DateTime.Today.Year}";

                TongSan = _repo.DemTongSan();
                SanHoatDong = _repo.DemSanHoatDong();
                SanBaoTri = _repo.DemSanBaoTri();
                TongHoiVien = _repo.DemTongHoiVien();
                DatSanHomNay = _repo.DemDatSanHomNay();
                DatSanThangNay = _repo.DemDatSanThangNay();
                DoanhThuThangNay = _repo.TinhDoanhThuThangNay();

                // Danh sách sân
                DsSan.Clear();
                foreach (var s in _repo.LoadDanhSachSan()) DsSan.Add(s);

                // Thống kê 7 ngày
                ThongKe7Ngay.Clear();
                var maxVal = 1;
                var data = _repo.ThongKeDatSan7Ngay();
                if (data.Any(d => d.SoLuong > 0)) maxVal = data.Max(d => d.SoLuong);
                foreach (var (ngay, soLuong) in data)
                {
                    ThongKe7Ngay.Add(new DatSan7NgayItem
                    {
                        Ngay = ngay.ToString("dd/MM"),
                        Thu = ngay.ToString("ddd"),
                        SoLuong = soLuong,
                        ChieuCaoBar = maxVal > 0 ? Math.Max(4, (double)soLuong / maxVal * 100) : 4
                    });
                }
            }
            catch (Exception ex)
            {
                // Nếu lỗi kết nối DB, vẫn hiển thị giao diện, chỉ log cảnh báo
                System.Diagnostics.Debug.WriteLine("Dashboard load error: " + ex.Message);
            }
        }

        // ══════════════════════════════════════════════
        //  SỬA SÂN
        // ══════════════════════════════════════════════

        private void ThucHienSuaSan(SanDashboardItem item)
        {
            try
            {
                _repo.CapNhatSan(item.MaSan, item.TenSan, item.DiaChi, item.GhiChu);
                MessageBox.Show($"Đã cập nhật thông tin sân \"{item.MaSan}\" thành công!", "Thành công",
                    MessageBoxButton.OK, MessageBoxImage.Information);
                LoadAll();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi cập nhật sân: " + ex.Message, "Lỗi",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // ══════════════════════════════════════════════
        //  XÓA SÂN
        // ══════════════════════════════════════════════

        private void ThucHienXoaSan(SanDashboardItem item)
        {
            var result = MessageBox.Show(
                $"Bạn có chắc chắn muốn xóa sân \"{item.TenSan}\" ({item.MaSan})?\n\n⚠ Tất cả khung giờ và lịch đặt sân liên quan sẽ bị xóa.",
                "Xác nhận xóa",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (result != MessageBoxResult.Yes) return;

            try
            {
                _repo.XoaSan(item.MaSan);
                MessageBox.Show($"Đã xóa sân \"{item.TenSan}\" thành công!", "Thành công",
                    MessageBoxButton.OK, MessageBoxImage.Information);
                LoadAll();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi xóa sân: " + ex.Message, "Lỗi",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }

    /// <summary>
    /// Model cho biểu đồ mini đặt sân 7 ngày.
    /// </summary>
    public class DatSan7NgayItem
    {
        public string Ngay { get; set; } = "";
        public string Thu { get; set; } = "";
        public int SoLuong { get; set; }
        public double ChieuCaoBar { get; set; }
    }
}
