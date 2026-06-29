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

        public ICommand XoaCommand { get; }

        public ThayDoiQuyDinhViewModel()
        {
            _repo = new QuyDinhRepository();

            LoadData();

            ThayDoiCommand = new RelayCommand(_ => ThucHienThayDoi());
            ThemCommand = new RelayCommand(_ =>
            {
                MessageBox.Show("Tính năng thêm loại hội viên sẽ được cập nhật trong sprint tiếp theo.", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
            });
            XoaCommand = new RelayCommand(param => ThucHienXoa(param as LoaiHoiVienQuyDinh));
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
            try
            {
                _repo.CapNhatQuyDinh(MucDiemMacDinh, LoaiHoiVienMacDinh, SoTienQuyDoi, DanhSachLoaiHoiVien);
                MessageBox.Show("Cập nhật quy định thành công!", "Thành công", MessageBoxButton.OK, MessageBoxImage.Information);
                LoadData(); // reload
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi khi cập nhật quy định: " + ex.Message, "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ThucHienXoa(LoaiHoiVienQuyDinh hang)
        {
            if (hang == null) return;
            
            try
            {
                int soLuongHoiVien = _repo.KiemTraHoiVienDangDungHang(hang.MaLoaiHoiVien);
                if (soLuongHoiVien > 0)
                {
                    MessageBox.Show($"Không thể xóa hạng này vì đang có {soLuongHoiVien} hội viên sử dụng.", "Cảnh báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                _repo.XoaLoaiHoiVien(hang.MaLoaiHoiVien);
                MessageBox.Show("Xóa hạng hội viên thành công!", "Thành công", MessageBoxButton.OK, MessageBoxImage.Information);
                LoadData();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi khi xóa: " + ex.Message, "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
