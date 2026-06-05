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

-- 3. Bảng Loại Sân (Khớp với các item trong cboMaLoaiSan)
CREATE TABLE LOAISAN (
    MaLoaiSan CHAR(2) PRIMARY KEY, -- BD, CL, PB
    TenLoaiSan NVARCHAR(100) NOT NULL
);

-- 4. Bảng Tình Trạng (Khớp với cboTinhTrang)
CREATE TABLE TINHTRANG (
    MaTinhTrang CHAR(2) PRIMARY KEY, -- HD, BT
    TenTinhTrang NVARCHAR(100) NOT NULL
);

-- 5. Bảng Loại Ngày (Khớp với logic tính đơn giá trong GioSanItem)
CREATE TABLE LOAINGAY (
    MaLoaiNgay CHAR(2) PRIMARY KEY, -- NT, CT, NL
    TenLoaiNgay NVARCHAR(100) NOT NULL,
    DonGiaNgay MONEY NOT NULL
);

-- 6. Bảng Sân (Bổ sung cột GhiChu và tăng độ dài MaSan để khớp code C#)
CREATE TABLE SAN (
    MaSan VARCHAR(20) PRIMARY KEY,   -- Khớp với "SAN" + timestamp trong code
    TenSan NVARCHAR(100) NOT NULL,   -- Khớp với txtTenSan
    DiaChi NVARCHAR(255),            -- Khớp với txtDiaChi
    GhiChu NVARCHAR(MAX),            -- Khớp với txtGhiChu
    MaLoaiSan CHAR(2) REFERENCES LOAISAN(MaLoaiSan),
    MaTinhTrang CHAR(2) REFERENCES TINHTRANG(MaTinhTrang)
);

-- 7. Bảng Chi Tiết Giờ Sân (Lưu danh sách từ DataGrid dgGioSan)
CREATE TABLE CHITIETDATSAN (
    MaDatSan INT IDENTITY(1,1) PRIMARY KEY,
    MaSan VARCHAR(20) REFERENCES SAN(MaSan),
    GioBatDau TIME NOT NULL,
    GioKetThuc TIME NOT NULL,
    MaLoaiNgay CHAR(2) REFERENCES LOAINGAY(MaLoaiNgay),
    DonGia MONEY
);
GO

-- 8. Bảng Loại Hội Viên
CREATE TABLE LOAIHOIVIEN (
    MaLoaiHoiVien CHAR(2) PRIMARY KEY, -- DO (Đồng), BA (Bạc), VA (Vàng), KC (Kim cương)
    TenLoaiHoiVien NVARCHAR(50) NOT NULL,
    DiemToiThieu INT DEFAULT 0
);

-- 9. Bảng Hội Viên
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
GO

-- 10. Bảng Phiếu Đặt Sân (Sprint 4 - BM4: Thông tin đặt sân)
CREATE TABLE PHIEUDATSAN (
    MaPhieuDat VARCHAR(20) PRIMARY KEY,
    MaSan VARCHAR(20) REFERENCES SAN(MaSan),
    MaHoiVien VARCHAR(20) REFERENCES HOIVIEN(MaHoiVien),
    NgayDat DATE NOT NULL,
    TongTien MONEY DEFAULT 0,
    GhiChu NVARCHAR(MAX)
);

-- 11. Bảng Chi Tiết Phiếu Đặt (danh sách khung giờ đặt của 1 phiếu)
CREATE TABLE CHITIETPHIEUDAT (
    MaChiTiet INT IDENTITY(1,1) PRIMARY KEY,
    MaPhieuDat VARCHAR(20) REFERENCES PHIEUDATSAN(MaPhieuDat),
    GioBatDau TIME NOT NULL,
    GioKetThuc TIME NOT NULL,
    MaLoaiNgay CHAR(2) REFERENCES LOAINGAY(MaLoaiNgay),
    DonGia MONEY
);
GO

-- 12. Chèn dữ liệu danh mục ban đầu
-- Theo Quy định 2: 4 loại hội viên với mức điểm tích luỹ tối thiểu lần lượt 0, 100, 200, 300
INSERT INTO LOAIHOIVIEN (MaLoaiHoiVien, TenLoaiHoiVien, DiemToiThieu) VALUES
('DO', N'Đồng', 0),
('BA', N'Bạc', 100),
('VA', N'Vàng', 200),
('KC', N'Kim cương', 300);

INSERT INTO LOAISAN (MaLoaiSan, TenLoaiSan) VALUES 
('BD', N'Sân bóng đá'), 
('CL', N'Sân cầu lông'), 
('PB', N'Sân pickleball');

INSERT INTO TINHTRANG (MaTinhTrang, TenTinhTrang) VALUES 
('HD', N'Hoạt động'), 
('BT', N'Bảo trì');

-- Chèn đơn giá theo đúng Quy định 1 (QĐ1) bạn đã viết trong Dictionary BangDonGia
INSERT INTO LOAINGAY (MaLoaiNgay, TenLoaiNgay, DonGiaNgay) VALUES 
('NT', N'Thường', 50000), 
('CT', N'Cuối tuần', 70000), 
('NL', N'Lễ', 100000);
GO
