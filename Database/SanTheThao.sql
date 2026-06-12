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

GO
