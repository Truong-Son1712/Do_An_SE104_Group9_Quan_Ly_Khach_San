# Quản Lý Khách Sạn – Group 9 SE104

Hệ thống quản lý khách sạn xây dựng bằng **WPF (.NET 9)** + **SQL Server**.

---

## Tính năng chính

| Module | Chức năng |
|---|---|
| **Đặt Phòng** | Tạo / sửa / hủy đặt phòng; tách biệt *người đặt phòng* và *khách ở phòng*; kiểm tra trùng phòng |
| **Hóa Đơn** | Lập hóa đơn, thanh toán, hủy hóa đơn; hiển thị chi tiết breakdown giá (gốc + phụ thu loại khách + phụ thu sức chứa + VAT) |
| **Khách Hàng** | Quản lý khách hàng, phân loại (nội địa / nước ngoài) với hệ số giá riêng |
| **Phòng** | Quản lý phòng, loại phòng, sức chứa, trạng thái |
| **Dịch Vụ** | Ghi nhận và tính tiền dịch vụ phòng |
| **Mã Giảm Giá** | Tạo / quản lý mã giảm giá theo loại phòng hoặc loại dịch vụ |
| **Nhân Viên** | Phân quyền 3 cấp: Admin / Quản Lý / Lễ Tân |
| **Báo Cáo** | Thống kê doanh thu, công suất phòng |
| **Cấu Hình** | Điều chỉnh hệ số giá loại khách, tỉ lệ phụ thu, VAT, sức chứa tối đa |

---

## Tài khoản mặc định

| Tài khoản | Mật khẩu    | Vai trò  | Quyền hạn |
|-----------|-------------|----------|-----------|
| `admin`   | `admin123`  | Admin    | Toàn quyền |
| `quanly`  | `quanly123` | Quản Lý  | Quản lý nghiệp vụ, hủy hóa đơn |
| `letan`   | `letan123`  | Lễ Tân   | Đặt phòng, check-in/out, dịch vụ |

---

## Hướng dẫn chạy sau khi clone

### Yêu cầu cài đặt

| Phần mềm | Phiên bản | Ghi chú |
|---|---|---|
| SQL Server | Express / Developer / Standard | [Tải tại đây](https://www.microsoft.com/sql-server/sql-server-downloads) |

> **.NET 9 không cần cài riêng** — file exe trong thư mục `Release/` đã được đóng gói self-contained (bao gồm runtime bên trong).

---

### Bước 1 – Tạo database

Mở **SQL Server Management Studio (SSMS)** → mở file:

```
database/init_sqlserver.sql
```

Nhấn **F5** để chạy. Script sẽ tự động:
- Tạo database `QuanLyKhachSan`
- Tạo toàn bộ bảng và quan hệ
- Chèn dữ liệu mẫu (nhân viên, phòng, khách hàng, cấu hình)

Hoặc chạy bằng command line:
```bash
sqlcmd -S . -E -i database\init_sqlserver.sql
```

---

### Bước 2 – Cấu hình kết nối

Mở file **`Release\appsettings.json`** và chỉnh connection string cho phù hợp:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=.;Database=QuanLyKhachSan;Trusted_Connection=True;TrustServerCertificate=True;"
  }
}
```

**Các trường hợp phổ biến:**

```
# Windows Authentication (mặc định)
Server=.;Database=QuanLyKhachSan;Trusted_Connection=True;TrustServerCertificate=True;

# Named instance (ví dụ SQLEXPRESS)
Server=.\SQLEXPRESS;Database=QuanLyKhachSan;Trusted_Connection=True;TrustServerCertificate=True;

# SQL Server Authentication
Server=.;Database=QuanLyKhachSan;User ID=sa;Password=YourPassword;TrustServerCertificate=True;
```

> Tên instance SQL Server xem trong SSMS ở góc trên bên trái khi kết nối (ví dụ: `DESKTOP-ABC\SQLEXPRESS`).

---

### Bước 3 – Chạy ứng dụng

Double-click file **`Release\Quản Lý Khách Sạn.exe`**.

> Lần đầu chạy sẽ mất vài giây do giải nén runtime vào thư mục temp.

Hoặc build từ source (cần cài .NET 9 SDK):
```bash
cd "Đồ Án Quản Lý Khách Sạn\Đồ Án Quản Lý Khách Sạn"
dotnet run
```

---

## Cấu trúc thư mục

```
📁 database/
   └── init_sqlserver.sql          # Script khởi tạo database (chạy 1 lần)
📁 Release/
   ├── Quản Lý Khách Sạn.exe       # File thực thi (self-contained, ~179 MB)
   └── appsettings.json            # Cấu hình kết nối database
📁 Đồ Án Quản Lý Khách Sạn/
   └── Đồ Án Quản Lý Khách Sạn/
       ├── appsettings.json        # Cấu hình kết nối (dùng khi chạy từ source)
       ├── Data/                   # DbContext, DatabaseInitializer
       ├── Models/                 # Entity models
       ├── ViewModels/             # MVVM ViewModels
       ├── Views/                  # XAML UI
       └── Helpers/                # AppConfig, SessionManager, Converters
```

---

## Công nghệ sử dụng

| Thành phần | Chi tiết |
|---|---|
| **UI Framework** | WPF (Windows Presentation Foundation) |
| **Database** | SQL Server + Entity Framework Core 9 |
| **Architecture** | MVVM (Model-View-ViewModel) |
| **Authentication** | BCrypt password hashing |
| **Target OS** | Windows 10/11 x64 |
