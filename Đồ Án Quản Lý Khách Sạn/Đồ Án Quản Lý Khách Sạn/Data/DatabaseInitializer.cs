using Microsoft.EntityFrameworkCore;
using Đồ_Án_Quản_Lý_Khách_Sạn.Helpers;
using Đồ_Án_Quản_Lý_Khách_Sạn.Models;

namespace Đồ_Án_Quản_Lý_Khách_Sạn.Data
{
    public static class DatabaseInitializer
    {
        public static void Initialize(HotelDbContext ctx)
        {
            // Tạo database và tất cả bảng nếu chưa tồn tại (SQL Server)
            ctx.Database.EnsureCreated();

            // Migration thủ công: thêm bảng mới nếu chưa tồn tại
            ctx.Database.ExecuteSqlRaw(@"
                IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'LoaiDichVus')
                CREATE TABLE LoaiDichVus (
                    MaLoaiDV  INT           NOT NULL IDENTITY(1,1) PRIMARY KEY,
                    TenLoaiDV NVARCHAR(200) NOT NULL,
                    DonGia    DECIMAL(18,2) NOT NULL DEFAULT 0,
                    DonViTinh NVARCHAR(50)  NOT NULL DEFAULT '',
                    MoTa      NVARCHAR(500) NULL,
                    IsActive  BIT           NOT NULL DEFAULT 1
                );");

            // Migration: thêm các cột breakdown giá phòng vào HoaDons
            ctx.Database.ExecuteSqlRaw(@"
                IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME='HoaDons' AND COLUMN_NAME='GiaPhongGoc')
                ALTER TABLE HoaDons ADD GiaPhongGoc DECIMAL(18,2) NOT NULL DEFAULT 0;
                IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME='HoaDons' AND COLUMN_NAME='HeSoLoaiKhach')
                ALTER TABLE HoaDons ADD HeSoLoaiKhach DECIMAL(10,4) NOT NULL DEFAULT 1;
                IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME='HoaDons' AND COLUMN_NAME='TenLoaiKhachMax')
                ALTER TABLE HoaDons ADD TenLoaiKhachMax NVARCHAR(100) NULL;
                IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME='HoaDons' AND COLUMN_NAME='TiLePhuThuSucChua')
                ALTER TABLE HoaDons ADD TiLePhuThuSucChua DECIMAL(5,4) NOT NULL DEFAULT 0;");

            // Migration: thêm cột NguoiDatPhongOPhong vào DatPhongs nếu chưa có
            ctx.Database.ExecuteSqlRaw(@"
                IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS
                               WHERE TABLE_NAME = 'DatPhongs' AND COLUMN_NAME = 'NguoiDatPhongOPhong')
                ALTER TABLE DatPhongs ADD NguoiDatPhongOPhong BIT NOT NULL DEFAULT 1;");

            // Migration: thêm GioiTinh, NgaySinh, NgayVaoLam vào NhanViens
            ctx.Database.ExecuteSqlRaw(@"
                IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME='NhanViens' AND COLUMN_NAME='GioiTinh')
                    ALTER TABLE NhanViens ADD GioiTinh NVARCHAR(10) NOT NULL DEFAULT 'Nam';
                IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME='NhanViens' AND COLUMN_NAME='NgaySinh')
                    ALTER TABLE NhanViens ADD NgaySinh DATETIME2 NULL;
                IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME='NhanViens' AND COLUMN_NAME='NgayVaoLam')
                    ALTER TABLE NhanViens ADD NgayVaoLam DATETIME2 NULL;");

            ctx.Database.ExecuteSqlRaw(@"
                IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'DichVuPhongs')
                CREATE TABLE DichVuPhongs (
                    MaDVP      INT           NOT NULL IDENTITY(1,1) PRIMARY KEY,
                    MaDatPhong INT           NOT NULL,
                    MaLoaiDV   INT           NOT NULL,
                    SoLuong    INT           NOT NULL DEFAULT 1,
                    DonGia     DECIMAL(18,2) NOT NULL DEFAULT 0,
                    NgayThem   DATETIME2     NOT NULL DEFAULT GETDATE(),
                    GhiChu     NVARCHAR(500) NULL,
                    CONSTRAINT FK_DichVuPhong_DatPhong  FOREIGN KEY (MaDatPhong) REFERENCES DatPhongs(MaDatPhong)  ON DELETE CASCADE,
                    CONSTRAINT FK_DichVuPhong_LoaiDichVu FOREIGN KEY (MaLoaiDV) REFERENCES LoaiDichVus(MaLoaiDV) ON DELETE NO ACTION
                );");

            if (!ctx.LoaiPhongs.Any())  SeedLoaiPhong(ctx);
            if (!ctx.Phongs.Any())      SeedPhong(ctx);
            if (!ctx.KhachHangs.Any())  SeedKhachHang(ctx);

            // Seed cấu hình mặc định
if (ctx.CauHinhs.Find("SucChuaToiDa") == null)
                ctx.CauHinhs.Add(new CauHinh { ConfigKey = "SucChuaToiDa",  ConfigValue = "4"   });
            if (ctx.CauHinhs.Find("TiLePhuThu") == null)
                ctx.CauHinhs.Add(new CauHinh { ConfigKey = "TiLePhuThu",    ConfigValue = "0.25" });
            if (ctx.CauHinhs.Find("TiLeCoc") == null)
                ctx.CauHinhs.Add(new CauHinh { ConfigKey = "TiLeCoc",       ConfigValue = "30"   });

            // Seed loại khách hàng mặc định (tra theo MaCode vì MaLKH là auto-increment)
            if (!ctx.LoaiKhachHangs.Any(l => l.MaCode == "NoiDia"))
                ctx.LoaiKhachHangs.Add(new LoaiKhachHang { MaCode = "NoiDia",    TenLoai = "Nội địa",    HeSoGia = 1.0m });
            if (!ctx.LoaiKhachHangs.Any(l => l.MaCode == "NuocNgoai"))
                ctx.LoaiKhachHangs.Add(new LoaiKhachHang { MaCode = "NuocNgoai", TenLoai = "Nước ngoài", HeSoGia = 1.2m });

            // Seed loại dịch vụ mặc định
            if (!ctx.LoaiDichVus.Any())
            {
                ctx.LoaiDichVus.AddRange(
                    new LoaiDichVu { TenLoaiDV = "Bữa ăn sáng",  DonGia =  50_000, DonViTinh = "bữa",    MoTa = "Buffet sáng tại nhà hàng khách sạn" },
                    new LoaiDichVu { TenLoaiDV = "Bữa ăn trưa",  DonGia =  80_000, DonViTinh = "bữa",    MoTa = "Set menu trưa" },
                    new LoaiDichVu { TenLoaiDV = "Bữa ăn tối",   DonGia = 100_000, DonViTinh = "bữa",    MoTa = "Set menu tối" },
                    new LoaiDichVu { TenLoaiDV = "Nước suối",    DonGia =  15_000, DonViTinh = "chai",   MoTa = "Nước khoáng 500ml" },
                    new LoaiDichVu { TenLoaiDV = "Nước ngọt",    DonGia =  20_000, DonViTinh = "lon",    MoTa = "Nước ngọt đóng lon" },
                    new LoaiDichVu { TenLoaiDV = "Đặt xe taxi",  DonGia = 200_000, DonViTinh = "chuyến", MoTa = "Dịch vụ đặt taxi nội thành" },
                    new LoaiDichVu { TenLoaiDV = "Giặt ủi",      DonGia =  50_000, DonViTinh = "kg",     MoTa = "Giặt ủi quần áo" },
                    new LoaiDichVu { TenLoaiDV = "Spa / Massage", DonGia = 300_000, DonViTinh = "giờ",   MoTa = "Dịch vụ spa và massage thư giãn" }
                );
            }

            ctx.SaveChanges();

            // Migration: tạo bảng LoaiNhanViens và LoaiNhanVienQuyens nếu chưa có
            ctx.Database.ExecuteSqlRaw(@"
                IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'LoaiNhanViens')
                CREATE TABLE LoaiNhanViens (
                    MaLoaiNV   INT           NOT NULL IDENTITY(1,1) PRIMARY KEY,
                    TenLoai    NVARCHAR(100) NOT NULL,
                    MoTa       NVARCHAR(500) NULL,
                    VaiTroCode NVARCHAR(50)  NOT NULL,
                    IsBuiltIn  BIT           NOT NULL DEFAULT 0,
                    CONSTRAINT UQ_LoaiNV_VaiTroCode UNIQUE (VaiTroCode)
                );");
            ctx.Database.ExecuteSqlRaw(@"
                IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'LoaiNhanVienQuyens')
                CREATE TABLE LoaiNhanVienQuyens (
                    MaLoaiNV INT           NOT NULL,
                    MaQuyen  NVARCHAR(100) NOT NULL,
                    CONSTRAINT PK_LoaiNVQuyen PRIMARY KEY (MaLoaiNV, MaQuyen),
                    CONSTRAINT FK_LNVQuyen_LoaiNV FOREIGN KEY (MaLoaiNV)
                        REFERENCES LoaiNhanViens(MaLoaiNV) ON DELETE CASCADE
                );");

            // Seed loại nhân viên tích hợp (built-in) – phải seed TRƯỚC NhanViens
            SeedLoaiNhanVien(ctx);

            // Migration: thêm cột MaLoaiNV FK vào NhanViens (liên kết với LoaiNhanViens)
            // Seed NhanViens sau khi đã có LoaiNhanViens
            if (!ctx.NhanViens.Any()) SeedNhanVien(ctx);
            ctx.Database.ExecuteSqlRaw(@"
                IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS
                               WHERE TABLE_NAME = 'NhanViens' AND COLUMN_NAME = 'MaLoaiNV')
                    ALTER TABLE NhanViens ADD MaLoaiNV INT NULL;");

            // Populate MaLoaiNV cho các bản ghi đã tồn tại dựa theo VaiTro = VaiTroCode
            ctx.Database.ExecuteSqlRaw(@"
                UPDATE n SET n.MaLoaiNV = l.MaLoaiNV
                FROM NhanViens n
                JOIN LoaiNhanViens l ON n.VaiTro = l.VaiTroCode
                WHERE n.MaLoaiNV IS NULL;");

            // Thêm FK constraint nếu chưa có
            ctx.Database.ExecuteSqlRaw(@"
                IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLE_CONSTRAINTS
                               WHERE CONSTRAINT_NAME = 'FK_NhanVien_LoaiNhanVien')
                    ALTER TABLE NhanViens ADD CONSTRAINT FK_NhanVien_LoaiNhanVien
                        FOREIGN KEY (MaLoaiNV) REFERENCES LoaiNhanViens(MaLoaiNV) ON DELETE NO ACTION;");

            // Migration: tạo bảng NhanVienQuyens nếu chưa có
            ctx.Database.ExecuteSqlRaw(@"
                IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'NhanVienQuyens')
                CREATE TABLE NhanVienQuyens (
                    MaNV    INT           NOT NULL,
                    MaQuyen NVARCHAR(100) NOT NULL,
                    CONSTRAINT PK_NhanVienQuyen PRIMARY KEY (MaNV, MaQuyen),
                    CONSTRAINT FK_NVQ_NhanVien FOREIGN KEY (MaNV)
                        REFERENCES NhanViens(MaNV) ON DELETE CASCADE
                );");

            // Seed quyền mặc định cho nhân viên hiện có chưa có quyền nào
            SeedQuyenMacDinh(ctx);
        }

        private static void SeedLoaiNhanVien(HotelDbContext ctx)
        {
            // Seed 3 loại tích hợp nếu chưa có
            void EnsureBuiltIn(string vaiTroCode, string tenLoai, string moTa, IEnumerable<string> quyens)
            {
                if (ctx.LoaiNhanViens.Any(l => l.VaiTroCode == vaiTroCode)) return;
                var loai = new LoaiNhanVien
                    { TenLoai = tenLoai, MoTa = moTa, VaiTroCode = vaiTroCode, IsBuiltIn = true };
                ctx.LoaiNhanViens.Add(loai);
                ctx.SaveChanges();
                foreach (var q in quyens)
                    ctx.LoaiNhanVienQuyens.Add(new LoaiNhanVienQuyen { MaLoaiNV = loai.MaLoaiNV, MaQuyen = q });
                ctx.SaveChanges();
            }

            EnsureBuiltIn("Admin",  "Quản Trị Viên", "Toàn quyền hệ thống (không thể thay đổi)",
                Array.Empty<string>()); // Admin bypass tất cả, không cần lưu quyền

            EnsureBuiltIn("QuanLy", "Quản Lý", "Quản lý nghiệp vụ khách sạn",
                Quyen.MacDinhQuanLy);

            EnsureBuiltIn("LeTan",  "Lễ Tân",  "Tiếp nhận và phục vụ khách",
                Quyen.MacDinhLeTan);
        }

        private static void SeedQuyenMacDinh(HotelDbContext ctx)
        {
            // Với mỗi nhân viên không phải Admin và chưa có bất kỳ quyền nào → cấp quyền mặc định theo vai trò
            var nvChuaCoQuyen = ctx.NhanViens
                .Where(n => n.VaiTro != "Admin" && !ctx.NhanVienQuyens.Any(q => q.MaNV == n.MaNV))
                .ToList();

            foreach (var nv in nvChuaCoQuyen)
            {
                var defaults = nv.VaiTro == "QuanLy" ? Quyen.MacDinhQuanLy : Quyen.MacDinhLeTan;
                foreach (var q in defaults)
                    ctx.NhanVienQuyens.Add(new NhanVienQuyen { MaNV = nv.MaNV, MaQuyen = q });
            }
            if (nvChuaCoQuyen.Any()) ctx.SaveChanges();
        }

        private static string Hash(string pw) => BCrypt.Net.BCrypt.HashPassword(pw);

        private static void SeedNhanVien(HotelDbContext ctx)
        {
            int? adminId  = ctx.LoaiNhanViens.FirstOrDefault(l => l.VaiTroCode == "Admin")?.MaLoaiNV;
            int? quanLyId = ctx.LoaiNhanViens.FirstOrDefault(l => l.VaiTroCode == "QuanLy")?.MaLoaiNV;
            int? leTanId  = ctx.LoaiNhanViens.FirstOrDefault(l => l.VaiTroCode == "LeTan")?.MaLoaiNV;

            ctx.NhanViens.AddRange(
                new NhanVien { HoTen = "Nguyễn Văn Admin", TaiKhoan = "admin",  MatKhau = Hash("admin123"),  VaiTro = "Admin",  MaLoaiNV = adminId,  Email = "admin@hotel.com",  SDT = "0901234560", IsActive = true },
                new NhanVien { HoTen = "Trần Thị Quản Lý", TaiKhoan = "quanly", MatKhau = Hash("quanly123"), VaiTro = "QuanLy", MaLoaiNV = quanLyId, Email = "quanly@hotel.com", SDT = "0901234561", IsActive = true },
                new NhanVien { HoTen = "Lê Văn Lễ Tân",    TaiKhoan = "letan",  MatKhau = Hash("letan123"),  VaiTro = "LeTan",  MaLoaiNV = leTanId,  Email = "letan@hotel.com",  SDT = "0901234562", IsActive = true }
            );
        }

        private static void SeedLoaiPhong(HotelDbContext ctx)
        {
            ctx.LoaiPhongs.AddRange(
                new LoaiPhong { TenLoaiPhong = "Phòng Đơn Standard",  GiaPhong =   500_000, SucChua = 1, MoTa = "Phòng đơn tiêu chuẩn, 1 giường đơn" },
                new LoaiPhong { TenLoaiPhong = "Phòng Đôi Standard",  GiaPhong =   800_000, SucChua = 2, MoTa = "Phòng đôi tiêu chuẩn, 1 giường đôi" },
                new LoaiPhong { TenLoaiPhong = "Phòng Gia Đình",      GiaPhong = 1_200_000, SucChua = 4, MoTa = "Phòng gia đình, 2 giường đôi" },
                new LoaiPhong { TenLoaiPhong = "Phòng Deluxe",        GiaPhong = 1_500_000, SucChua = 2, MoTa = "Phòng Deluxe, view biển" },
                new LoaiPhong { TenLoaiPhong = "Suite VIP",           GiaPhong = 3_000_000, SucChua = 2, MoTa = "Suite cao cấp, phòng khách riêng" }
            );
        }

        private static void SeedPhong(HotelDbContext ctx)
        {
            for (int i = 1; i <= 6; i++)
                ctx.Phongs.Add(new Phong { SoPhong = $"1{i:D2}", MaLoaiPhong = 1, Tang = 1, TrangThai = TrangThaiPhong.TrongSach });
            for (int i = 1; i <= 6; i++)
                ctx.Phongs.Add(new Phong { SoPhong = $"2{i:D2}", MaLoaiPhong = 2, Tang = 2, TrangThai = TrangThaiPhong.TrongSach });
            ctx.Phongs.Add(new Phong { SoPhong = "301", MaLoaiPhong = 3, Tang = 3, TrangThai = TrangThaiPhong.TrongSach });
            ctx.Phongs.Add(new Phong { SoPhong = "302", MaLoaiPhong = 3, Tang = 3, TrangThai = TrangThaiPhong.TrongSach });
            ctx.Phongs.Add(new Phong { SoPhong = "303", MaLoaiPhong = 4, Tang = 3, TrangThai = TrangThaiPhong.TrongSach });
            ctx.Phongs.Add(new Phong { SoPhong = "304", MaLoaiPhong = 4, Tang = 3, TrangThai = TrangThaiPhong.TrongSach });
            ctx.Phongs.Add(new Phong { SoPhong = "401", MaLoaiPhong = 5, Tang = 4, TrangThai = TrangThaiPhong.TrongSach });
            ctx.Phongs.Add(new Phong { SoPhong = "402", MaLoaiPhong = 5, Tang = 4, TrangThai = TrangThaiPhong.TrongSach });
        }

        private static void SeedKhachHang(HotelDbContext ctx)
        {
            // Lấy MaLKH của từng loại để gán đúng FK
            int ndId = ctx.LoaiKhachHangs.FirstOrDefault(l => l.MaCode == "NoiDia")?.MaLKH    ?? 1;
            int nnId = ctx.LoaiKhachHangs.FirstOrDefault(l => l.MaCode == "NuocNgoai")?.MaLKH ?? 2;

            ctx.KhachHangs.AddRange(
                new KhachHang { HoTen = "Nguyễn Văn An",   CMND = "001085012345", SDT = "0901111001", Email = "an.nguyen@gmail.com",   DiaChi = "12 Lý Thường Kiệt, Hà Nội",  QuocTich = "Việt Nam",   MaLoaiKH = ndId, GioiTinh = "Nam", NgaySinh = new DateTime(1990, 3, 15) },
                new KhachHang { HoTen = "Trần Thị Bình",   CMND = "079085067890", SDT = "0902222002", Email = "binh.tran@gmail.com",   DiaChi = "45 Nguyễn Huệ, TP.HCM",      QuocTich = "Việt Nam",   MaLoaiKH = ndId, GioiTinh = "Nu",  NgaySinh = new DateTime(1995, 7, 22) },
                new KhachHang { HoTen = "Lê Minh Châu",    CMND = "048085034567", SDT = "0903333003", Email = "chau.le@gmail.com",     DiaChi = "78 Trần Phú, Đà Nẵng",       QuocTich = "Việt Nam",   MaLoaiKH = ndId, GioiTinh = "Nam", NgaySinh = new DateTime(1988, 11, 5) },
                new KhachHang { HoTen = "Phạm Thu Hà",     CMND = "036085089012", SDT = "0904444004", Email = "ha.pham@gmail.com",     DiaChi = "23 Hoàng Diệu, Huế",         QuocTich = "Việt Nam",   MaLoaiKH = ndId, GioiTinh = "Nu",  NgaySinh = new DateTime(1993, 5, 18) },
                new KhachHang { HoTen = "Võ Quốc Hùng",   CMND = "092085056789", SDT = "0905555005", Email = "hung.vo@gmail.com",     DiaChi = "56 Pasteur, Cần Thơ",        QuocTich = "Việt Nam",   MaLoaiKH = ndId, GioiTinh = "Nam", NgaySinh = new DateTime(1985, 9, 30) },
                new KhachHang { HoTen = "John Smith",      CMND = "A12345678",    SDT = "+1-202-555-0101", Email = "john.smith@gmail.com", DiaChi = "New York, USA",           QuocTich = "Hoa Kỳ",     MaLoaiKH = nnId, GioiTinh = "Nam", NgaySinh = new DateTime(1982, 4, 12) },
                new KhachHang { HoTen = "Wang Fang",       CMND = "G87654321",    SDT = "+86-138-0000-1234", Email = "wang.fang@qq.com", DiaChi = "Beijing, China",           QuocTich = "Trung Quốc", MaLoaiKH = nnId, GioiTinh = "Nu",  NgaySinh = new DateTime(1991, 8, 20) },
                new KhachHang { HoTen = "Nguyễn Thị Lan", CMND = "001090023456", SDT = "0908888008", Email = "lan.nguyen@yahoo.com",  DiaChi = "99 Đinh Tiên Hoàng, Hà Nội", QuocTich = "Việt Nam",   MaLoaiKH = ndId, GioiTinh = "Nu",  NgaySinh = new DateTime(1997, 1, 8) },
                new KhachHang { HoTen = "Đặng Văn Đức",   CMND = "025090045678", SDT = "0909999009", Email = "duc.dang@gmail.com",    DiaChi = "34 Lê Lợi, Hải Phòng",      QuocTich = "Việt Nam",   MaLoaiKH = ndId, GioiTinh = "Nam", NgaySinh = new DateTime(1989, 6, 25) },
                new KhachHang { HoTen = "Tanaka Yuki",     CMND = "TK9876543",    SDT = "+81-90-1234-5678", Email = "tanaka.y@mail.jp", DiaChi = "Tokyo, Japan",             QuocTich = "Nhật Bản",   MaLoaiKH = nnId, GioiTinh = "Nu",  NgaySinh = new DateTime(1994, 3, 3) }
            );
        }
    }
}
