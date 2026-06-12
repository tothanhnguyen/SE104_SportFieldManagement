#nullable enable
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
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
        private string _maDatSan = "";
        public string MaDatSan { get => _maDatSan; set { _maDatSan = value; OnPropertyChanged(); } }

        private San? _sanSelected;
        public San? SanSelected 
        { 
            get => _sanSelected; 
            set 
            { 
                _sanSelected = value; 
                OnPropertyChanged();
                // Khi chọn sân mới, xóa danh sách giờ cũ
                DsGioDat.Clear();
                TongTien = 0;
                _chiTietCounter = 0;
            } 
        }

        private HoiVien? _hoiVienSelected;
        public HoiVien? HoiVienSelected 
        { 
            get => _hoiVienSelected; 
            set 
            { 
                _hoiVienSelected = value; 
                OnPropertyChanged();
                OnPropertyChanged(nameof(TenHoiVienHienThi));
            } 
        }

        // Tên hội viên hiển thị (read-only, tự cập nhật khi HoiVienSelected thay đổi)
        public string TenHoiVienHienThi => HoiVienSelected?.HoTen ?? "";

        private string _maHoiVien = "";
        public string MaHoiVien 
        { 
            get => _maHoiVien; 
            set 
            { 
                _maHoiVien = value; 
                OnPropertyChanged();
                // Auto-populate Tên hội viên when Mã hội viên changes
                if (!string.IsNullOrEmpty(value))
                {
                    var hv = DsHoiVien.FirstOrDefault(h => h.MaHoiVien == value);
                    if (hv != null)
                    {
                        _hoiVienSelected = hv;
                        OnPropertyChanged(nameof(HoiVienSelected));
                        OnPropertyChanged(nameof(TenHoiVienHienThi));
                    }
                    else
                    {
                        _hoiVienSelected = null;
                        OnPropertyChanged(nameof(HoiVienSelected));
                        OnPropertyChanged(nameof(TenHoiVienHienThi));
                    }
                }
                else
                {
                    _hoiVienSelected = null;
                    OnPropertyChanged(nameof(HoiVienSelected));
                    OnPropertyChanged(nameof(TenHoiVienHienThi));
                }
            } 
        }

        private DateTime? _ngayDat;
        public DateTime? NgayDat { get => _ngayDat; set { _ngayDat = value; OnPropertyChanged(); } }

        private string _ghiChu = "";
        public string GhiChu { get => _ghiChu; set { _ghiChu = value; OnPropertyChanged(); } }

        private decimal _tongTien;
        public decimal TongTien { get => _tongTien; set { _tongTien = value; OnPropertyChanged(); } }

        // Mã chi tiết đặt sân hiển thị (read-only, hiển thị mã chi tiết đang chọn trong DataGrid)
        private string _maChiTietHienThi = "";
        public string MaChiTietHienThi { get => _maChiTietHienThi; set { _maChiTietHienThi = value; OnPropertyChanged(); } }

        // Mã chi tiết đặt sân nhập bởi người dùng (editable)
        private string _maChiTietInput = "";
        public string MaChiTietInput { get => _maChiTietInput; set { _maChiTietInput = value; OnPropertyChanged(); } }

        // Bộ đếm mã chi tiết (tự tăng cho mỗi phiếu đặt)
        private int _chiTietCounter = 0;

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

            ThemGioCommand = new RelayCommand(_ => ThucHienThemGio());
            XoaGioCommand = new RelayCommand(p =>
            {
                if (p is GioSanItem item)
                {
                    DsGioDat.Remove(item);
                    // Cập nhật lại STT
                    for (int i = 0; i < DsGioDat.Count; i++) DsGioDat[i].STT = i + 1;
                    TinhTongTien();
                }
            });

            DatSanCommand = new RelayCommand(_ => ThucHienDatSan());
            HuyCommand = new RelayCommand(_ => ThucHienHuy());

            NgayDat = DateTime.Now;
            PhatSinhMaDatSan();
        }

        // ===================== THÊM GIỜ =====================
        // Tra bảng KHUNGGIO theo MaSan đã chọn, lấy các khung giờ có sẵn → thêm vào DataGrid

        private void ThucHienThemGio()
        {
            // Validate: phải chọn sân trước
            if (SanSelected == null)
            {
                MessageBox.Show("Vui lòng chọn mã sân trước!",
                    "Thiếu thông tin", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Quy định 4: không cho đặt sân đang bảo trì
            if (SanSelected.MaTinhTrang == MA_TINHTRANG_BAOTRI)
            {
                MessageBox.Show("Sân này đang bảo trì, không thể đặt!",
                    "Không hợp lệ", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Nếu có nhập Mã chi tiết đặt sân → tra cứu chi tiết từ CSDL
            if (!string.IsNullOrWhiteSpace(MaChiTietInput))
            {
                TraCuuChiTietDatSan();
                return;
            }

            // Chưa nhập Mã chi tiết → yêu cầu nhập
            MessageBox.Show("Vui lòng nhập Mã chi tiết đặt sân để tra cứu!",
                "Thiếu thông tin", MessageBoxButton.OK, MessageBoxImage.Warning);
        }

        /// <summary>
        /// Tra cứu chi tiết đặt sân từ bảng CHITIETDATSAN theo MaChiTietInput và MaSan đã chọn.
        /// Hiển thị thông tin chi tiết (giờ bắt đầu, kết thúc, loại ngày, đơn giá) lên DataGrid.
        /// Đồng thời nạp thông tin phiếu đặt (MaDatSan, NgayDat, MaHoiVien, TenHoiVien, GhiChu, TongTien).
        /// </summary>
        private void TraCuuChiTietDatSan()
        {
            try
            {
                using var conn = new SqlConnection(_connectionString);
                conn.Open();

                // Truy vấn chi tiết đặt sân kèm thông tin phiếu đặt và hội viên
                string sql = @"SELECT ct.MaChiTiet, ct.GioBatDau, ct.GioKetThuc, ct.DonGia,
                                      ln.TenLoaiNgay,
                                      d.MaDatSan, d.NgayDat, d.TongTien, d.GhiChu,
                                      hv.MaHoiVien, hv.HoTen
                               FROM CHITIETDATSAN ct
                               JOIN DATSAN d ON ct.MaDatSan = d.MaDatSan
                               JOIN HOIVIEN hv ON d.MaHoiVien = hv.MaHoiVien
                               LEFT JOIN LOAINGAY ln ON ct.MaLoaiNgay = ln.MaLoaiNgay
                               WHERE ct.MaChiTiet = @MaChiTiet AND ct.MaSan = @MaSan";

                using var cmd = new SqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@MaChiTiet", MaChiTietInput.Trim());
                cmd.Parameters.AddWithValue("@MaSan", SanSelected!.MaSan);

                using var reader = cmd.ExecuteReader();

                if (reader.Read())
                {
                    // Nạp thông tin phiếu đặt sân
                    string maDatSanDB = reader["MaDatSan"]?.ToString() ?? "";
                    DateTime ngayDatDB = reader.GetDateTime(reader.GetOrdinal("NgayDat"));
                    decimal tongTienDB = reader.IsDBNull(reader.GetOrdinal("TongTien")) ? 0 : reader.GetDecimal(reader.GetOrdinal("TongTien"));
                    string ghiChuDB = reader["GhiChu"]?.ToString() ?? "";
                    string maHvDB = reader["MaHoiVien"]?.ToString() ?? "";
                    string tenHvDB = reader["HoTen"]?.ToString() ?? "";

                    // Cập nhật thông tin phiếu lên form
                    MaDatSan = maDatSanDB;
                    NgayDat = ngayDatDB;
                    TongTien = tongTienDB;
                    GhiChu = ghiChuDB;

                    // Cập nhật hội viên
                    _maHoiVien = maHvDB;
                    OnPropertyChanged(nameof(MaHoiVien));
                    _hoiVienSelected = DsHoiVien.FirstOrDefault(h => h.MaHoiVien == maHvDB);
                    if (_hoiVienSelected == null)
                    {
                        // Nếu hội viên chưa có trong danh sách, tạo tạm để hiển thị
                        _hoiVienSelected = new HoiVien { MaHoiVien = maHvDB, HoTen = tenHvDB };
                    }
                    OnPropertyChanged(nameof(HoiVienSelected));
                    OnPropertyChanged(nameof(TenHoiVienHienThi));

                    // Nạp chi tiết đặt sân vào DataGrid
                    string maChiTiet = reader["MaChiTiet"]?.ToString() ?? "";
                    TimeSpan gioBD = reader.GetTimeSpan(reader.GetOrdinal("GioBatDau"));
                    TimeSpan gioKT = reader.GetTimeSpan(reader.GetOrdinal("GioKetThuc"));
                    decimal donGia = reader.IsDBNull(reader.GetOrdinal("DonGia")) ? 0 : reader.GetDecimal(reader.GetOrdinal("DonGia"));
                    string loaiNgay = reader.IsDBNull(reader.GetOrdinal("TenLoaiNgay")) ? "" : reader.GetString(reader.GetOrdinal("TenLoaiNgay"));

                    // Xóa danh sách cũ và thêm chi tiết tra cứu được
                    DsGioDat.Clear();
                    DsGioDat.Add(new GioSanItem
                    {
                        STT = 1,
                        MaChiTiet = maChiTiet,
                        GioBatDau = gioBD.ToString(@"hh\:mm"),
                        GioKetThuc = gioKT.ToString(@"hh\:mm"),
                        LoaiNgay = loaiNgay,
                        DonGia = donGia
                    });

                    // Đọc thêm các chi tiết khác cùng phiếu đặt (nếu có)
                    // Đóng reader hiện tại trước
                    reader.Close();

                    // Truy vấn thêm các chi tiết cùng MaDatSan và MaSan (ngoại trừ chi tiết đã nạp)
                    string sqlOther = @"SELECT ct.MaChiTiet, ct.GioBatDau, ct.GioKetThuc, ct.DonGia, ln.TenLoaiNgay
                                        FROM CHITIETDATSAN ct
                                        LEFT JOIN LOAINGAY ln ON ct.MaLoaiNgay = ln.MaLoaiNgay
                                        WHERE ct.MaDatSan = @MaDatSan AND ct.MaSan = @MaSan AND ct.MaChiTiet <> @MaChiTiet
                                        ORDER BY ct.GioBatDau";
                    using var cmdOther = new SqlCommand(sqlOther, conn);
                    cmdOther.Parameters.AddWithValue("@MaDatSan", maDatSanDB);
                    cmdOther.Parameters.AddWithValue("@MaSan", SanSelected!.MaSan);
                    cmdOther.Parameters.AddWithValue("@MaChiTiet", MaChiTietInput.Trim());

                    using var readerOther = cmdOther.ExecuteReader();
                    while (readerOther.Read())
                    {
                        DsGioDat.Add(new GioSanItem
                        {
                            STT = DsGioDat.Count + 1,
                            MaChiTiet = readerOther["MaChiTiet"]?.ToString() ?? "",
                            GioBatDau = readerOther.GetTimeSpan(readerOther.GetOrdinal("GioBatDau")).ToString(@"hh\:mm"),
                            GioKetThuc = readerOther.GetTimeSpan(readerOther.GetOrdinal("GioKetThuc")).ToString(@"hh\:mm"),
                            LoaiNgay = readerOther.IsDBNull(readerOther.GetOrdinal("TenLoaiNgay")) ? "" : readerOther.GetString(readerOther.GetOrdinal("TenLoaiNgay")),
                            DonGia = readerOther.IsDBNull(readerOther.GetOrdinal("DonGia")) ? 0 : readerOther.GetDecimal(readerOther.GetOrdinal("DonGia"))
                        });
                    }

                    MaChiTietHienThi = maChiTiet;
                    TinhTongTien();
                }
                else
                {
                    MessageBox.Show($"Không tìm thấy chi tiết đặt sân với mã \"{MaChiTietInput}\" trên sân \"{SanSelected!.MaSan}\".",
                        "Không tìm thấy", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi tra cứu chi tiết đặt sân: " + ex.Message, "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
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

        private void PhatSinhMaDatSan() => MaDatSan = "DS" + new Random().Next(10000, 99999).ToString();

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
            if (SanSelected == null) { MessageBox.Show("Vui lòng chọn mã sân!"); return; }
            if (string.IsNullOrEmpty(MaHoiVien) || HoiVienSelected == null) { MessageBox.Show("Vui lòng nhập mã hội viên hợp lệ!"); return; }
            if (NgayDat == null) { MessageBox.Show("Vui lòng chọn ngày đặt!"); return; }
            if (DsGioDat.Count == 0) { MessageBox.Show("Vui lòng thêm ít nhất một khung giờ đặt!"); return; }

            // 2. Quy định 4: không cho đặt sân đang bảo trì
            if (SanSelected.MaTinhTrang == MA_TINHTRANG_BAOTRI)
            {
                MessageBox.Show("Sân này đang bảo trì, không thể đặt!", "Không hợp lệ", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // 3. Chuẩn hóa từng khung giờ
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
                string maSanCheck = SanSelected.MaSan;
                for (int i = 0; i < DsGioDat.Count; i++)
                {
                    if (!string.IsNullOrEmpty(maSanCheck) && DemKhungGioTrung(connection, trans, maSanCheck, ngay, khungGio[i].bd, khungGio[i].kt) > 0)
                    {
                        trans.Rollback();
                        MessageBox.Show($"Dòng {i + 1} ({DsGioDat[i].GioBatDau}-{DsGioDat[i].GioKetThuc}) trùng với khung giờ đã được đặt!", "Trùng giờ", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }
                }

                // 6. Lưu phiếu đặt vào bảng DATSAN
                string sqlDatSan = @"INSERT INTO DATSAN (MaDatSan, MaHoiVien, NgayDat, TongTien, GhiChu)
                                     VALUES (@Ma, @MaHV, @Ngay, @Tong, @GC)";
                using (var cmd = new SqlCommand(sqlDatSan, connection, trans))
                {
                    cmd.Parameters.AddWithValue("@Ma", MaDatSan);
                    cmd.Parameters.AddWithValue("@MaHV", HoiVienSelected.MaHoiVien);
                    cmd.Parameters.AddWithValue("@Ngay", ngay);
                    cmd.Parameters.AddWithValue("@Tong", TongTien);
                    cmd.Parameters.AddWithValue("@GC", GhiChu ?? "");
                    cmd.ExecuteNonQuery();
                }

                // 7. Lưu chi tiết các khung giờ vào bảng CHITIETDATSAN
                string maSanInsert = SanSelected.MaSan;
                for (int i = 0; i < DsGioDat.Count; i++)
                {
                    var item = DsGioDat[i];
                    string maLoaiNgay = TiepNhanSanViewModel.MapLoaiNgay[item.LoaiNgay];
                    string sqlCt = @"INSERT INTO CHITIETDATSAN (MaChiTiet, MaDatSan, MaSan, GioBatDau, GioKetThuc, MaLoaiNgay, DonGia)
                                     VALUES (@MaCT, @Ma, @MaSan, @BD, @KT, @MLN, @Gia)";
                    using var cmd = new SqlCommand(sqlCt, connection, trans);
                    cmd.Parameters.AddWithValue("@MaCT", item.MaChiTiet);
                    cmd.Parameters.AddWithValue("@Ma", MaDatSan);
                    cmd.Parameters.AddWithValue("@MaSan", maSanInsert);
                    cmd.Parameters.AddWithValue("@BD", khungGio[i].bd);
                    cmd.Parameters.AddWithValue("@KT", khungGio[i].kt);
                    cmd.Parameters.AddWithValue("@MLN", maLoaiNgay);
                    cmd.Parameters.AddWithValue("@Gia", item.DonGia);
                    cmd.ExecuteNonQuery();
                }

                trans.Commit();
                MessageBox.Show($"Đặt sân thành công!\nMã đặt sân: {MaDatSan}\nTổng tiền: {TongTien:N0} VNĐ", "Thành công", MessageBoxButton.OK, MessageBoxImage.Information);
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
                           FROM CHITIETDATSAN ct
                           JOIN DATSAN d ON ct.MaDatSan = d.MaDatSan
                           WHERE ct.MaSan = @MaSan AND d.NgayDat = @Ngay
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
            _hoiVienSelected = null;
            OnPropertyChanged(nameof(HoiVienSelected));
            OnPropertyChanged(nameof(TenHoiVienHienThi));
            _maHoiVien = "";
            OnPropertyChanged(nameof(MaHoiVien));
            GhiChu = "";
            NgayDat = DateTime.Now;
            DsGioDat.Clear();
            TongTien = 0;
            _chiTietCounter = 0;
            MaChiTietHienThi = "";
            MaChiTietInput = "";
            PhatSinhMaDatSan();
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
