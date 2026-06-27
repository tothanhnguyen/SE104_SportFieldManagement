# Hướng dẫn Test — Sprint 5 (BM5) & Sprint 6 (BM6.1, BM6.2)

Tài liệu chỉ tập trung **flow test**. Chạy trên **Windows** (WPF không chạy trên macOS).

## 0. Yêu cầu
- Windows + .NET 8 SDK + SQL Server (Express).
- Mở `QuanLySan/QuanLySan.sln` bằng Visual Studio (hoặc `dotnet build`).
- Sửa chuỗi kết nối tại `QuanLySan/QuanLySan/Models/DatabaseConfig.cs` cho đúng SQL Server của bạn.

## 1. Tạo database + dữ liệu mẫu
Chạy **1 file duy nhất** trong SSMS: `Database/SanTheThao.sql`
→ Tự tạo DB `QLSanTheThao`, danh mục, bảng `TICHDIEM` (HeSoTichDiem=100000) và dữ liệu mẫu.

Dữ liệu mẫu chính:

| Hội viên | Hạng | Giảm giá | Điểm ban đầu |
|----------|------|----------|--------------|
| HV01 – Nguyễn Văn An | Đồng | 0% | 0 |
| HV02 – Trần Thị Bình | Bạc | 3% | 100 |
| HV03 – Lê Văn Cường | Vàng | 5% | 200 |

Phiếu đặt sân (tháng 6/2026): DS01..DS07 (mỗi phiếu 1 khung giờ).

## 2. Chọn màn hình để test
Chạy app → mở **Menu chính** (`MainWindow`) có **sidebar bên trái**.
Bấm nút tương ứng để mở từng biểu mẫu; đóng biểu mẫu sẽ quay lại menu để chọn cái khác:
- **Thanh toán đặt sân** — BM5
- **Báo cáo DT theo sân** — BM6.1
- **Báo cáo DT theo khách hàng** — BM6.2
- (kèm: Tiếp nhận sân, Đăng ký/Tra cứu hội viên, Đặt sân)

---

## 3. Test BM5 — Thanh toán đặt sân (Quy định 5)
**Flow:** chọn **Mã đặt sân** → tự điền Tên sân, Tổng tiền, Hội viên → **Giảm giá tự tính theo hạng** → **Số tiền phải trả** = Tổng tiền − Giảm giá → bấm **Thanh toán** → cộng điểm cho hội viên.

Công thức điểm: `điểm = floor(Số tiền phải trả / 100000)`.

| Chọn phiếu | Hội viên (hạng) | Tổng tiền | Giảm giá | Phải trả | Điểm +  |
|-----------|------------------|-----------|----------|----------|---------|
| DS06 | HV01 (Đồng 0%) | 100.000 | 0 | 100.000 | **+1** |
| DS03 | HV02 (Bạc 3%) | 50.000 | 1.500 | 48.500 | +0 |
| DS05 | HV03 (Vàng 5%) | 50.000 | 2.500 | 47.500 | +0 |

**Kiểm tra đã cộng điểm:** chạy query
```sql
SELECT MaHoiVien, HoTen, DiemTichLuy FROM HOIVIEN ORDER BY MaHoiVien;
```
Sau khi thanh toán DS06 → HV01 `DiemTichLuy` từ 0 → 1.

> Lưu ý: không có bảng lưu hóa đơn → thanh toán chỉ cộng điểm (không chặn thanh toán lại 1 phiếu).

**Cần kiểm:** chọn thiếu mã phiếu / mã hội viên → hiện cảnh báo; nút Hủy xóa form; nút Thoát/✕ đóng cửa sổ.

---

## 4. Test BM6.1 — Báo cáo doanh thu theo sân
**Flow:** chọn **Năm = 2026**, **Tháng = 6** → bấm **Báo cáo doanh thu**.

Kết quả mong đợi:

| Tên sân | Doanh thu | Tỷ lệ lấp đầy |
|---------|-----------|---------------|
| Sân bóng đá A | 150.000đ | 50% (2/4 khung) |
| Sân cầu lông B | 50.000đ | 33,33% (1/3) |
| Sân pickleball C | 200.000đ | 100% (2/2) |

Tổng doanh số tháng = **400.000đ**.

---

## 5. Test BM6.2 — Báo cáo doanh thu theo khách hàng
**Flow:** chọn **Năm = 2026**, **Tháng = 6** → bấm **Báo cáo doanh thu**.

Kết quả mong đợi:

| Họ tên | Doanh thu | Tỷ lệ |
|--------|-----------|-------|
| Nguyễn Văn An | 200.000đ | 50% |
| Trần Thị Bình | 100.000đ | 25% |
| Lê Văn Cường | 100.000đ | 25% |

Tổng doanh số tháng = **400.000đ**.

> Nếu vừa test BM5 ở mục 3 (thanh toán) thì điểm hội viên thay đổi nhưng **doanh thu không đổi** (doanh thu lấy từ `DATSAN.TongTien`).

---

## 6. Checklist nhanh
- [ ] `SanTheThao.sql` chạy không lỗi.
- [ ] BM5: giảm giá đúng theo hạng, điểm cộng đúng công thức.
- [ ] BM6.1 / BM6.2: số liệu khớp bảng trên; nút Thoát/kéo cửa sổ chạy.
- [ ] Báo cáo tháng khác (vd tháng 5) → bảng rỗng, tổng = 0đ.
