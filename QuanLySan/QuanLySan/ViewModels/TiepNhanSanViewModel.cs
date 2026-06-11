#nullable enable
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;
using Microsoft.Data.SqlClient;
using QuanLySan.Models;
using QuanLySan.ViewModels.Base;

namespace QuanLySan.ViewModels
{
    public class TiepNhanSanViewModel : BaseViewModel
    {
        private string _connectionString = DatabaseConfig.ConnectionString;

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

        // Dictionary dùng để ánh xạ Tên sang Mã khi lưu vào CSDL
        private Dictionary<string, string> _mapLoaiSan = new();
        private Dictionary<string, string> _mapTinhTrang = new();
        public static Dictionary<string, string> MapLoaiNgay = new();

        // Commands
        public ICommand ThemGioCommand { get; }
        public ICommand XoaGioCommand { get; }
        public ICommand LuuCommand { get; }
        public ICommand HuyCommand { get; }

        public TiepNhanSanViewModel()
        {
            // Nạp dữ liệu từ SQL ngay khi mở Form
            LoadDanhMucTuDatabase();

            ThemGioCommand = new RelayCommand(_ => {
                DsGioSan.Add(new GioSanItem
                {
                    STT = DsGioSan.Count + 1,
                    GioBatDau = "07:00",
                    GioKetThuc = "08:00",
                    LoaiNgay = GioSanItem.DsLoaiNgay.Count > 0 ? GioSanItem.DsLoaiNgay[0] : ""
                });
            });

            XoaGioCommand = new RelayCommand(p => {
                if (p is GioSanItem item)
                {
                    DsGioSan.Remove(item);
                    for (int i = 0; i < DsGioSan.Count; i++) DsGioSan[i].STT = i + 1;
                }
            });

            LuuCommand = new RelayCommand(_ => ThucHienLuu());
            HuyCommand = new RelayCommand(_ => ThucHienHuy());

            PhatSinhMaSan();
        }

        private void LoadDanhMucTuDatabase()
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(_connectionString))
                {
                    conn.Open();

                    // Load Loại Sân (Bảng LOAISAN)
                    SqlCommand cmdLoaiSan = new SqlCommand("SELECT MaLoaiSan, TenLoaiSan FROM LOAISAN", conn);
                    using (SqlDataReader reader = cmdLoaiSan.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            string ma = reader.GetString(0);
                            string ten = reader.GetString(1);
                            DsLoaiSan.Add(ten);
                            _mapLoaiSan[ten] = ma; // Ánh xạ: "Sân bóng đá" -> "BD"
                        }
                    }

                    // Load Tình Trạng (Bảng TINHTRANG)
                    SqlCommand cmdTinhTrang = new SqlCommand("SELECT MaTinhTrang, TenTinhTrang FROM TINHTRANG", conn);
                    using (SqlDataReader reader = cmdTinhTrang.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            string ma = reader.GetString(0);
                            string ten = reader.GetString(1);
                            DsTinhTrang.Add(ten);
                            _mapTinhTrang[ten] = ma; // Ánh xạ: "Hoạt động" -> "HD"
                        }
                    }

                    // Load Loại Ngày & Đơn giá (Bảng LOAINGAY)
                    SqlCommand cmdLoaiNgay = new SqlCommand("SELECT MaLoaiNgay, TenLoaiNgay, DonGiaNgay FROM LOAINGAY", conn);
                    using (SqlDataReader reader = cmdLoaiNgay.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            string ma = reader.GetString(0);
                            string ten = reader.GetString(1);
                            decimal donGia = reader.GetDecimal(2);

                            if (!GioSanItem.DsLoaiNgay.Contains(ten)) GioSanItem.DsLoaiNgay.Add(ten);
                            GioSanItem.BangGiaQuyDinh[ten] = donGia; // Nạp đơn giá từ SQL
                            MapLoaiNgay[ten] = ma; // Ánh xạ: "Ngày Lễ" -> "NL"
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi kết nối CSDL để nạp danh mục: " + ex.Message);
            }
        }

        // Phát sinh mã sân đúng chuẩn char(6) của database (VD: S24599)
        private void PhatSinhMaSan() => MaSan = "S" + new Random().Next(10000, 99999).ToString();

        private void ThucHienHuy()
        {
            TenSan = ""; DiaChi = ""; GhiChu = "";
            LoaiSanSelected = null;
            TinhTrangSelected = null;
            DsGioSan.Clear();
            PhatSinhMaSan();
        }

        /// <summary>
        /// Parse chuỗi giờ nhập vào thành TimeSpan hợp lệ (00:00 - 23:59).
        /// Hỗ trợ cả format "HH:mm", "H:mm", "HH" hoặc số nguyên (giờ).
        /// </summary>
        private static bool TryParseGio(string input, out TimeSpan result)
        {
            result = TimeSpan.Zero;
            if (string.IsNullOrWhiteSpace(input)) return false;

            input = input.Trim();

            // Nếu chỉ nhập số nguyên (VD: "5" → 05:00)
            if (int.TryParse(input, out int gio) && gio >= 0 && gio <= 23)
            {
                result = new TimeSpan(gio, 0, 0);
                return true;
            }

            // Parse format HH:mm hoặc H:mm
            if (TimeSpan.TryParse(input, out TimeSpan ts) && ts >= TimeSpan.Zero && ts < TimeSpan.FromHours(24))
            {
                result = ts;
                return true;
            }

            return false;
        }

        private void ThucHienLuu()
        {
            if (string.IsNullOrWhiteSpace(TenSan)) { MessageBox.Show("Vui lòng nhập tên sân!"); return; }
            if (string.IsNullOrWhiteSpace(LoaiSanSelected)) { MessageBox.Show("Vui lòng chọn Mã loại sân!"); return; }
            if (string.IsNullOrWhiteSpace(TinhTrangSelected)) { MessageBox.Show("Vui lòng chọn Tình trạng!"); return; }

            // Validate giờ sân trước khi lưu
            for (int i = 0; i < DsGioSan.Count; i++)
            {
                var item = DsGioSan[i];
                if (!TryParseGio(item.GioBatDau, out _))
                {
                    MessageBox.Show($"Dòng {i + 1}: Giờ bắt đầu \"{item.GioBatDau}\" không hợp lệ.\nVui lòng nhập theo định dạng HH:mm (VD: 07:00)",
                        "Sai định dạng", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
                if (!TryParseGio(item.GioKetThuc, out _))
                {
                    MessageBox.Show($"Dòng {i + 1}: Giờ kết thúc \"{item.GioKetThuc}\" không hợp lệ.\nVui lòng nhập theo định dạng HH:mm (VD: 08:00)",
                        "Sai định dạng", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
                if (string.IsNullOrEmpty(item.LoaiNgay))
                {
                    MessageBox.Show($"Dòng {i + 1}: Vui lòng chọn Loại ngày.",
                        "Thiếu thông tin", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
            }

            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                using (SqlTransaction trans = conn.BeginTransaction())
                {
                    try
                    {
                        // Lấy mã (MaLoaiSan, MaTinhTrang) thay vì lấy Tên
                        string maLoaiSan = _mapLoaiSan[LoaiSanSelected];
                        string maTinhTrang = _mapTinhTrang[TinhTrangSelected];

                        // Lưu vào bảng SAN
                        string sqlSan = "INSERT INTO SAN (MaSan, TenSan, DiaChi, GhiChu, MaLoaiSan, MaTinhTrang) VALUES (@Ma, @Ten, @DC, @GC, @MLS, @MTT)";
                        using (SqlCommand cmd = new SqlCommand(sqlSan, conn, trans))
                        {
                            cmd.Parameters.AddWithValue("@Ma", MaSan);
                            cmd.Parameters.AddWithValue("@Ten", TenSan);
                            cmd.Parameters.AddWithValue("@DC", DiaChi ?? "");
                            cmd.Parameters.AddWithValue("@GC", GhiChu ?? "");
                            cmd.Parameters.AddWithValue("@MLS", maLoaiSan);
                            cmd.Parameters.AddWithValue("@MTT", maTinhTrang);
                            cmd.ExecuteNonQuery();
                        }

                        // Lưu vào bảng KHUNGGIO (khung giờ mặc định của sân)
                        foreach (var item in DsGioSan)
                        {
                            string maLoaiNgay = MapLoaiNgay[item.LoaiNgay];
                            TryParseGio(item.GioBatDau, out TimeSpan gioBD);
                            TryParseGio(item.GioKetThuc, out TimeSpan gioKT);

                            string sqlGio = "INSERT INTO KHUNGGIO (MaSan, GioBatDau, GioKetThuc, MaLoaiNgay, DonGia) VALUES (@MaSan, @BD, @KT, @MLN, @Gia)";
                            using (SqlCommand cmd = new SqlCommand(sqlGio, conn, trans))
                            {
                                cmd.Parameters.AddWithValue("@MaSan", MaSan);
                                cmd.Parameters.AddWithValue("@BD", gioBD);
                                cmd.Parameters.AddWithValue("@KT", gioKT);
                                cmd.Parameters.AddWithValue("@MLN", maLoaiNgay);
                                cmd.Parameters.AddWithValue("@Gia", item.DonGia);
                                cmd.ExecuteNonQuery();
                            }
                        }

                        trans.Commit();
                        MessageBox.Show("Lưu thành công dữ liệu xuống cơ sở dữ liệu!");
                        ThucHienHuy(); // Tự động dọn form sau khi lưu thành công
                    }
                    catch (Exception ex)
                    {
                        trans.Rollback();
                        MessageBox.Show("Lỗi lưu dữ liệu: " + ex.Message);
                    }
                }
            }
        }
    }
}