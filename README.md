# 🏨 Đồ Án Quản Lý Khách Sạn - Nhóm 9 (SE104)

Dự án phần mềm quản lý khách sạn được phát triển với mô hình 3 lớp (3-Tier Architecture) trên nền tảng WPF C# và SQL Server.

## 🚀 Báo cáo tiến độ - Thành viên 4 (Module Hóa Đơn & Thanh Toán)

Nhánh `feature/hoa-don` chứa toàn bộ mã nguồn liên quan đến chức năng Thanh toán phòng (Checkout) và Quản lý Hóa đơn do **Thành viên 4** phụ trách. 
Toàn bộ 6/6 yêu cầu của Thành viên 4 trong tài liệu phân công đồ án đã được hoàn thiện 100%.

### 📋 Các tính năng đã hoàn thành
1. **Giao diện Quản lý Hóa đơn (`InvoiceUserControl.xaml`)**
   - Lấy danh sách các phòng **Đang chờ thanh toán**.
   - Bổ sung **Ô tìm kiếm thông minh (Real-time Search)** theo tên khách, phòng, mã phiếu.
   - Bổ sung tab **Lịch sử hóa đơn** liệt kê các hóa đơn đã thanh toán.
2. **Cửa sổ Thanh toán (Checkout) (`CheckoutWindow.xaml`)**
   - Xử lý nghiệp vụ trả phòng, tự động gộp dữ liệu khách hàng, tính tổng số ngày ở.
   - Xử lý thuật toán phụ thu cực kỳ chặt chẽ theo Quy Định 4:
     - Phụ thu 25% nếu phòng có 3 khách.
     - Nhân hệ số 1.5 nếu phòng có chứa khách nước ngoài.
   - Cho phép chọn hình thức thanh toán (Tiền mặt / Chuyển khoản / Quẹt thẻ).
3. **Cơ sở dữ liệu (DAL & SQL)**
   - Cập nhật tự động trạng thái phòng từ **Đang thuê** sang **Trống** ngay khi thanh toán.
   - Lưu trữ hóa đơn vào bảng `INVOICE`.
4. **Xuất file PDF Hóa đơn siêu đẹp (`PdfExportHelper.cs`)**
   - Tích hợp thư viện **QuestPDF** thế hệ mới (Thay cho iText7 bị lỗi font).
   - Hỗ trợ hiển thị 100% tiếng Việt có dấu.
   - Giao diện hóa đơn có kẻ bảng giống siêu thị, tự động gọi hộp thoại chọn nơi lưu (`SaveFileDialog`) và bật file PDF lên sau khi lưu xong.

---

## 🛠 Hướng dẫn tích hợp cho các thành viên khác

### 1. Cập nhật Database
Khi kéo code từ nhánh này về, bạn bắt buộc phải chạy 3 file SQL theo thứ tự sau để cập nhật cơ sở dữ liệu trên máy tính của bạn:
1. `Create_Database.sql`: Tạo bảng ban đầu.
2. `Fix_Database.sql`: Sửa lỗi bảng và bổ sung dữ liệu nhân viên.
3. `Prepare_Invoice_Data.sql`: Script cực kỳ quan trọng chứa các `Stored Procedures` (`sp_CreateInvoice`, `sp_GetBookingsForCheckout`, `sp_GetAllInvoices`) và dữ liệu test mẫu cho tính năng Checkout.

### 2. Cài đặt NuGet Packages
Thành viên 4 đã sử dụng thư viện `QuestPDF` để xuất PDF. Nếu máy bạn báo lỗi đỏ, hãy mở Terminal/Package Manager Console lên và chạy:
```bash
dotnet add package QuestPDF
```

### 3. Tích hợp giao diện (GUI)
Giao diện `InvoiceUserControl` đã được nhúng sẵn vào Menu của `MainWindow`. Nếu các bạn làm các Module Quản lý Phòng (TV 2) hay Booking (TV 3), cứ làm bình thường. Chỉ cần khi các bạn tạo Booking xong, các bạn `INSERT` vào DB đúng quy chuẩn thì phòng đó sẽ tự động xuất hiện ở màn hình Hóa đơn của mình!

---
*Chúc cả nhóm tích hợp thành công và hoàn thành đồ án xuất sắc! ✨*
