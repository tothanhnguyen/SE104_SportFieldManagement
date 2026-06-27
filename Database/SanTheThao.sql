USE master;
GO

-- 1. Kiểm tra và xóa database nếu đã tồn tại
IF EXISTS (SELECT * FROM sys.databases WHERE name = 'QLSanTheThao')
BEGIN
    -- Ngắt các kết nối đang hiện hành để có thể xóa database
    ALTER DATABASE QLSanTheThao SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
    DROP DATABASE QLSanTheThao;
END
GO

-- 2. Tạo mới Database
CREATE DATABASE QLSanTheThao;
GO

USE QLSanTheThao;
GO

-- ═══════════════════════════════════════════════════════
-- 3. Bảng Loại Sân
-- ═══════════════════════════════════════════════════════
CREATE TABLE LOAISAN (
    MaLoaiSan CHAR(2) PRIMARY KEY, -- BD, CL, PB
    TenLoaiSan NVARCHAR(100) NOT NULL
);

-- ═══════════════════════════════════════════════════════
-- 4. Bảng Tình Trạng
-- ═══════════════════════════════════════════════════════
CREATE TABLE TINHTRANG (
    MaTinhTrang CHAR(2) PRIMARY KEY, -- HD, BT
    TenTinhTrang NVARCHAR(100) NOT NULL
);

-- ═══════════════════════════════════════════════════════
-- 5. Bảng Loại Ngày (+ Đơn giá theo loại ngày – Quy định 1)
-- ═══════════════════════════════════════════════════════
CREATE TABLE LOAINGAY (
    MaLoaiNgay CHAR(2) PRIMARY KEY, -- NT, CT, NL
    TenLoaiNgay NVARCHAR(100) NOT NULL,
    DonGiaNgay MONEY NOT NULL
);

-- ═══════════════════════════════════════════════════════
-- 6. Bảng Sân
-- ═══════════════════════════════════════════════════════
CREATE TABLE SAN (
    MaSan VARCHAR(20) PRIMARY KEY,
    TenSan NVARCHAR(100) NOT NULL,
    DiaChi NVARCHAR(255),
    GhiChu NVARCHAR(MAX),
    MaLoaiSan CHAR(2) REFERENCES LOAISAN(MaLoaiSan),
    MaTinhTrang CHAR(2) REFERENCES TINHTRANG(MaTinhTrang)
);

-- ═══════════════════════════════════════════════════════
-- 7. Bảng Loại Hội Viên (Quy định 2)
-- ═══════════════════════════════════════════════════════
CREATE TABLE LOAIHOIVIEN (
    MaLoaiHoiVien CHAR(2) PRIMARY KEY, -- DO, BA, VA, KC
    TenLoaiHoiVien NVARCHAR(50) NOT NULL,
    DiemToiThieu INT DEFAULT 0
);

-- ═══════════════════════════════════════════════════════
-- 8. Bảng Hội Viên
-- ═══════════════════════════════════════════════════════
CREATE TABLE HOIVIEN (
    MaHoiVien VARCHAR(20) PRIMARY KEY,
    HoTen NVARCHAR(100) NOT NULL,
    SDT VARCHAR(20) NOT NULL UNIQUE,
    Email VARCHAR(100) NOT NULL UNIQUE,
    GioiTinh NVARCHAR(10),
    NgayDangKyHoiVien DATE,
    DiemTichLuy INT DEFAULT 0,
    MaLoaiHoiVien CHAR(2) REFERENCES LOAIHOIVIEN(MaLoaiHoiVien),
    GhiChu NVARCHAR(MAX)
);

-- ═══════════════════════════════════════════════════════
-- 9. Bảng Tham Số (cấu hình hệ thống)
--    - MucDiemTichLuyMacDinh: điểm tích lũy mặc định khi đăng ký
--    - MaLoaiHoiVienMacDinh : loại HV mặc định cho HV mới
--    - TinhTrangKhongDuocDat: tình trạng sân KHÔNG cho đặt (Quy định 4)
-- ═══════════════════════════════════════════════════════
CREATE TABLE THAMSO (
    Id INT PRIMARY KEY DEFAULT 1, -- chỉ có 1 dòng duy nhất
    MucDiemTichLuyMacDinh INT DEFAULT 0,
    MaLoaiHoiVienMacDinh CHAR(2) REFERENCES LOAIHOIVIEN(MaLoaiHoiVien),
    TinhTrangKhongDuocDat CHAR(2) REFERENCES TINHTRANG(MaTinhTrang),
    CONSTRAINT CK_ThamSo_OnlyOneRow CHECK (Id = 1)
);

-- ═══════════════════════════════════════════════════════
-- 9b. Bảng Tích Điểm (cấu hình – chỉ 1 dòng) – Quy định 5
--     HeSoTichDiem: số tiền (VNĐ) tương ứng 1 điểm tích lũy.
--     Điểm tích lũy = floor(Số tiền phải trả / HeSoTichDiem).
-- ═══════════════════════════════════════════════════════
CREATE TABLE TICHDIEM (
    Id INT PRIMARY KEY DEFAULT 1, -- chỉ có 1 dòng duy nhất
    HeSoTichDiem FLOAT NOT NULL DEFAULT 100000, -- 100.000đ = 1 điểm
    CONSTRAINT CK_TichDiem_OnlyOneRow CHECK (Id = 1)
);

-- ═══════════════════════════════════════════════════════
-- 10. Bảng Chi Tiết Đặt Sân
-- ═══════════════════════════════════════════════════════
CREATE TABLE CHITIETDATSAN (
    MaChiTiet VARCHAR(20) PRIMARY KEY,
    MaSan VARCHAR(20) NOT NULL REFERENCES SAN(MaSan),
    GioBatDau TIME NOT NULL,
    GioKetThuc TIME NOT NULL,
    MaLoaiNgay CHAR(2) REFERENCES LOAINGAY(MaLoaiNgay)
);
GO

-- ═══════════════════════════════════════════════════════
-- 11. Bảng Đặt Sân
-- ═══════════════════════════════════════════════════════
CREATE TABLE DATSAN (
    MaDatSan VARCHAR(20) PRIMARY KEY,
    MaHoiVien VARCHAR(20) NOT NULL REFERENCES HOIVIEN(MaHoiVien),
    MaChiTiet VARCHAR(20) REFERENCES CHITIETDATSAN(MaChiTiet),
    NgayDat DATE NOT NULL,
    TongTien MONEY DEFAULT 0,
    GhiChu NVARCHAR(MAX)
);
GO

-- ═══════════════════════════════════════════════════════
-- 12. DỮ LIỆU DANH MỤC BAN ĐẦU
-- ═══════════════════════════════════════════════════════

-- Loại hội viên (Quy định 2: 4 loại với mức điểm 0, 100, 200, 300)
INSERT INTO LOAIHOIVIEN (MaLoaiHoiVien, TenLoaiHoiVien, DiemToiThieu) VALUES
('DO', N'Đồng', 0),
('BA', N'Bạc', 100),
('VA', N'Vàng', 200),
('KC', N'Kim cương', 300);

-- Loại sân
INSERT INTO LOAISAN (MaLoaiSan, TenLoaiSan) VALUES 
('BD', N'Sân bóng đá'), 
('CL', N'Sân cầu lông'), 
('PB', N'Sân pickleball');

-- Tình trạng sân
INSERT INTO TINHTRANG (MaTinhTrang, TenTinhTrang) VALUES 
('HD', N'Hoạt động'), 
('BT', N'Bảo trì');

-- Đơn giá theo loại ngày (Quy định 1)
INSERT INTO LOAINGAY (MaLoaiNgay, TenLoaiNgay, DonGiaNgay) VALUES 
('NT', N'Thường', 50000), 
('CT', N'Cuối tuần', 70000), 
('NL', N'Lễ', 100000);

-- Tham số hệ thống mặc định
INSERT INTO THAMSO (Id, MucDiemTichLuyMacDinh, MaLoaiHoiVienMacDinh, TinhTrangKhongDuocDat)
VALUES (1, 0, 'DO', 'BT');

-- Hệ số tích điểm mặc định (1 dòng duy nhất): 100.000đ = 1 điểm
INSERT INTO TICHDIEM (Id, HeSoTichDiem) VALUES (1, 100000);

GO

-- ═══════════════════════════════════════════════════════
-- 13. DỮ LIỆU MẪU KIỂM THỬ BÁO CÁO DOANH THU (Sprint 6.1 & 6.2)
--     Đặt sân nằm trong THÁNG 6/2026 → mở báo cáo chọn Tháng 6 / Năm 2026.
--     Kết quả mong đợi:
--       Theo sân:  S01 = 150.000đ (lấp đầy 2/4 = 50%)
--                  S02 =  50.000đ (lấp đầy 1/3 ≈ 33,33%)
--                  S03 = 200.000đ (lấp đầy 2/2 = 100%)   → Tổng = 400.000đ
--       Theo KH:   An = 200.000đ (50%), Bình = 100.000đ (25%), Cường = 100.000đ (25%)
-- ═══════════════════════════════════════════════════════

-- Sân (LOAISAN: BD/CL/PB, TINHTRANG: HD)
INSERT INTO SAN (MaSan, TenSan, DiaChi, GhiChu, MaLoaiSan, MaTinhTrang) VALUES
('S01', N'Sân bóng đá A', N'123 Lê Lợi, Q1', N'', 'BD', 'HD'),
('S02', N'Sân cầu lông B', N'45 Nguyễn Huệ, Q1', N'', 'CL', 'HD'),
('S03', N'Sân pickleball C', N'7 Trần Hưng Đạo, Q5', N'', 'PB', 'HD');

-- Khung giờ (CHITIETDATSAN) — đơn giá theo LOAINGAY: NT/CT/NL
-- S01 có 4 khung, S02 có 3 khung, S03 có 2 khung
INSERT INTO CHITIETDATSAN (MaChiTiet, MaSan, GioBatDau, GioKetThuc, MaLoaiNgay) VALUES
('CT01', 'S01', '05:00:00', '06:00:00', 'NT'),
('CT02', 'S01', '06:00:00', '07:00:00', 'NT'),
('CT03', 'S01', '17:00:00', '18:00:00', 'CT'),
('CT04', 'S01', '18:00:00', '19:00:00', 'NL'),
('CT05', 'S02', '06:00:00', '07:00:00', 'NT'),
('CT06', 'S02', '17:00:00', '18:00:00', 'CT'),
('CT07', 'S02', '19:00:00', '20:00:00', 'CT'),
('CT08', 'S03', '07:00:00', '08:00:00', 'NT'),
('CT09', 'S03', '20:00:00', '21:00:00', 'NL');

-- Hội viên (hạng khác nhau để test giảm giá Quy định 5: DO 0%, BA 3%, VA 5%, KC 10%)
INSERT INTO HOIVIEN (MaHoiVien, HoTen, SDT, Email, GioiTinh, NgayDangKyHoiVien, DiemTichLuy, MaLoaiHoiVien, GhiChu) VALUES
('HV01', N'Nguyễn Văn An', '0901000001', 'an.nguyen@example.com', N'Nam', '2026-01-10', 0, 'DO', N''),
('HV02', N'Trần Thị Bình', '0901000002', 'binh.tran@example.com', N'Nữ', '2026-02-15', 100, 'BA', N''),
('HV03', N'Lê Văn Cường', '0901000003', 'cuong.le@example.com', N'Nam', '2026-03-20', 200, 'VA', N'');

-- Đặt sân (Tháng 6/2026)
INSERT INTO DATSAN (MaDatSan, MaHoiVien, MaChiTiet, NgayDat, TongTien, GhiChu) VALUES
('DS01', 'HV01', 'CT01', '2026-06-05', 50000, N''),
('DS02', 'HV01', 'CT02', '2026-06-06', 50000, N''),
('DS03', 'HV02', 'CT01', '2026-06-07', 50000, N''),  -- trùng khung CT01 (distinct vẫn tính 1)
('DS04', 'HV02', 'CT05', '2026-06-08', 50000, N''),
('DS05', 'HV03', 'CT08', '2026-06-10', 50000, N''),
('DS06', 'HV01', 'CT09', '2026-06-12', 100000, N''),
('DS07', 'HV03', 'CT08', '2026-06-15', 50000, N'');  -- trùng khung CT08

GO
