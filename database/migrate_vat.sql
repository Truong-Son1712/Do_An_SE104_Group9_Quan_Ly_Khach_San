-- Migration: Thêm thuế VAT
USE QuanLyKhachSan;
GO

-- 1. Cấu hình VAT mặc định 10%
IF NOT EXISTS (SELECT 1 FROM CauHinhs WHERE ConfigKey = 'ThueSuatVAT')
BEGIN
    INSERT INTO CauHinhs (ConfigKey, ConfigValue) VALUES ('ThueSuatVAT', '10');
    PRINT 'Added ThueSuatVAT = 10 to CauHinhs';
END
GO

-- 2. Thêm TienVAT vào HoaDons
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('HoaDons') AND name = 'TienVAT')
BEGIN
    ALTER TABLE HoaDons ADD TienVAT DECIMAL(18,2) NOT NULL DEFAULT 0;
    PRINT 'Added TienVAT to HoaDons';
END
GO

-- 3. Thêm VATPercent vào HoaDons (lưu % đã dùng lúc lập HĐ)
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('HoaDons') AND name = 'VATPercent')
BEGIN
    ALTER TABLE HoaDons ADD VATPercent DECIMAL(5,2) NOT NULL DEFAULT 0;
    PRINT 'Added VATPercent to HoaDons';
END
GO

PRINT 'Migration VAT completed!';
