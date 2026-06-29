USE master;
GO

-- 1. Kiểm tra và xóa database nếu đã tồn tại
IF EXISTS (SELECT * FROM sys.databases WHERE name = 'QLSanTheThao')
BEGIN
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
-- 0. Bảng Tài Khoản (Multi-tenant)
-- ═══════════════════════════════════════════════════════
CREATE TABLE ACCOUNT (
    AccountId INT IDENTITY(1,1) PRIMARY KEY,
    Username NVARCHAR(50) NOT NULL UNIQUE,
    PasswordHash NVARCHAR(255) NOT NULL,
    Email NVARCHAR(100)
);

-- ═══════════════════════════════════════════════════════
-- Bảng Danh Mục Dùng Chung (Shared across accounts)
-- ═══════════════════════════════════════════════════════
CREATE TABLE LOAISAN (
    MaLoaiSan CHAR(2) PRIMARY KEY,
    TenLoaiSan NVARCHAR(100) NOT NULL
);

CREATE TABLE TINHTRANG (
    MaTinhTrang CHAR(2) PRIMARY KEY,
    TenTinhTrang NVARCHAR(100) NOT NULL
);

CREATE TABLE LOAINGAY (
    MaLoaiNgay CHAR(2) PRIMARY KEY,
    TenLoaiNgay NVARCHAR(100) NOT NULL,
    DonGiaNgay MONEY NOT NULL
);

CREATE TABLE LOAIHOIVIEN (
    MaLoaiHoiVien CHAR(2) PRIMARY KEY,
    TenLoaiHoiVien NVARCHAR(50) NOT NULL,
    DiemToiThieu INT DEFAULT 0,
    MucGiamGia FLOAT DEFAULT 0
);

-- ═══════════════════════════════════════════════════════
-- Các Bảng Bị Cô Lập Dữ Liệu (Tenant-specific Data)
-- Chú ý: Dùng DEFAULT CAST(SESSION_CONTEXT(N'AccountId') AS INT)
--        để C# không cần sửa Insert statement
-- ═══════════════════════════════════════════════════════

CREATE TABLE SAN (
    MaSan VARCHAR(20) PRIMARY KEY,
    TenSan NVARCHAR(100) NOT NULL,
    DiaChi NVARCHAR(255),
    GhiChu NVARCHAR(MAX),
    MaLoaiSan CHAR(2) REFERENCES LOAISAN(MaLoaiSan),
    MaTinhTrang CHAR(2) REFERENCES TINHTRANG(MaTinhTrang),
    AccountId INT NOT NULL DEFAULT CAST(SESSION_CONTEXT(N'AccountId') AS INT) REFERENCES ACCOUNT(AccountId)
);

CREATE TABLE HOIVIEN (
    MaHoiVien VARCHAR(20) PRIMARY KEY,
    HoTen NVARCHAR(100) NOT NULL,
    SDT VARCHAR(20) NOT NULL,
    Email VARCHAR(100) NOT NULL,
    GioiTinh NVARCHAR(10),
    NgayDangKyHoiVien DATE,
    DiemTichLuy INT DEFAULT 0,
    MaLoaiHoiVien CHAR(2) REFERENCES LOAIHOIVIEN(MaLoaiHoiVien),
    GhiChu NVARCHAR(MAX),
    AccountId INT NOT NULL DEFAULT CAST(SESSION_CONTEXT(N'AccountId') AS INT) REFERENCES ACCOUNT(AccountId),
    CONSTRAINT UQ_HoiVien_SDT_Account UNIQUE(SDT, AccountId),
    CONSTRAINT UQ_HoiVien_Email_Account UNIQUE(Email, AccountId)
);

CREATE TABLE THAMSO (
    AccountId INT PRIMARY KEY DEFAULT CAST(SESSION_CONTEXT(N'AccountId') AS INT) REFERENCES ACCOUNT(AccountId),
    MucDiemTichLuyMacDinh INT DEFAULT 0,
    MaLoaiHoiVienMacDinh CHAR(2) REFERENCES LOAIHOIVIEN(MaLoaiHoiVien),
    TinhTrangKhongDuocDat CHAR(2) REFERENCES TINHTRANG(MaTinhTrang)
);

CREATE TABLE TICHDIEM (
    AccountId INT PRIMARY KEY DEFAULT CAST(SESSION_CONTEXT(N'AccountId') AS INT) REFERENCES ACCOUNT(AccountId),
    HeSoTichDiem FLOAT NOT NULL DEFAULT 100000 
);

CREATE TABLE CHITIETDATSAN (
    MaChiTiet VARCHAR(20) PRIMARY KEY,
    MaSan VARCHAR(20) NOT NULL REFERENCES SAN(MaSan),
    GioBatDau TIME NOT NULL,
    GioKetThuc TIME NOT NULL,
    MaLoaiNgay CHAR(2) REFERENCES LOAINGAY(MaLoaiNgay)
);

CREATE TABLE DATSAN (
    MaDatSan VARCHAR(20) PRIMARY KEY,
    MaHoiVien VARCHAR(20) NOT NULL REFERENCES HOIVIEN(MaHoiVien),
    MaChiTiet VARCHAR(20) REFERENCES CHITIETDATSAN(MaChiTiet),
    NgayDat DATE NOT NULL,
    TongTien MONEY DEFAULT 0,
    GhiChu NVARCHAR(MAX),
    AccountId INT NOT NULL DEFAULT CAST(SESSION_CONTEXT(N'AccountId') AS INT) REFERENCES ACCOUNT(AccountId)
);
GO

-- ═══════════════════════════════════════════════════════
-- DỮ LIỆU BAN ĐẦU
-- ═══════════════════════════════════════════════════════

-- Tạo tài khoản Admin
INSERT INTO ACCOUNT (Username, PasswordHash, Email) VALUES ('admin', 'admin', 'admin@example.com');

-- Cài đặt Context AccountId=1 cho các Insert tiếp theo
EXEC sp_set_session_context @key=N'AccountId', @value=1;

INSERT INTO LOAIHOIVIEN (MaLoaiHoiVien, TenLoaiHoiVien, DiemToiThieu, MucGiamGia) VALUES
('DO', N'Đồng', 0, 0), ('BA', N'Bạc', 100, 0.03), ('VA', N'Vàng', 200, 0.05), ('KC', N'Kim cương', 300, 0.10);

INSERT INTO LOAISAN (MaLoaiSan, TenLoaiSan) VALUES 
('BD', N'Sân bóng đá'), ('CL', N'Sân cầu lông'), ('PB', N'Sân pickleball');

INSERT INTO TINHTRANG (MaTinhTrang, TenTinhTrang) VALUES 
('HD', N'Hoạt động'), ('BT', N'Bảo trì');

INSERT INTO LOAINGAY (MaLoaiNgay, TenLoaiNgay, DonGiaNgay) VALUES 
('NT', N'Thường', 50000), ('CT', N'Cuối tuần', 70000), ('NL', N'Lễ', 100000);

-- Tham số hệ thống mặc định (Tự nhận AccountId = 1 qua SESSION_CONTEXT)
INSERT INTO THAMSO (MucDiemTichLuyMacDinh, MaLoaiHoiVienMacDinh, TinhTrangKhongDuocDat)
VALUES (0, 'DO', 'BT');

-- Tích điểm mặc định
INSERT INTO TICHDIEM (HeSoTichDiem) VALUES (100000);

-- Sân mẫu
INSERT INTO SAN (MaSan, TenSan, DiaChi, GhiChu, MaLoaiSan, MaTinhTrang) VALUES
('S01', N'Sân bóng đá A', N'123 Lê Lợi, Q1', N'', 'BD', 'HD'),
('S02', N'Sân cầu lông B', N'45 Nguyễn Huệ, Q1', N'', 'CL', 'HD'),
('S03', N'Sân pickleball C', N'7 Trần Hưng Đạo, Q5', N'', 'PB', 'HD');

-- Khung giờ mẫu
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

-- Hội viên mẫu
INSERT INTO HOIVIEN (MaHoiVien, HoTen, SDT, Email, GioiTinh, NgayDangKyHoiVien, DiemTichLuy, MaLoaiHoiVien, GhiChu) VALUES
('HV01', N'Nguyễn Văn An', '0901000001', 'an.nguyen@example.com', N'Nam', '2026-01-10', 0, 'DO', N''),
('HV02', N'Trần Thị Bình', '0901000002', 'binh.tran@example.com', N'Nữ', '2026-02-15', 100, 'BA', N''),
('HV03', N'Lê Văn Cường', '0901000003', 'cuong.le@example.com', N'Nam', '2026-03-20', 200, 'VA', N'');

-- Đặt sân mẫu
INSERT INTO DATSAN (MaDatSan, MaHoiVien, MaChiTiet, NgayDat, TongTien, GhiChu) VALUES
('DS01', 'HV01', 'CT01', '2026-06-05', 50000, N''),
('DS02', 'HV01', 'CT02', '2026-06-06', 50000, N''),
('DS03', 'HV02', 'CT01', '2026-06-07', 50000, N''),
('DS04', 'HV02', 'CT05', '2026-06-08', 50000, N''),
('DS05', 'HV03', 'CT08', '2026-06-10', 50000, N''),
('DS06', 'HV01', 'CT09', '2026-06-12', 100000, N''),
('DS07', 'HV03', 'CT08', '2026-06-15', 50000, N'');
GO

-- ═══════════════════════════════════════════════════════
-- ROW-LEVEL SECURITY (RLS) POLICY (QUAN TRỌNG: Đảm bảo multi-tenant)
-- ═══════════════════════════════════════════════════════

CREATE FUNCTION dbo.fn_tenant_security_predicate(@AccountId AS INT)
    RETURNS TABLE
WITH SCHEMABINDING
AS
    RETURN SELECT 1 AS fn_tenant_security_predicate_result
    WHERE @AccountId = CAST(SESSION_CONTEXT(N'AccountId') AS INT);
GO

CREATE SECURITY POLICY TenantPolicy
ADD FILTER PREDICATE dbo.fn_tenant_security_predicate(AccountId) ON dbo.SAN,
ADD BLOCK PREDICATE dbo.fn_tenant_security_predicate(AccountId) ON dbo.SAN,
ADD FILTER PREDICATE dbo.fn_tenant_security_predicate(AccountId) ON dbo.HOIVIEN,
ADD BLOCK PREDICATE dbo.fn_tenant_security_predicate(AccountId) ON dbo.HOIVIEN,
ADD FILTER PREDICATE dbo.fn_tenant_security_predicate(AccountId) ON dbo.DATSAN,
ADD BLOCK PREDICATE dbo.fn_tenant_security_predicate(AccountId) ON dbo.DATSAN,
ADD FILTER PREDICATE dbo.fn_tenant_security_predicate(AccountId) ON dbo.THAMSO,
ADD BLOCK PREDICATE dbo.fn_tenant_security_predicate(AccountId) ON dbo.THAMSO,
ADD FILTER PREDICATE dbo.fn_tenant_security_predicate(AccountId) ON dbo.TICHDIEM,
ADD BLOCK PREDICATE dbo.fn_tenant_security_predicate(AccountId) ON dbo.TICHDIEM
WITH (STATE = ON);
GO
