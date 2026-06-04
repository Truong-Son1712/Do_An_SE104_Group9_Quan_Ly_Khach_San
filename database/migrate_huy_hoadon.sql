-- ============================================================
--  Migration: Hỗ trợ hủy hóa đơn
--  Thêm NgayTraPhongGoc vào HoaDons (lưu ngày trả phòng dự kiến ban đầu)
-- ============================================================
USE QuanLyKhachSan;
GO

IF NOT EXISTS (
    SELECT 1 FROM sys.columns
    WHERE object_id = OBJECT_ID('HoaDons') AND name = 'NgayTraPhongGoc'
)
BEGIN
    ALTER TABLE HoaDons ADD NgayTraPhongGoc DATETIME2 NULL;
    PRINT 'Added column NgayTraPhongGoc to HoaDons';
END
GO

PRINT 'Migration huy_hoadon completed!';
