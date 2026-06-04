-- ============================================================
--  Migration: Thêm tính năng Mã Giảm Giá
--  Chạy script này để cập nhật database hiện có (KHÔNG xóa dữ liệu cũ)
--  Cách dùng: Mở SSMS → File → Open → chạy F5
-- ============================================================

USE QuanLyKhachSan;
GO

-- ── 1. Tạo bảng MaGiamGias (nếu chưa có) ──────────────────────────────────
IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'MaGiamGias') AND type = 'U')
BEGIN
    CREATE TABLE MaGiamGias (
        MaGG        INT           NOT NULL IDENTITY(1,1) PRIMARY KEY,
        TenMa       NVARCHAR(100) NOT NULL UNIQUE,
        MoTa        NVARCHAR(500) NULL,
        NgayBatDau  DATETIME2     NOT NULL,
        NgayKetThuc DATETIME2     NOT NULL,
        TrangThai   NVARCHAR(20)  NOT NULL DEFAULT 'Active',
        NgayTao     DATETIME2     NOT NULL DEFAULT GETDATE(),
        MaNVTao     INT           NULL,
        CONSTRAINT FK_MGG_NhanVien FOREIGN KEY (MaNVTao)
            REFERENCES NhanViens(MaNV) ON DELETE NO ACTION
    );
    PRINT 'Created table MaGiamGias';
END
GO

-- ── 2. Thêm cột TienGiam vào HoaDons (nếu chưa có) ────────────────────────
IF NOT EXISTS (
    SELECT 1 FROM sys.columns
    WHERE object_id = OBJECT_ID('HoaDons') AND name = 'TienGiam'
)
BEGIN
    ALTER TABLE HoaDons ADD TienGiam DECIMAL(18,2) NOT NULL DEFAULT 0;
    PRINT 'Added column TienGiam to HoaDons';
END
GO

-- ── 3. Thêm cột MaGG vào HoaDons (nếu chưa có) ────────────────────────────
IF NOT EXISTS (
    SELECT 1 FROM sys.columns
    WHERE object_id = OBJECT_ID('HoaDons') AND name = 'MaGG'
)
BEGIN
    ALTER TABLE HoaDons ADD MaGG INT NULL;
    PRINT 'Added column MaGG to HoaDons';
END
GO

-- ── 4. Thêm FK từ HoaDons.MaGG → MaGiamGias (nếu chưa có) ────────────────
IF NOT EXISTS (
    SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_HoaDon_MaGiamGia'
)
BEGIN
    ALTER TABLE HoaDons ADD CONSTRAINT FK_HoaDon_MaGiamGia
        FOREIGN KEY (MaGG) REFERENCES MaGiamGias(MaGG) ON DELETE NO ACTION;
    PRINT 'Added FK FK_HoaDon_MaGiamGia';
END
GO

-- ── 5. Tạo bảng ChiTietMaGiamGias (nếu chưa có) ───────────────────────────
IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'ChiTietMaGiamGias') AND type = 'U')
BEGIN
    CREATE TABLE ChiTietMaGiamGias (
        MaChiTiet  INT           NOT NULL IDENTITY(1,1) PRIMARY KEY,
        MaGG       INT           NOT NULL,
        LoaiApDung NVARCHAR(20)  NOT NULL DEFAULT 'LoaiPhong',
        MaLoai     INT           NOT NULL,
        TiLeGiam   DECIMAL(5,2)  NOT NULL DEFAULT 0,
        CONSTRAINT FK_CTMGG_MaGiamGia FOREIGN KEY (MaGG)
            REFERENCES MaGiamGias(MaGG) ON DELETE CASCADE
    );
    PRINT 'Created table ChiTietMaGiamGias';
END
GO

-- ── 6. Tạo bảng LichSuDungMaGiams (nếu chưa có) ───────────────────────────
IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'LichSuDungMaGiams') AND type = 'U')
BEGIN
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
    PRINT 'Created table LichSuDungMaGiams';
END
GO

PRINT '============================================';
PRINT 'Migration completed successfully!';
PRINT '============================================';
