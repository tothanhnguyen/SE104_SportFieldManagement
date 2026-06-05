#nullable enable
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Windows;
using System.Windows.Input;
using Microsoft.Data.SqlClient;
using QuanLySan.Models;
using QuanLySan.ViewModels.Base;

namespace QuanLySan.ViewModels
{
    // Màn hình Đặt sân (Sprint 4). Áp dụng Quy định 4:
    //  - Không cho đặt sân đang bảo trì.
    //  - Các khung giờ đặt không được trùng với khung giờ đã được đặt.
    public class DatSanViewModel : BaseViewModel
    {
        private readonly string _connectionString = DatabaseConfig.ConnectionString;
        private const string MA_TINHTRANG_BAOTRI = "BT";

        // ── Thông tin phiếu đặt (BM4) ──
        private string _maPhieuDat = "";
        public string MaPhieuDat { get => _maPhieuDat; set { _maPhieuDat = value; OnPropertyChanged(); } }

        private San? _sanSelected;
        public San? SanSelected { get => _sanSelected; set { _sanSelected = value; OnPropertyChanged(); } }

        private HoiVien? _hoiVienSelected;
        public HoiVien? HoiVienSelected { get => _hoiVienSelected; set { _hoiVienSelected = value; OnPropertyChanged(); } }

        private DateTime? _ngayDat;
        public DateTime? NgayDat { get => _ngayDat; set { _ngayDat = value; OnPropertyChanged(); } }

        private string _ghiChu = "";
        public string GhiChu { get => _ghiChu; set { _ghiChu = value; OnPropertyChanged(); } }

        private decimal _tongTien;
        public decimal TongTien { get => _tongTien; set { _tongTien = value; OnPropertyChanged(); } }

        // ── Danh sách hiển thị ──
        public ObservableCollection<San> DsSan { get; } = new();
        public ObservableCollection<HoiVien> DsHoiVien { get; } = new();
        public ObservableCollection<GioSanItem> DsGioDat { get; } = new();

        // ── Commands ──
        public ICommand ThemGioCommand { get; }
        public ICommand XoaGioCommand { get; }
        public ICommand DatSanCommand { get; }
        public ICommand HuyCommand { get; }

        public DatSanViewModel()
        {
            LoadDanhMucTuDatabase();

            // Tự tính lại tổng tiền khi danh sách giờ thay đổi
            DsGioDat.CollectionChanged += DsGioDat_CollectionChanged;

            ThemGioCommand = new RelayCommand(_ =>
            {
                DsGioDat.Add(new GioSanItem
                {
                    STT = DsGioDat.Count + 1,
                    GioBatDau = "07:00",
                    GioKetThuc = "08:00",
                    LoaiNgay = GioSanItem.DsLoaiNgay.Count > 0 ? GioSanItem.DsLoaiNgay[0] : ""
                });
            });

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
            PhatSinhMaPhieuDat();
        }

        // ===================== NẠP DỮ LIỆU =====================

        private void LoadDanhMucTuDatabase()
        {
            try
            {
                using var conn = new SqlConnection(_connectionString);
                conn.Open();

                // Danh sách sân (kèm tình trạng để chặn sân bảo trì)
                using (var cmd = new SqlCommand("SELECT MaSan, TenSan, MaTinhTrang FROM SAN", conn))
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        DsSan.Add(new San
                        {
                            MaSan = reader["MaSan"].ToString() ?? "",
                            TenSan = reader["TenSan"].ToString() ?? "",
                            MaTinhTrang = reader["MaTinhTrang"].ToString() ?? ""
                        });
                    }
                }

                // Danh sách hội viên
                using (var cmd = new SqlCommand("SELECT MaHoiVien, HoTen FROM HOIVIEN", conn))
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        DsHoiVien.Add(new HoiVien
                        {
                            MaHoiVien = reader["MaHoiVien"].ToString() ?? "",
                            HoTen = reader["HoTen"].ToString() ?? ""
                        });
                    }
                }

                // Loại ngày & đơn giá (nạp vào dữ liệu dùng chung của GioSanItem)
                using (var cmd = new SqlCommand("SELECT MaLoaiNgay, TenLoaiNgay, DonGiaNgay FROM LOAINGAY", conn))
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        string ma = reader.GetString(0);
                        string ten = reader.GetString(1);
                        decimal donGia = reader.GetDecimal(2);

                        if (!GioSanItem.DsLoaiNgay.Contains(ten)) GioSanItem.DsLoaiNgay.Add(ten);
                        GioSanItem.BangGiaQuyDinh[ten] = donGia;
                        TiepNhanSanViewModel.MapLoaiNgay[ten] = ma; // tái dùng ánh xạ Tên -> Mã loại ngày
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi kết nối CSDL để nạp danh mục: " + ex.Message);
            }
        }

        private void PhatSinhMaPhieuDat() => MaPhieuDat = "DS" + new Random().Next(10000, 99999).ToString();

        // ===================== TÍNH TỔNG TIỀN =====================

        private void DsGioDat_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            // Lắng nghe thay đổi từng dòng (đổi giờ / loại ngày) để cập nhật tổng tiền
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
            if (SanSelected == null) { MessageBox.Show("Vui lòng chọn sân!"); return; }
            if (HoiVienSelected == null) { MessageBox.Show("Vui lòng chọn hội viên!"); return; }
            if (NgayDat == null) { MessageBox.Show("Vui lòng chọn ngày đặt!"); return; }
            if (DsGioDat.Count == 0) { MessageBox.Show("Vui lòng thêm ít nhất một khung giờ đặt!"); return; }

            // 2. Quy định 4: không cho đặt sân đang bảo trì
            if (SanSelected.MaTinhTrang == MA_TINHTRANG_BAOTRI)
            {
                MessageBox.Show("Sân này đang bảo trì, không thể đặt!", "Không hợp lệ", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // 3. Validate & chuẩn hóa từng khung giờ
            var khungGio = new List<(TimeSpan bd, TimeSpan kt)>();
            for (int i = 0; i < DsGioDat.Count; i++)
            {
                var item = DsGioDat[i];
                if (!TryParseGio(item.GioBatDau, out TimeSpan bd) || !TryParseGio(item.GioKetThuc, out TimeSpan kt))
                {
                    MessageBox.Show($"Dòng {i + 1}: Giờ không hợp lệ (định dạng HH:mm, VD 07:00).", "Sai định dạng", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
                if (kt <= bd)
                {
                    MessageBox.Show($"Dòng {i + 1}: Giờ kết thúc phải lớn hơn giờ bắt đầu.", "Không hợp lệ", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
                if (string.IsNullOrEmpty(item.LoaiNgay))
                {
                    MessageBox.Show($"Dòng {i + 1}: Vui lòng chọn loại ngày.", "Thiếu thông tin", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
                khungGio.Add((bd, kt));
            }

            // 4. Quy định 4: các khung giờ trong cùng phiếu không được trùng nhau
            for (int i = 0; i < khungGio.Count; i++)
                for (int j = i + 1; j < khungGio.Count; j++)
                    if (BiTrung(khungGio[i].bd, khungGio[i].kt, khungGio[j].bd, khungGio[j].kt))
                    {
                        MessageBox.Show($"Khung giờ dòng {i + 1} và dòng {j + 1} bị trùng nhau!", "Trùng giờ", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }

            using var connection = new SqlConnection(_connectionString);
            connection.Open();
            using var trans = connection.BeginTransaction();
            try
            {
                DateTime ngay = NgayDat.Value.Date;

                // 5. Quy định 4: kiểm tra trùng với các khung giờ đã đặt trước đó (cùng sân, cùng ngày)
                for (int i = 0; i < DsGioDat.Count; i++)
                {
                    if (DemKhungGioTrung(connection, trans, SanSelected.MaSan, ngay, khungGio[i].bd, khungGio[i].kt) > 0)
                    {
                        trans.Rollback();
                        MessageBox.Show($"Dòng {i + 1} ({DsGioDat[i].GioBatDau}-{DsGioDat[i].GioKetThuc}) trùng với khung giờ đã được đặt!", "Trùng giờ", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }
                }

                // 6. Lưu phiếu đặt
                string sqlPhieu = @"INSERT INTO PHIEUDATSAN (MaPhieuDat, MaSan, MaHoiVien, NgayDat, TongTien, GhiChu)
                                    VALUES (@Ma, @MaSan, @MaHV, @Ngay, @Tong, @GC)";
                using (var cmd = new SqlCommand(sqlPhieu, connection, trans))
                {
                    cmd.Parameters.AddWithValue("@Ma", MaPhieuDat);
                    cmd.Parameters.AddWithValue("@MaSan", SanSelected.MaSan);
                    cmd.Parameters.AddWithValue("@MaHV", HoiVienSelected.MaHoiVien);
                    cmd.Parameters.AddWithValue("@Ngay", ngay);
                    cmd.Parameters.AddWithValue("@Tong", TongTien);
                    cmd.Parameters.AddWithValue("@GC", GhiChu ?? "");
                    cmd.ExecuteNonQuery();
                }

                // 7. Lưu chi tiết các khung giờ
                for (int i = 0; i < DsGioDat.Count; i++)
                {
                    var item = DsGioDat[i];
                    string maLoaiNgay = TiepNhanSanViewModel.MapLoaiNgay[item.LoaiNgay];
                    string sqlCt = @"INSERT INTO CHITIETPHIEUDAT (MaPhieuDat, GioBatDau, GioKetThuc, MaLoaiNgay, DonGia)
                                     VALUES (@Ma, @BD, @KT, @MLN, @Gia)";
                    using var cmd = new SqlCommand(sqlCt, connection, trans);
                    cmd.Parameters.AddWithValue("@Ma", MaPhieuDat);
                    cmd.Parameters.AddWithValue("@BD", khungGio[i].bd);
                    cmd.Parameters.AddWithValue("@KT", khungGio[i].kt);
                    cmd.Parameters.AddWithValue("@MLN", maLoaiNgay);
                    cmd.Parameters.AddWithValue("@Gia", item.DonGia);
                    cmd.ExecuteNonQuery();
                }

                trans.Commit();
                MessageBox.Show($"Đặt sân thành công!\nMã phiếu: {MaPhieuDat}\nTổng tiền: {TongTien:N0} VNĐ", "Thành công", MessageBoxButton.OK, MessageBoxImage.Information);
                ThucHienHuy();
            }
            catch (Exception ex)
            {
                trans.Rollback();
                MessageBox.Show("Lỗi lưu phiếu đặt: " + ex.Message, "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // Đếm số khung giờ đã đặt bị trùng (cùng sân, cùng ngày) với khoảng [bd, kt)
        private int DemKhungGioTrung(SqlConnection conn, SqlTransaction trans, string maSan, DateTime ngay, TimeSpan bd, TimeSpan kt)
        {
            string sql = @"SELECT COUNT(*)
                           FROM CHITIETPHIEUDAT ct
                           JOIN PHIEUDATSAN p ON ct.MaPhieuDat = p.MaPhieuDat
                           WHERE p.MaSan = @MaSan AND p.NgayDat = @Ngay
                                 AND ct.GioBatDau < @KT AND @BD < ct.GioKetThuc";
            using var cmd = new SqlCommand(sql, conn, trans);
            cmd.Parameters.AddWithValue("@MaSan", maSan);
            cmd.Parameters.AddWithValue("@Ngay", ngay);
            cmd.Parameters.AddWithValue("@BD", bd);
            cmd.Parameters.AddWithValue("@KT", kt);
            return Convert.ToInt32(cmd.ExecuteScalar());
        }

        private void ThucHienHuy()
        {
            SanSelected = null;
            HoiVienSelected = null;
            GhiChu = "";
            NgayDat = DateTime.Now;
            DsGioDat.Clear();
            TongTien = 0;
            PhatSinhMaPhieuDat();
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
