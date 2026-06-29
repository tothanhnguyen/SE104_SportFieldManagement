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
        public int Nam 
        { 
            get => _nam; 
            set 
            { 
                if (_nam != value)
                {
                    _nam = value; 
                    OnPropertyChanged(); 
                    UpdateDsThang();
                }
            } 
        }

        private int _thang;
        public int Thang { get => _thang; set { _thang = value; OnPropertyChanged(); } }

        private DateTime _minDate;
        private DateTime _maxDate;

        private string _tongDoanhThuText = "0 đ";
        public string TongDoanhThuText { get => _tongDoanhThuText; set { _tongDoanhThuText = value; OnPropertyChanged(); } }

        public ICommand BaoCaoCommand { get; }

        public DoanhThuTheoSanViewModel() : this(new DialogService(), new DoanhThuRepository()) { }

        public DoanhThuTheoSanViewModel(IDialogService dialog, DoanhThuRepository repo)
        {
            _dialog = dialog;
            _repo = repo;

            var dates = _repo.GetMinMaxReportDate();
            _minDate = dates.MinDate;
            _maxDate = dates.MaxDate;

            for (int y = _minDate.Year; y <= _maxDate.Year; y++) 
            {
                DsNam.Add(y);
            }
            
            // Set default Nam to maxDate.Year, which triggers UpdateDsThang()
            Nam = _maxDate.Year;
            Thang = _maxDate.Month;

            BaoCaoCommand = new RelayCommand(_ => LapBaoCao());
        }

        private void UpdateDsThang()
        {
            DsThang.Clear();
            int startMonth = (Nam == _minDate.Year) ? _minDate.Month : 1;
            int endMonth = (Nam == _maxDate.Year) ? _maxDate.Month : 12;

            for (int m = startMonth; m <= endMonth; m++)
            {
                DsThang.Add(m);
            }

            if (Thang < startMonth) Thang = startMonth;
            if (Thang > endMonth) Thang = endMonth;
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
