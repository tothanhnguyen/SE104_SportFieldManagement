using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;
using QuanLySan.Data;
using QuanLySan.ViewModels.Base;

namespace QuanLySan.ViewModels
{
    public class ThayDoiQuyDinhViewModel : BaseViewModel
    {
        private readonly QuyDinhRepository _repo;

        private int _mucDiemMacDinh;
        public int MucDiemMacDinh
        {
            get => _mucDiemMacDinh;
            set { _mucDiemMacDinh = value; OnPropertyChanged(); }
        }

        private string _loaiHoiVienMacDinh = "";
        public string LoaiHoiVienMacDinh
        {
            get => _loaiHoiVienMacDinh;
            set { _loaiHoiVienMacDinh = value; OnPropertyChanged(); }
        }

        private int _soTienQuyDoi;
        public int SoTienQuyDoi
        {
            get => _soTienQuyDoi;
            set { _soTienQuyDoi = value; OnPropertyChanged(); }
        }

        public ObservableCollection<LoaiHoiVienQuyDinh> DanhSachLoaiHoiVien { get; set; } = new();

        public ICommand ThayDoiCommand { get; }
        public ICommand ThemCommand { get; }

        public ThayDoiQuyDinhViewModel()
        {
            _repo = new QuyDinhRepository();

            LoadData();

            ThayDoiCommand = new RelayCommand(_ => ThucHienThayDoi());
            ThemCommand = new RelayCommand(_ =>
            {
                MessageBox.Show("Tính năng thêm loại hội viên sẽ được cập nhật trong sprint tiếp theo.", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
            });
        }

        private void LoadData()
        {
            try
            {
                var thamSo = _repo.LoadThamSoHethong();
                MucDiemMacDinh = thamSo.MucDiemMacDinh;
                LoaiHoiVienMacDinh = thamSo.LoaiHoiVienMacDinh;
                SoTienQuyDoi = thamSo.SoTienQuyDoi;

                var ds = _repo.LoadDanhSachLoaiHoiVien();
                DanhSachLoaiHoiVien.Clear();
                foreach (var item in ds)
                {
                    DanhSachLoaiHoiVien.Add(item);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi tải quy định: " + ex.Message, "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ThucHienThayDoi()
        {
            MessageBox.Show("Tính năng cập nhật quy định đang được thiết kế lại để hỗ trợ lưu lịch sử (Versioning) đảm bảo nghiệp vụ điểm tích luỹ.", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }
}
