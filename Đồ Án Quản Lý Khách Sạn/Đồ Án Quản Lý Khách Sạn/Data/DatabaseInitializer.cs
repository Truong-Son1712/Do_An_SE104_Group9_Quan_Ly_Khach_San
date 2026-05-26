using Microsoft.EntityFrameworkCore;
using Đồ_Án_Quản_Lý_Khách_Sạn.Models;

namespace Đồ_Án_Quản_Lý_Khách_Sạn.Data
{
    public static class DatabaseInitializer
    {
        public static void Initialize(HotelDbContext ctx)
        {
            ctx.Database.EnsureCreated();

            // Migration thủ công: tạo bảng / cột mới nếu chưa tồn tại
            ctx.Database.ExecuteSqlRaw(
                "CREATE TABLE IF NOT EXISTS CauHinhs (Khoa TEXT NOT NULL PRIMARY KEY, GiaTri TEXT NOT NULL DEFAULT '');");
            ctx.Database.ExecuteSqlRaw(
                "CREATE TABLE IF NOT EXISTS DatPhongKhachHangs (MaDatPhong INTEGER NOT NULL, MaKH INTEGER NOT NULL, PRIMARY KEY(MaDatPhong, MaKH));");
            try { ctx.Database.ExecuteSqlRaw("ALTER TABLE HoaDons ADD COLUMN TienCoc TEXT NOT NULL DEFAULT '0';"); }
            catch { /* cột đã tồn tại */ }
            try { ctx.Database.ExecuteSqlRaw("ALTER TABLE DatPhongs ADD COLUMN SoKhach INTEGER NOT NULL DEFAULT 1;"); }
            catch { /* cột đã tồn tại */ }
            try { ctx.Database.ExecuteSqlRaw("ALTER TABLE HoaDons ADD COLUMN NgayThanhToan TEXT NULL;"); }
            catch { /* cột đã tồn tại */ }

            if (!ctx.NhanViens.Any())   SeedNhanVien(ctx);
            if (!ctx.LoaiPhongs.Any())  SeedLoaiPhong(ctx);
            if (!ctx.Phongs.Any())      SeedPhong(ctx);
            if (!ctx.KhachHangs.Any()) SeedKhachHang(ctx);

            ctx.SaveChanges();
        }

        private static string Hash(string pw) => BCrypt.Net.BCrypt.HashPassword(pw);

        private static void SeedNhanVien(HotelDbContext ctx)
        {
            ctx.NhanViens.AddRange(
                new NhanVien
                {
                    HoTen = "Nguyễn Văn Admin", TaiKhoan = "admin",
                    MatKhau = Hash("admin123"),  VaiTro = "Admin",
                    Email = "admin@hotel.com",   SDT = "0901234560", IsActive = true
                },
                new NhanVien
                {
                    HoTen = "Trần Thị Quản Lý", TaiKhoan = "quanly",
                    MatKhau = Hash("quanly123"), VaiTro = "QuanLy",
                    Email = "quanly@hotel.com",  SDT = "0901234561", IsActive = true
                },
                new NhanVien
                {
                    HoTen = "Lê Văn Lễ Tân", TaiKhoan = "letan",
                    MatKhau = Hash("letan123"),  VaiTro = "LeTan",
                    Email = "letan@hotel.com",   SDT = "0901234562", IsActive = true
                }
            );
        }

        private static void SeedLoaiPhong(HotelDbContext ctx)
        {
            ctx.LoaiPhongs.AddRange(
                new LoaiPhong { TenLoaiPhong = "Phòng Đơn Standard",  GiaPhong = 500_000,  SucChua = 1, MoTa = "Phòng đơn tiêu chuẩn, 1 giường đơn" },
                new LoaiPhong { TenLoaiPhong = "Phòng Đôi Standard",  GiaPhong = 800_000,  SucChua = 2, MoTa = "Phòng đôi tiêu chuẩn, 1 giường đôi" },
                new LoaiPhong { TenLoaiPhong = "Phòng Gia Đình",      GiaPhong = 1_200_000, SucChua = 4, MoTa = "Phòng gia đình, 2 giường đôi" },
                new LoaiPhong { TenLoaiPhong = "Phòng Deluxe",        GiaPhong = 1_500_000, SucChua = 2, MoTa = "Phòng Deluxe, view biển" },
                new LoaiPhong { TenLoaiPhong = "Suite VIP",           GiaPhong = 3_000_000, SucChua = 2, MoTa = "Suite cao cấp, phòng khách riêng" }
            );
        }

        private static void SeedPhong(HotelDbContext ctx)
        {
            // Tầng 1 – Đơn Standard
            for (int i = 1; i <= 6; i++)
                ctx.Phongs.Add(new Phong { SoPhong = $"1{i:D2}", MaLoaiPhong = 1, Tang = 1, TrangThai = TrangThaiPhong.TrongSach });

            // Tầng 2 – Đôi Standard
            for (int i = 1; i <= 6; i++)
                ctx.Phongs.Add(new Phong { SoPhong = $"2{i:D2}", MaLoaiPhong = 2, Tang = 2, TrangThai = TrangThaiPhong.TrongSach });

            // Tầng 3 – Gia Đình + Deluxe
            ctx.Phongs.Add(new Phong { SoPhong = "301", MaLoaiPhong = 3, Tang = 3, TrangThai = TrangThaiPhong.TrongSach });
            ctx.Phongs.Add(new Phong { SoPhong = "302", MaLoaiPhong = 3, Tang = 3, TrangThai = TrangThaiPhong.TrongSach });
            ctx.Phongs.Add(new Phong { SoPhong = "303", MaLoaiPhong = 4, Tang = 3, TrangThai = TrangThaiPhong.TrongSach });
            ctx.Phongs.Add(new Phong { SoPhong = "304", MaLoaiPhong = 4, Tang = 3, TrangThai = TrangThaiPhong.TrongSach });

            // Tầng 4 – Suite VIP
            ctx.Phongs.Add(new Phong { SoPhong = "401", MaLoaiPhong = 5, Tang = 4, TrangThai = TrangThaiPhong.TrongSach });
            ctx.Phongs.Add(new Phong { SoPhong = "402", MaLoaiPhong = 5, Tang = 4, TrangThai = TrangThaiPhong.TrongSach });
        }

        private static void SeedKhachHang(HotelDbContext ctx)
        {
            ctx.KhachHangs.AddRange(
                new KhachHang { HoTen = "Nguyễn Văn An",   CMND = "001085012345", SDT = "0901111001", Email = "an.nguyen@gmail.com",     DiaChi = "12 Lý Thường Kiệt, Hà Nội",    QuocTich = "Việt Nam",       LoaiKhach = "NoiDia",    GioiTinh = "Nam", NgaySinh = new DateTime(1990, 3, 15) },
                new KhachHang { HoTen = "Trần Thị Bình",   CMND = "079085067890", SDT = "0902222002", Email = "binh.tran@gmail.com",      DiaChi = "45 Nguyễn Huệ, TP.HCM",        QuocTich = "Việt Nam",       LoaiKhach = "NoiDia",    GioiTinh = "Nu",  NgaySinh = new DateTime(1995, 7, 22) },
                new KhachHang { HoTen = "Lê Minh Châu",    CMND = "048085034567", SDT = "0903333003", Email = "chau.le@gmail.com",        DiaChi = "78 Trần Phú, Đà Nẵng",          QuocTich = "Việt Nam",       LoaiKhach = "NoiDia",    GioiTinh = "Nam", NgaySinh = new DateTime(1988, 11, 5) },
                new KhachHang { HoTen = "Phạm Thu Hà",     CMND = "036085089012", SDT = "0904444004", Email = "ha.pham@gmail.com",        DiaChi = "23 Hoàng Diệu, Huế",            QuocTich = "Việt Nam",       LoaiKhach = "NoiDia",    GioiTinh = "Nu",  NgaySinh = new DateTime(1993, 5, 18) },
                new KhachHang { HoTen = "Võ Quốc Hùng",   CMND = "092085056789", SDT = "0905555005", Email = "hung.vo@gmail.com",        DiaChi = "56 Pasteur, Cần Thơ",           QuocTich = "Việt Nam",       LoaiKhach = "NoiDia",    GioiTinh = "Nam", NgaySinh = new DateTime(1985, 9, 30) },
                new KhachHang { HoTen = "John Smith",      CMND = "A12345678",    SDT = "+1-202-555-0101", Email = "john.smith@gmail.com", DiaChi = "New York, USA",                QuocTich = "Hoa Kỳ",         LoaiKhach = "NuocNgoai", GioiTinh = "Nam", NgaySinh = new DateTime(1982, 4, 12) },
                new KhachHang { HoTen = "Wang Fang",       CMND = "G87654321",    SDT = "+86-138-0000-1234", Email = "wang.fang@qq.com",  DiaChi = "Beijing, China",               QuocTich = "Trung Quốc",     LoaiKhach = "NuocNgoai", GioiTinh = "Nu",  NgaySinh = new DateTime(1991, 8, 20) },
                new KhachHang { HoTen = "Nguyễn Thị Lan", CMND = "001090023456", SDT = "0908888008", Email = "lan.nguyen@yahoo.com",    DiaChi = "99 Đinh Tiên Hoàng, Hà Nội",    QuocTich = "Việt Nam",       LoaiKhach = "NoiDia",    GioiTinh = "Nu",  NgaySinh = new DateTime(1997, 1, 8) },
                new KhachHang { HoTen = "Đặng Văn Đức",   CMND = "025090045678", SDT = "0909999009", Email = "duc.dang@gmail.com",       DiaChi = "34 Lê Lợi, Hải Phòng",         QuocTich = "Việt Nam",       LoaiKhach = "NoiDia",    GioiTinh = "Nam", NgaySinh = new DateTime(1989, 6, 25) },
                new KhachHang { HoTen = "Tanaka Yuki",    CMND = "TK9876543",    SDT = "+81-90-1234-5678", Email = "tanaka.y@mail.jp",   DiaChi = "Tokyo, Japan",                 QuocTich = "Nhật Bản",       LoaiKhach = "NuocNgoai", GioiTinh = "Nu",  NgaySinh = new DateTime(1994, 3, 3) }
            );
        }

    }
}
