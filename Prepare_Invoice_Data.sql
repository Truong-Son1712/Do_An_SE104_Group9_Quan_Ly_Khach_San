USE QuanLyKhachSan;
GO

-- 1. Thêm Khách hàng mẫu
INSERT INTO CUSTOMER (CustomerID, FullName, IdentityCard, Nationality, PhoneNumber, Address) VALUES
('KH001', N'Nguyễn Văn A', '123456789', N'Việt Nam', '0901111111', N'TP.HCM'),
('KH002', N'John Doe', '987654321', N'Nước ngoài', '0902222222', N'Mỹ'),
('KH003', N'Trần Thị C', '112233445', N'Việt Nam', '0903333333', N'Hà Nội');

-- 2. Thêm phiếu đặt phòng (Booking)
-- Booking 1: Khách hàng KH001 thuê phòng R001 (1 khách VN)
INSERT INTO BOOKING (BookingID, BookerID, RoomID, StaffID, CheckInDate, ExpectedCheckOutDate, NumberOfGuests) VALUES
('BK001', 'KH001', 'R001', 'NV002', DATEADD(DAY, -2, GETDATE()), GETDATE(), 1);

-- Booking 2: Khách hàng KH002 thuê phòng R002 (3 khách, có khách nước ngoài => Phụ thu 25% + Hệ số 1.5)
INSERT INTO BOOKING (BookingID, BookerID, RoomID, StaffID, CheckInDate, ExpectedCheckOutDate, NumberOfGuests) VALUES
('BK002', 'KH002', 'R002', 'NV002', DATEADD(DAY, -1, GETDATE()), GETDATE(), 3);

-- 3. Chi tiết khách hàng lưu trú (BOOKING_GUEST)
-- BK001 có 1 khách (KH001)
INSERT INTO BOOKING_GUEST (BookingID, CustomerID) VALUES
('BK001', 'KH001');

-- BK002 có 3 khách (KH001, KH002, KH003) -> Trong đó KH002 là khách nước ngoài
INSERT INTO BOOKING_GUEST (BookingID, CustomerID) VALUES
('BK002', 'KH001'),
('BK002', 'KH002'),
('BK002', 'KH003');

-- Thay đổi trạng thái phòng thành Đang Thuê
UPDATE ROOM SET Status = N'Đang thuê' WHERE RoomID IN ('R001', 'R002');
GO

-- Tạo các Stored Procedures cho Hóa Đơn
CREATE OR ALTER PROCEDURE sp_GetBookingsForCheckout
AS
BEGIN
    SELECT 
        b.BookingID, 
        b.RoomID, 
        r.RoomNumber, 
        c.FullName AS BookerName, 
        b.CheckInDate, 
        b.NumberOfGuests,
        r.PricePerDay
    FROM BOOKING b
    JOIN ROOM r ON b.RoomID = r.RoomID
    JOIN CUSTOMER c ON b.BookerID = c.CustomerID
    -- Lấy những booking chưa được thanh toán (chưa có trong bảng INVOICE)
    WHERE NOT EXISTS (SELECT 1 FROM INVOICE i WHERE i.BookingID = b.BookingID);
END
GO

CREATE OR ALTER PROCEDURE sp_GetBookingGuests
    @BookingID VARCHAR(20)
AS
BEGIN
    SELECT 
        c.CustomerID,
        c.FullName,
        c.Nationality
    FROM BOOKING_GUEST bg
    JOIN CUSTOMER c ON bg.CustomerID = c.CustomerID
    WHERE bg.BookingID = @BookingID;
END
GO

CREATE OR ALTER PROCEDURE sp_CreateInvoice
    @InvoiceID VARCHAR(20),
    @BookingID VARCHAR(20),
    @StaffID VARCHAR(20),
    @RoomCharge DECIMAL(18,2),
    @SurchargeAmount DECIMAL(18,2),
    @ServiceCharge DECIMAL(18,2),
    @TotalAmount DECIMAL(18,2),
    @PaymentMethod NVARCHAR(50)
AS
BEGIN
    -- Lưu hóa đơn
    INSERT INTO INVOICE (InvoiceID, BookingID, StaffID, IssuedDate, RoomCharge, SurchargeAmount, ServiceCharge, TotalAmount, PaymentMethod)
    VALUES (@InvoiceID, @BookingID, @StaffID, GETDATE(), @RoomCharge, @SurchargeAmount, @ServiceCharge, @TotalAmount, @PaymentMethod);

    -- Cập nhật trạng thái phòng thành Trống (hoặc Dọn dẹp)
    UPDATE ROOM 
    SET Status = N'Trống'
    WHERE RoomID = (SELECT RoomID FROM BOOKING WHERE BookingID = @BookingID);
END
GO
