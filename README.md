# Quản Lý Khách Sạn (Nhóm 9)

Mã nguồn được viết bằng WPF C# và SQL Server (Mô hình 3 lớp).

## Bàn giao tính năng Hóa đơn & Thanh toán (Thành viên 4)
Nhánh `feature/hoa-don` đã hoàn thành 100% công việc của Thành viên 4, bao gồm:
1. **Giao diện:** Xem phòng chờ thanh toán, Lịch sử hóa đơn, Tìm kiếm theo tên/phòng.
2. **Nghiệp vụ:** Tính tiền phòng, tự động áp dụng phụ thu (Quy định 4: 25% cho khách thứ 3, x1.5 nếu có khách nước ngoài).
3. **Checkout:** Tự động chuyển trạng thái phòng thành "Trống" sau khi thanh toán.
4. **PDF:** Tự động xuất file PDF hóa đơn tiếng Việt bằng thư viện QuestPDF.

---

## Hướng dẫn chạy thử (Dành cho nhóm)

1. **Database:** 
   Chạy 3 file SQL theo thứ tự: `Create_Database.sql` -> `Fix_Database.sql` -> `Prepare_Invoice_Data.sql`.

2. **Thư viện:** 
   Cài đặt gói NuGet `QuestPDF` bằng lệnh:
   ```bash
   dotnet add package QuestPDF
   ```

3. **Chạy Code:** 
   - Code giao diện Hóa Đơn đã được nhúng sẵn vào Menu bên trái của MainWindow.
   - Các bạn (TV 2, TV 3) cứ code chức năng Đặt phòng bình thường, khi có dữ liệu nó sẽ tự động đổ sang màn hình Hóa đơn.
