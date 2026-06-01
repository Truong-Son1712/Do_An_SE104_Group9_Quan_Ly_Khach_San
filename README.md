# Quản Lý Khách Sạn – Group 9 SE104

Hệ thống quản lý khách sạn xây dựng bằng **WPF (.NET 9)** + **SQL Server**.

---

## Tài khoản mặc định

| Tài khoản | Mật khẩu  | Vai trò    |
|-----------|-----------|------------|
| `admin`   | `admin123`  | Admin      |
| `quanly`  | `quanly123` | Quản Lý    |
| `letan`   | `letan123`  | Lễ Tân     |

---

## Hướng dẫn chạy sau khi clone

### Yêu cầu cài đặt

| Phần mềm | Phiên bản | Link tải |
|---|---|---|
| SQL Server | Express / Developer / Standard | [microsoft.com/sql-server](https://www.microsoft.com/sql-server/sql-server-downloads) |
| .NET 9 Windows Desktop Runtime | 9.0 trở lên | [dotnet.microsoft.com](https://dotnet.microsoft.com/download/dotnet/9.0) |

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

Mở file **`appsettings.json`** (nằm cùng thư mục với file `.exe`) và chỉnh sửa connection string cho phù hợp với máy:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=.;Database=QuanLyKhachSan;Trusted_Connection=True;TrustServerCertificate=True;"
  }
}
```

**Các trường hợp phổ biến:**

```
# SQL Server mặc định trên máy local (Windows Auth) – mặc định
Server=.;Database=QuanLyKhachSan;Trusted_Connection=True;TrustServerCertificate=True;

# Named instance (ví dụ: DESKTOP-ABC\SQLEXPRESS)
Server=DESKTOP-ABC\SQLEXPRESS;Database=QuanLyKhachSan;Trusted_Connection=True;TrustServerCertificate=True;

# SQL Server Authentication (sa/password)
Server=.;Database=QuanLyKhachSan;User ID=sa;Password=YourPassword;TrustServerCertificate=True;
```

> **Lưu ý:** Tên instance SQL Server có thể xem trong SSMS ở góc trên bên trái khi kết nối (ví dụ: `DESKTOP-ABC\SQLEXPRESS`).

---

### Bước 3 – Chạy ứng dụng

Double-click file **`Quản Lý Khách Sạn.exe`** trong thư mục gốc của repo.

Hoặc build từ source:
```bash
cd "Đồ Án Quản Lý Khách Sạn\Đồ Án Quản Lý Khách Sạn"
dotnet run
```

---

## Cấu trúc thư mục

```
📁 database/
   └── init_sqlserver.sql     # Script khởi tạo SQL Server
📁 Đồ Án Quản Lý Khách Sạn/
   └── Đồ Án Quản Lý Khách Sạn/
       ├── appsettings.json   # Cấu hình kết nối database
       ├── Data/              # DbContext, DatabaseInitializer
       ├── Models/            # Entity models
       ├── ViewModels/        # MVVM ViewModels
       ├── Views/             # XAML UI
       └── Helpers/           # Utilities, AppConfig, Converters
```

---

## Công nghệ sử dụng

- **UI:** WPF (Windows Presentation Foundation)
- **Database:** SQL Server + Entity Framework Core 9
- **Pattern:** MVVM
- **Auth:** BCrypt password hashing
