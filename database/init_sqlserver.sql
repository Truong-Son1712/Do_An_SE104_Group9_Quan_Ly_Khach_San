-- ============================================================
--  Quản Lý Khách Sạn – Grand Hotel
--  SQL Server init script  (schema đầy đủ + dữ liệu mẫu)
--
--  Cách dùng:
--    sqlcmd -S . -E -i init_sqlserver.sql
--    Hoặc mở SSMS → File → Open → chạy F5
--
--  Tài khoản mặc định:
--    admin  / admin123   (Admin)
--    quanly / quanly123  (Quản Lý)
--    letan  / letan123   (Lễ Tân)
--
--  Enum TrangThaiPhong   : 0=TrongSach 1=DangSuDung 2=CanDonDep 3=BaoDuong 4=DaDat
--  Enum TrangThaiDatPhong: 0=DaDat 1=DaNhanPhong 2=DaTraPhong 3=HuyDat
-- ============================================================

-- ── Tạo database ──────────────────────────────────────────────────────────
IF NOT EXISTS (SELECT name FROM sys.databases WHERE name = 'QuanLyKhachSan')
BEGIN
    CREATE DATABASE QuanLyKhachSan COLLATE Vietnamese_CI_AS;
    PRINT 'Database QuanLyKhachSan created.';
END
GO

USE QuanLyKhachSan;
GO

-- ── Xóa bảng cũ theo đúng thứ tự FK ─────────────────────────────────────
IF OBJECT_ID('LichSuDungMaGiams',  'U') IS NOT NULL DROP TABLE LichSuDungMaGiams;
IF OBJECT_ID('ChiTietMaGiamGias',  'U') IS NOT NULL DROP TABLE ChiTietMaGiamGias;
IF OBJECT_ID('DichVuPhongs',       'U') IS NOT NULL DROP TABLE DichVuPhongs;
IF OBJECT_ID('HoaDons',            'U') IS NOT NULL DROP TABLE HoaDons;
IF OBJECT_ID('DatPhongKhachHangs', 'U') IS NOT NULL DROP TABLE DatPhongKhachHangs;
IF OBJECT_ID('DatPhongs',          'U') IS NOT NULL DROP TABLE DatPhongs;
IF OBJECT_ID('KhachHangs',         'U') IS NOT NULL DROP TABLE KhachHangs;
IF OBJECT_ID('Phongs',             'U') IS NOT NULL DROP TABLE Phongs;
IF OBJECT_ID('LoaiPhongs',         'U') IS NOT NULL DROP TABLE LoaiPhongs;
IF OBJECT_ID('LoaiDichVus',        'U') IS NOT NULL DROP TABLE LoaiDichVus;
IF OBJECT_ID('NhanViens',          'U') IS NOT NULL DROP TABLE NhanViens;
IF OBJECT_ID('MaGiamGias',         'U') IS NOT NULL DROP TABLE MaGiamGias;
IF OBJECT_ID('LoaiKhachHangs',     'U') IS NOT NULL DROP TABLE LoaiKhachHangs;
IF OBJECT_ID('CauHinhs',           'U') IS NOT NULL DROP TABLE CauHinhs;
GO

-- ── Schema ─────────────────────────────────────────────────────────────────

-- 1. Cấu hình hệ thống
CREATE TABLE CauHinhs (
    ConfigKey   NVARCHAR(100) NOT NULL PRIMARY KEY,
    ConfigValue NVARCHAR(500) NOT NULL DEFAULT ''
);

-- 2. Loại khách hàng
CREATE TABLE LoaiKhachHangs (
    MaLKH   INT           NOT NULL IDENTITY(1,1) PRIMARY KEY,
    MaCode  NVARCHAR(100) NOT NULL UNIQUE,
    TenLoai NVARCHAR(200) NOT NULL,
    HeSoGia DECIMAL(10,4) NOT NULL DEFAULT 1.0
);

-- 3. Nhân viên
CREATE TABLE NhanViens (
    MaNV     INT           NOT NULL IDENTITY(1,1) PRIMARY KEY,
    HoTen    NVARCHAR(100) NOT NULL,
    TaiKhoan NVARCHAR(50)  NOT NULL UNIQUE,
    MatKhau  NVARCHAR(MAX) NOT NULL,
    VaiTro   NVARCHAR(20)  NOT NULL DEFAULT 'LeTan',
    CCCD     NVARCHAR(30)  NULL,
    Email    NVARCHAR(200) NULL,
    SDT      NVARCHAR(20)  NULL,
    DiaChi   NVARCHAR(500) NULL,
    NgayTao  DATETIME2     NOT NULL DEFAULT GETDATE(),
    IsActive BIT           NOT NULL DEFAULT 1
);

-- 4. Mã giảm giá (TenMa KHÔNG unique; SoLuongToiDa NULL = vô hạn)
CREATE TABLE MaGiamGias (
    MaGG         INT           NOT NULL IDENTITY(1,1) PRIMARY KEY,
    TenMa        NVARCHAR(100) NOT NULL,
    MoTa         NVARCHAR(500) NULL,
    NgayBatDau   DATETIME2     NOT NULL,
    NgayKetThuc  DATETIME2     NOT NULL,
    TrangThai    NVARCHAR(20)  NOT NULL DEFAULT 'Active',
    NgayTao      DATETIME2     NOT NULL DEFAULT GETDATE(),
    SoLuongToiDa INT           NULL,
    MaNVTao      INT           NULL,
    CONSTRAINT FK_MGG_NhanVien FOREIGN KEY (MaNVTao)
        REFERENCES NhanViens(MaNV) ON DELETE NO ACTION
);

-- 5. Loại phòng
CREATE TABLE LoaiPhongs (
    MaLoaiPhong  INT           NOT NULL IDENTITY(1,1) PRIMARY KEY,
    TenLoaiPhong NVARCHAR(200) NOT NULL,
    GiaPhong     DECIMAL(18,2) NOT NULL DEFAULT 0,
    SucChua      INT           NOT NULL DEFAULT 2,
    MoTa         NVARCHAR(500) NULL
);

-- 6. Phòng
CREATE TABLE Phongs (
    MaPhong     INT           NOT NULL IDENTITY(1,1) PRIMARY KEY,
    SoPhong     NVARCHAR(20)  NOT NULL,
    MaLoaiPhong INT           NOT NULL,
    Tang        INT           NOT NULL DEFAULT 1,
    TrangThai   INT           NOT NULL DEFAULT 0,
    MoTa        NVARCHAR(500) NULL,
    CONSTRAINT FK_Phong_LoaiPhong FOREIGN KEY (MaLoaiPhong)
        REFERENCES LoaiPhongs(MaLoaiPhong) ON DELETE NO ACTION
);

-- 7. Khách hàng
CREATE TABLE KhachHangs (
    MaKH      INT           NOT NULL IDENTITY(1,1) PRIMARY KEY,
    HoTen     NVARCHAR(100) NOT NULL,
    CMND      NVARCHAR(30)  NOT NULL,
    SDT       NVARCHAR(20)  NULL,
    Email     NVARCHAR(200) NULL,
    DiaChi    NVARCHAR(500) NULL,
    QuocTich  NVARCHAR(100) NOT NULL DEFAULT N'Việt Nam',
    MaLoaiKH  INT           NOT NULL DEFAULT 1,
    NgaySinh  DATETIME2     NULL,
    GioiTinh  NVARCHAR(10)  NOT NULL DEFAULT 'Nam',
    NgayTao   DATETIME2     NOT NULL DEFAULT GETDATE(),
    CONSTRAINT FK_KhachHang_LoaiKhach FOREIGN KEY (MaLoaiKH)
        REFERENCES LoaiKhachHangs(MaLKH) ON DELETE NO ACTION
);

-- 8. Đặt phòng
--    NguoiDatPhongOPhong: 1 = người đặt cũng ở phòng, 0 = chỉ đặt hộ
CREATE TABLE DatPhongs (
    MaDatPhong          INT           NOT NULL IDENTITY(1,1) PRIMARY KEY,
    MaKH                INT           NOT NULL,
    MaPhong             INT           NOT NULL,
    NgayDat             DATETIME2     NOT NULL DEFAULT GETDATE(),
    NgayNhanPhong       DATETIME2     NOT NULL,
    NgayTraPhong        DATETIME2     NOT NULL,
    TrangThai           INT           NOT NULL DEFAULT 0,
    TienCoc             DECIMAL(18,2) NOT NULL DEFAULT 0,
    SoKhach             INT           NOT NULL DEFAULT 1,
    NguoiDatPhongOPhong BIT           NOT NULL DEFAULT 1,
    GhiChu              NVARCHAR(500) NULL,
    CONSTRAINT FK_DatPhong_KhachHang FOREIGN KEY (MaKH)
        REFERENCES KhachHangs(MaKH) ON DELETE NO ACTION,
    CONSTRAINT FK_DatPhong_Phong FOREIGN KEY (MaPhong)
        REFERENCES Phongs(MaPhong) ON DELETE NO ACTION
);

-- 9. Khách ở phòng (many-to-many; chỉ chứa người thực sự ngủ tại phòng)
CREATE TABLE DatPhongKhachHangs (
    MaDatPhong INT NOT NULL,
    MaKH       INT NOT NULL,
    CONSTRAINT PK_DatPhongKhachHang PRIMARY KEY (MaDatPhong, MaKH),
    CONSTRAINT FK_DPKH_DatPhong  FOREIGN KEY (MaDatPhong)
        REFERENCES DatPhongs(MaDatPhong) ON DELETE CASCADE,
    CONSTRAINT FK_DPKH_KhachHang FOREIGN KEY (MaKH)
        REFERENCES KhachHangs(MaKH) ON DELETE NO ACTION
);

-- 10. Hóa đơn
CREATE TABLE HoaDons (
    MaHD              INT           NOT NULL IDENTITY(1,1) PRIMARY KEY,
    MaDatPhong        INT           NOT NULL UNIQUE,
    MaNV              INT           NOT NULL,
    NgayLap           DATETIME2     NOT NULL DEFAULT GETDATE(),
    -- Breakdown giá phòng
    GiaPhongGoc       DECIMAL(18,2) NOT NULL DEFAULT 0,
    HeSoLoaiKhach     DECIMAL(10,4) NOT NULL DEFAULT 1,
    TenLoaiKhachMax   NVARCHAR(100) NULL,
    TiLePhuThuSucChua DECIMAL(5,4)  NOT NULL DEFAULT 0,
    TienPhong         DECIMAL(18,2) NOT NULL DEFAULT 0,
    -- Giảm giá & VAT
    TienGiam          DECIMAL(18,2) NOT NULL DEFAULT 0,
    MaGG              INT           NULL,
    TienVAT           DECIMAL(18,2) NOT NULL DEFAULT 0,
    VATPercent        DECIMAL(5,2)  NOT NULL DEFAULT 0,
    -- Thanh toán
    TienCoc           DECIMAL(18,2) NOT NULL DEFAULT 0,
    TongTien          DECIMAL(18,2) NOT NULL DEFAULT 0,
    PhuongThucTT      NVARCHAR(30)  NOT NULL DEFAULT 'TienMat',
    TrangThai         NVARCHAR(30)  NOT NULL DEFAULT 'ChuaThanhToan',
    NgayThanhToan     DATETIME2     NULL,
    NgayTraPhongGoc   DATETIME2     NULL,
    GhiChu            NVARCHAR(500) NULL,
    CONSTRAINT FK_HoaDon_DatPhong  FOREIGN KEY (MaDatPhong)
        REFERENCES DatPhongs(MaDatPhong) ON DELETE CASCADE,
    CONSTRAINT FK_HoaDon_NhanVien  FOREIGN KEY (MaNV)
        REFERENCES NhanViens(MaNV) ON DELETE NO ACTION,
    CONSTRAINT FK_HoaDon_MaGiamGia FOREIGN KEY (MaGG)
        REFERENCES MaGiamGias(MaGG) ON DELETE NO ACTION
);

-- 11. Loại dịch vụ
CREATE TABLE LoaiDichVus (
    MaLoaiDV  INT           NOT NULL IDENTITY(1,1) PRIMARY KEY,
    TenLoaiDV NVARCHAR(200) NOT NULL,
    DonGia    DECIMAL(18,2) NOT NULL DEFAULT 0,
    DonViTinh NVARCHAR(50)  NOT NULL DEFAULT '',
    MoTa      NVARCHAR(500) NULL,
    IsActive  BIT           NOT NULL DEFAULT 1
);

-- 12. Chi tiết mã giảm giá
CREATE TABLE ChiTietMaGiamGias (
    MaChiTiet  INT          NOT NULL IDENTITY(1,1) PRIMARY KEY,
    MaGG       INT          NOT NULL,
    LoaiApDung NVARCHAR(20) NOT NULL DEFAULT 'LoaiPhong',
    MaLoai     INT          NOT NULL,
    TiLeGiam   DECIMAL(5,2) NOT NULL DEFAULT 0,
    CONSTRAINT FK_CTMGG_MaGiamGia FOREIGN KEY (MaGG)
        REFERENCES MaGiamGias(MaGG) ON DELETE CASCADE
);

-- 13. Lịch sử sử dụng mã giảm giá
CREATE TABLE LichSuDungMaGiams (
    MaSuDung   INT       NOT NULL IDENTITY(1,1) PRIMARY KEY,
    MaGG       INT       NOT NULL,
    MaHD       INT       NOT NULL,
    NgaySuDung DATETIME2 NOT NULL DEFAULT GETDATE(),
    CONSTRAINT FK_LSDMGG_MaGiamGia FOREIGN KEY (MaGG)
        REFERENCES MaGiamGias(MaGG) ON DELETE NO ACTION,
    CONSTRAINT FK_LSDMGG_HoaDon FOREIGN KEY (MaHD)
        REFERENCES HoaDons(MaHD) ON DELETE CASCADE
);

-- 14. Dịch vụ phòng
CREATE TABLE DichVuPhongs (
    MaDVP      INT           NOT NULL IDENTITY(1,1) PRIMARY KEY,
    MaDatPhong INT           NOT NULL,
    MaLoaiDV   INT           NOT NULL,
    SoLuong    INT           NOT NULL DEFAULT 1,
    DonGia     DECIMAL(18,2) NOT NULL DEFAULT 0,
    NgayThem   DATETIME2     NOT NULL DEFAULT GETDATE(),
    GhiChu     NVARCHAR(500) NULL,
    CONSTRAINT FK_DichVuPhong_DatPhong   FOREIGN KEY (MaDatPhong)
        REFERENCES DatPhongs(MaDatPhong) ON DELETE CASCADE,
    CONSTRAINT FK_DichVuPhong_LoaiDichVu FOREIGN KEY (MaLoaiDV)
        REFERENCES LoaiDichVus(MaLoaiDV) ON DELETE NO ACTION
);
GO

-- ── Dữ liệu mẫu ──────────────────────────────────────────────────────────

INSERT INTO LoaiKhachHangs (MaCode, TenLoai, HeSoGia) VALUES
('NoiDia',    N'Nội địa',    1.0000),
('NuocNgoai', N'Nước ngoài', 1.5000);

INSERT INTO CauHinhs (ConfigKey, ConfigValue) VALUES
('SucChuaToiDa', '4'),
('TiLePhuThu',   '0.25'),
('ThueSuatVAT',  '10');

SET IDENTITY_INSERT NhanViens ON;
INSERT INTO NhanViens (MaNV, HoTen, TaiKhoan, MatKhau, VaiTro, CCCD, Email, SDT, NgayTao, IsActive) VALUES
(1, N'Nguyễn Văn Admin', 'admin',  '$2a$11$oZyF2ESXb93yz/sAxfADQ.y8lY41GbJdxIIZr0VSD5y6Ad4sqfGva', 'Admin',  '001085000001', 'admin@hotel.com',  '0901234560', '2026-05-25', 1),
(2, N'Trần Thị Quản Lý', 'quanly', '$2a$11$tzwSrDi27V1nmndwgVZnKOoz/bG4ikNF6yuX2fxen6GgcmYdAt7y6', 'QuanLy', '001085000002', 'quanly@hotel.com', '0901234561', '2026-05-25', 1),
(3, N'Lê Văn Lễ Tân',   'letan',  '$2a$11$3/IzprxXOgLMJFOl/sB8huLnqyRiQ.j63fNQulTEL47vNo2d/7PQa', 'LeTan',  '001085000003', 'letan@hotel.com',  '0901234562', '2026-05-25', 1);
SET IDENTITY_INSERT NhanViens OFF;

SET IDENTITY_INSERT LoaiPhongs ON;
INSERT INTO LoaiPhongs (MaLoaiPhong, TenLoaiPhong, GiaPhong, SucChua, MoTa) VALUES
(1, N'Phòng Đơn Standard',   500000, 1, N'Phòng đơn tiêu chuẩn, 1 giường đơn'),
(2, N'Phòng Đôi Standard',   800000, 2, N'Phòng đôi tiêu chuẩn, 1 giường đôi'),
(3, N'Phòng Gia Đình',      1200000, 4, N'Phòng gia đình, 2 giường đôi'),
(4, N'Phòng Deluxe',        1500000, 2, N'Phòng Deluxe, view biển'),
(5, N'Suite VIP',           3000000, 2, N'Suite cao cấp, phòng khách riêng');
SET IDENTITY_INSERT LoaiPhongs OFF;

SET IDENTITY_INSERT Phongs ON;
INSERT INTO Phongs (MaPhong, SoPhong, MaLoaiPhong, Tang, TrangThai) VALUES
( 1,'101',1,1,0),( 2,'102',1,1,0),( 3,'103',1,1,0),
( 4,'104',1,1,0),( 5,'105',1,1,0),( 6,'106',1,1,0),
( 7,'201',2,2,0),( 8,'202',2,2,0),( 9,'203',2,2,0),
(10,'204',2,2,0),(11,'205',2,2,0),(12,'206',2,2,0),
(13,'301',3,3,0),(14,'302',3,3,0),
(15,'303',4,3,0),(16,'304',4,3,0),
(17,'401',5,4,0),(18,'402',5,4,0);
SET IDENTITY_INSERT Phongs OFF;

SET IDENTITY_INSERT KhachHangs ON;
INSERT INTO KhachHangs (MaKH, HoTen, CMND, SDT, Email, DiaChi, QuocTich, MaLoaiKH, NgaySinh, GioiTinh, NgayTao) VALUES
( 1, N'Nguyễn Văn An',   '001085012345', '0901111001', 'an.nguyen@gmail.com',      N'12 Lý Thường Kiệt, Hà Nội',  N'Việt Nam',   1, '1990-03-15', 'Nam', '2026-05-25'),
( 2, N'Trần Thị Bình',   '079085067890', '0902222002', 'binh.tran@gmail.com',      N'45 Nguyễn Huệ, TP.HCM',      N'Việt Nam',   1, '1995-07-22', 'Nu',  '2026-05-25'),
( 3, N'Lê Minh Châu',    '048085034567', '0903333003', 'chau.le@gmail.com',        N'78 Trần Phú, Đà Nẵng',       N'Việt Nam',   1, '1988-11-05', 'Nam', '2026-05-25'),
( 4, N'Phạm Thu Hà',     '036085089012', '0904444004', 'ha.pham@gmail.com',        N'23 Hoàng Diệu, Huế',         N'Việt Nam',   1, '1993-05-18', 'Nu',  '2026-05-25'),
( 5, N'Võ Quốc Hùng',   '092085056789', '0905555005', 'hung.vo@gmail.com',        N'56 Pasteur, Cần Thơ',        N'Việt Nam',   1, '1985-09-30', 'Nam', '2026-05-25'),
( 6, N'John Smith',       'A12345678',   '+1-202-555-0101',   'john.smith@gmail.com', N'New York, USA',            N'Hoa Kỳ',     2, '1982-04-12', 'Nam', '2026-05-25'),
( 7, N'Wang Fang',        'G87654321',   '+86-138-0000-1234', 'wang.fang@qq.com',     N'Beijing, China',           N'Trung Quốc', 2, '1991-08-20', 'Nu',  '2026-05-25'),
( 8, N'Nguyễn Thị Lan',  '001090023456', '0908888008', 'lan.nguyen@yahoo.com',     N'99 Đinh Tiên Hoàng, Hà Nội', N'Việt Nam',   1, '1997-01-08', 'Nu',  '2026-05-25'),
( 9, N'Đặng Văn Đức',    '025090045678', '0909999009', 'duc.dang@gmail.com',       N'34 Lê Lợi, Hải Phòng',      N'Việt Nam',   1, '1989-06-25', 'Nam', '2026-05-25'),
(10, N'Tanaka Yuki',      'TK9876543',   '+81-90-1234-5678',  'tanaka.y@mail.jp',     N'Tokyo, Japan',             N'Nhật Bản',   2, '1994-03-03', 'Nu',  '2026-05-25');
SET IDENTITY_INSERT KhachHangs OFF;

INSERT INTO LoaiDichVus (TenLoaiDV, DonGia, DonViTinh, MoTa, IsActive) VALUES
(N'Bữa ăn sáng',   40000,  N'bữa',    N'Buffet sáng tại nhà hàng khách sạn', 1),
(N'Bữa ăn tối',    80000,  N'bữa',    N'Set menu tối',                        1),
(N'Bữa ăn trưa',   64000,  N'bữa',    N'Set menu trưa',                       1),
(N'Đặt xe taxi',  160000,  N'chuyến', N'Dịch vụ đặt taxi nội thành',          1),
(N'Giặt ủi',       40000,  N'kg',     N'Giặt ủi quần áo',                     1),
(N'Nước ngọt',     16000,  N'lon',    N'Nước ngọt đóng lon',                  1),
(N'Nước suối',     12000,  N'chai',   N'Nước khoáng 500ml',                   1),
(N'Spa / Massage', 240000, N'giờ',    N'Dịch vụ spa và massage thư giãn',     1);
GO

PRINT '============================================';
PRINT 'Database QuanLyKhachSan initialized OK.';
PRINT 'Accounts: admin/admin123 | quanly/quanly123 | letan/letan123';
PRINT '============================================';
