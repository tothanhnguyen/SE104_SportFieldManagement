#nullable enable
using System;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Windows.Input;
using QuanLySan.Data;
using QuanLySan.Models;
using QuanLySan.Services;
using QuanLySan.ViewModels.Base;

namespace QuanLySan.ViewModels
{
    // BM6.1 - Báo cáo doanh thu theo sân. Chọn tháng/năm → bấm báo cáo → đổ dữ liệu từ DB.
    public class DoanhThuTheoSanViewModel : BaseViewModel
    {
        private readonly IDialogService _dialog;
        private readonly DoanhThuRepository _repo;

        public ObservableCollection<int> DsNam { get; } = new();
        public ObservableCollection<int> DsThang { get; } = new();
        public ObservableCollection<DoanhThuSanItem> KetQua { get; } = new();

        private int _nam;
        public int Nam { get => _nam; set { _nam = value; OnPropertyChanged(); } }

        private int _thang;
        public int Thang { get => _thang; set { _thang = value; OnPropertyChanged(); } }

        private string _tongDoanhThuText = "0 đ";
        public string TongDoanhThuText { get => _tongDoanhThuText; set { _tongDoanhThuText = value; OnPropertyChanged(); } }

        public ICommand BaoCaoCommand { get; }

        public DoanhThuTheoSanViewModel() : this(new DialogService(), new DoanhThuRepository()) { }

        public DoanhThuTheoSanViewModel(IDialogService dialog, DoanhThuRepository repo)
        {
            _dialog = dialog;
            _repo = repo;

            var now = DateTime.Now;
            for (int y = now.Year; y >= now.Year - 5; y--) DsNam.Add(y);
            for (int m = 1; m <= 12; m++) DsThang.Add(m);
            Nam = now.Year;
            Thang = now.Month;

            BaoCaoCommand = new RelayCommand(_ => LapBaoCao());
        }

        private void LapBaoCao()
        {
            try
            {
                KetQua.Clear();
                decimal tong = 0;
                foreach (var item in _repo.BaoCaoTheoSan(Thang, Nam))
                {
                    KetQua.Add(item);
                    tong += item.DoanhThu;
                }
                TongDoanhThuText = tong.ToString("#,##0", CultureInfo.GetCultureInfo("vi-VN")) + " đ";
            }
            catch (Exception ex)
            {
                _dialog.Loi("Lỗi truy vấn dữ liệu: " + ex.Message);
            }
        }
    }
}
