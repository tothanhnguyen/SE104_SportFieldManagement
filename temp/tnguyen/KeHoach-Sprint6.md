# Kế hoạch triển khai Sprint 6.1 & 6.2 — Báo cáo doanh thu

> TRẠNG THÁI: Phase 1–4 ✅ DONE (code xong). Phase 6 ⏳ chờ build/test trên Windows (macOS không build được WPF).
> File đã tạo/sửa:
> - Mới: `Models/DoanhThuSanItem.cs`, `Models/DoanhThuKhachHangItem.cs`, `Data/DoanhThuRepository.cs`,
>   `ViewModels/DoanhThuTheoSanViewModel.cs`, `ViewModels/DoanhThuTheoKhachHangViewModel.cs`
> - Sửa: `Views/DoanhThuTheoSan.xaml(.cs)`, `Views/DoanhThuTheoKhachHang.xaml(.cs)`

> Mục tiêu: GIỮ NGUYÊN giao diện đã thiết kế, bắt sự kiện các nút, kết nối database và hoàn thiện
> chức năng cho 2 màn hình **BM6.1 - Báo cáo doanh thu theo sân** và **BM6.2 - Báo cáo doanh thu theo khách hàng**.

## Hiện trạng (kết quả khảo sát)
- App WPF C# (.NET 8, MVVM): `Views` + `ViewModels` (`BaseViewModel`, `RelayCommand`) + `Data` (Repository) + `Services` (`IDialogService`).
- 2 View đã có sẵn nhưng **chỉ là mockup tĩnh**:
  - `Views/DoanhThuTheoSan.xaml` (+ `.xaml.cs`)
  - `Views/DoanhThuTheoKhachHang.xaml` (+ `.xaml.cs`)
  - Năm/Tháng là `TextBlock` tĩnh; nút "Báo cáo doanh thu" **chưa có handler**; ô tổng doanh số là `Border` xám; các cột DataGrid là thanh xám placeholder (chưa binding).
- DB schema `Database/SanTheThao.sql` đã có đủ bảng cần thiết: `DATSAN(TongTien, NgayDat, MaHoiVien, MaChiTiet)` → `CHITIETDATSAN(MaSan)` → `SAN`, và `HOIVIEN`.
  → **KHÔNG cần đổi schema** cho 2 báo cáo này.
- Lưu ý môi trường: máy hiện tại là macOS; WPF (`net8.0-windows`) **không build/chạy được trên macOS** — code viết xong cần build trên Windows.

## Công thức nghiệp vụ (chốt)
**BM6.1 — Doanh thu theo sân (lọc theo Tháng + Năm):**
- Doanh thu mỗi sân = `SUM(DATSAN.TongTien)` của các phiếu thuộc sân đó (qua `CHITIETDATSAN.MaSan`) với `MONTH(NgayDat)=tháng AND YEAR(NgayDat)=năm`.
- Tỷ lệ lấp đầy khung giờ = `(số khung giờ riêng biệt đã đặt trong tháng) / (tổng số khung giờ của sân) × 100%`.
  - Tổng số khung giờ của sân = `COUNT(CHITIETDATSAN WHERE MaSan = sân)`.
  - Số khung giờ đã đặt = `COUNT(DISTINCT MaChiTiet)` trong `DATSAN` của sân đó, `MONTH/YEAR(NgayDat)` khớp.
- Tổng doanh số = tổng doanh thu tất cả sân trong tháng.

**BM6.2 — Doanh thu theo khách hàng (lọc theo Tháng + Năm):**
- Doanh thu mỗi khách = `SUM(DATSAN.TongTien)` theo `MaHoiVien` với `MONTH(NgayDat)=tháng AND YEAR(NgayDat)=năm`.
- Tỷ lệ = `(doanh thu khách / tổng doanh thu tất cả khách) × 100%`.
- Tổng doanh số = tổng doanh thu tất cả khách trong tháng.

## Nguyên tắc "GIỮ NGUYÊN giao diện"
Giữ nguyên bố cục, màu sắc, style (header xanh, nút, viền bo góc, DataGrid). Chỉ thay phần placeholder
bằng control thật để hiển thị dữ liệu:
- Năm/Tháng: `TextBlock` tĩnh → `ComboBox`/`TextBox` cùng style trắng bo góc (mặc định năm/tháng hiện hành).
- Ô tổng doanh số: `Border` xám → `TextBlock` hiển thị số tiền (giữ khung cũ).
- Cột DataGrid: thanh xám → `DataGridTextColumn` binding dữ liệu thật.

---

## Các giai đoạn (Phases)

### Phase 1 — Models (kết quả báo cáo)
- [ ] `Models/DoanhThuSanItem.cs`: `STT, TenSan, DoanhThu, TyLeLapDay` (+ chuỗi hiển thị tiền/%).
- [ ] `Models/DoanhThuKhachHangItem.cs`: `STT, HoTen, DoanhThu, TyLe`.

### Phase 2 — Repository (truy vấn DB)
- [ ] `Data/DoanhThuRepository.cs`:
  - `List<DoanhThuSanItem> BaoCaoTheoSan(int thang, int nam)`
  - `List<DoanhThuKhachHangItem> BaoCaoTheoKhachHang(int thang, int nam)`
  - Dùng `Microsoft.Data.SqlClient`, tham số hóa (chống SQL injection), theo đúng style repo hiện có.

### Phase 3 — ViewModels
- [ ] `ViewModels/DoanhThuTheoSanViewModel.cs`: thuộc tính `Nam, Thang, DsNam, DsThang, TongDoanhThu, KetQua (ObservableCollection)`, `BaoCaoCommand`, `ThoatCommand`; validate tháng/năm; gọi repo; tính tổng.
- [ ] `ViewModels/DoanhThuTheoKhachHangViewModel.cs`: tương tự cho báo cáo theo khách hàng.
- [ ] Kế thừa `BaseViewModel`, dùng `RelayCommand`, `IDialogService` báo lỗi/cảnh báo.

### Phase 4 — Cập nhật View (giữ giao diện)
- [ ] `DoanhThuTheoSan.xaml`: đặt `DataContext`, thay placeholder Năm/Tháng = ComboBox binding, ô tổng = TextBlock binding, cột DataGrid = `DataGridTextColumn` binding (`TenSan, DoanhThuText, TyLeText`); nút "Báo cáo doanh thu" `Command="{Binding BaoCaoCommand}"`.
- [ ] `DoanhThuTheoKhachHang.xaml`: tương tự (`HoTen, DoanhThuText, TyLeText`).
- [ ] `*.xaml.cs`: khởi tạo ViewModel + gán `DataContext`; giữ `DragMove`/đóng cửa sổ; bỏ handler rỗng nếu chuyển sang Command.

### Phase 5 — (ĐÃ BỎ) Dữ liệu mẫu — không thực hiện theo yêu cầu.

### Phase 6 — Build & kiểm thử (trên Windows)
- [ ] Build solution `QuanLySan.sln` trên Windows (`dotnet build`).
- [ ] Chạy thử 2 màn hình: chọn tháng/năm → bấm "Báo cáo doanh thu" → kiểm tra số liệu, tổng, tỷ lệ.
- [ ] `code-reviewer` review code sau khi hoàn thành.

## Tiêu chí hoàn thành (Definition of Done)
- Nút "Báo cáo doanh thu" hoạt động, đổ dữ liệu thật từ DB theo tháng/năm.
- Tổng doanh số + tỷ lệ tính đúng; nút "Thoát"/"✕" hoạt động; kéo cửa sổ vẫn chạy.
- Giao diện giữ nguyên bố cục & style; không lỗi biên dịch.
- Không hardcode dữ liệu; tham số hóa truy vấn.

## Quyết định đã chốt
1. Tỷ lệ lấp đầy = số khung giờ riêng biệt đã đặt / tổng khung giờ của sân × 100%.
2. Năm/Tháng dùng **ComboBox**.
3. **Không** tạo dữ liệu mẫu (Phase 5 bỏ).
