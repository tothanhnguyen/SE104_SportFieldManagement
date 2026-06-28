#nullable enable
using QuanLySan.ViewModels.Base;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace QuanLySan.Models
{
    public class GioSanItem : BaseViewModel
    {
        // Danh sách và Bảng giá sẽ được ViewModel nạp từ SQL Database
        public static ObservableCollection<string> DsLoaiNgay { get; set; } = new();
        public static Dictionary<string, decimal> BangGiaQuyDinh { get; set; } = new();

        private int _stt;
        private string _maChiTiet = "";
        private string _gioBatDau = "";
        private string _gioKetThuc = "";
        private string _loaiNgay = "";
        private decimal _donGia;

        public int STT { get => _stt; set { _stt = value; OnPropertyChanged(); } }
        public string MaChiTiet { get => _maChiTiet; set { _maChiTiet = value; OnPropertyChanged(); } }
        public string GioBatDau { get => _gioBatDau; set { _gioBatDau = value; OnPropertyChanged(); } }
        public string GioKetThuc { get => _gioKetThuc; set { _gioKetThuc = value; OnPropertyChanged(); } }

        // Tự động nhảy đơn giá dựa trên dữ liệu thật từ SQL
        public string LoaiNgay
        {
            get => _loaiNgay;
            set
            {
                _loaiNgay = value;
                OnPropertyChanged();

                // Tra cứu giá từ Dictionary nạp từ SQL
                if (!string.IsNullOrEmpty(value) && BangGiaQuyDinh.TryGetValue(value, out decimal gia))
                {
                    DonGia = gia;
                }
            }
        }

        public decimal DonGia { get => _donGia; set { _donGia = value; OnPropertyChanged(); } }

        private bool _isBooked;
        public bool IsBooked { get => _isBooked; set { _isBooked = value; OnPropertyChanged(); } }
    }
}