-- ============================================================
--  Migration v2: Thêm SoLuongToiDa, bỏ UNIQUE constraint TenMa
--  Chạy trên database hiện có (KHÔNG xóa dữ liệu)
-- ============================================================
USE QuanLyKhachSan;
GO

-- 1. Thêm cột SoLuongToiDa (NULL = vô hạn)
IF NOT EXISTS (
    SELECT 1 FROM sys.columns
    WHERE object_id = OBJECT_ID('MaGiamGias') AND name = 'SoLuongToiDa'
)
BEGIN
    ALTER TABLE MaGiamGias ADD SoLuongToiDa INT NULL;
    PRINT 'Added column SoLuongToiDa to MaGiamGias';
END
GO

-- 2. Xóa UNIQUE constraint trên TenMa (nếu có)
--    Tên constraint có thể là UQ__MaGiamGi... (auto-generated) hoặc đặt tên rõ
DECLARE @constraintName NVARCHAR(200);
SELECT @constraintName = i.name
FROM sys.indexes i
INNER JOIN sys.index_columns ic ON i.object_id = ic.object_id AND i.index_id = ic.index_id
INNER JOIN sys.columns c ON ic.object_id = c.object_id AND ic.column_id = c.column_id
WHERE i.object_id = OBJECT_ID('MaGiamGias')
  AND i.is_unique = 1
  AND c.name = 'TenMa';

IF @constraintName IS NOT NULL
BEGIN
    EXEC('ALTER TABLE MaGiamGias DROP CONSTRAINT [' + @constraintName + ']');
    PRINT 'Dropped UNIQUE constraint on TenMa';
END
GO

PRINT 'Migration v2 completed!';
GO
