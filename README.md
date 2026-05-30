# Do_An_SE104_Group9_Quan_Ly_Khach_San
Repostory of Group 9 for SE104

## ⚠️ LƯU Ý

Hiện tại đang sử dụng một số dữ liệu giả để hoàn thiện UI. Có gì thay đổi:

1. Trong `WpfApp1/ViewModels/LoginViewModel.cs`, đang có đoạn code nhập `admin / 1` để vào thẳng phần mềm không cần check Database. Xoá/comment đoạn này đi khi đã có DB.
2. Trong `WpfApp1/DAL/DatabaseConnection.cs`, Data Source đang tạm đổi thành `.` (máy cục bộ mặc định). Nhớ đổi lại
3. Trong `WpfApp1/BLL/ReportService.cs`, các hàm lấy doanh thu đang tự generate ra số liệu random. Khi nào xây dựng xong bảng HOADON thì thay bằng hàm thật nha
