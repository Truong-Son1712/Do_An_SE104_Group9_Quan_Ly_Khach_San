-- Migration: Thêm CCCD cho NhanViens
USE QuanLyKhachSan;
GO

IF NOT EXISTS (
    SELECT 1 FROM sys.columns
    WHERE object_id = OBJECT_ID('NhanViens') AND name = 'CCCD'
)
BEGIN
    ALTER TABLE NhanViens ADD CCCD NVARCHAR(30) NULL;
    PRINT 'Added column CCCD to NhanViens';
END
GO

PRINT 'Migration nhanvien_cccd completed!';
