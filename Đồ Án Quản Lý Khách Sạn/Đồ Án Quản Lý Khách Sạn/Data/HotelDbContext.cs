using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Đồ_Án_Quản_Lý_Khách_Sạn.Models;

namespace Đồ_Án_Quản_Lý_Khách_Sạn.Data
{
    public class HotelDbContext : DbContext
    {
        // ── Chuỗi kết nối – đọc từ appsettings.json (nằm cùng thư mục .exe) ──
        // Người dùng chỉnh sửa file appsettings.json để thay đổi kết nối,
        // không cần recompile lại code.
        private static string? _connectionString;
        public static string ConnectionString
        {
            get
            {
                if (_connectionString != null) return _connectionString;
                try
                {
                    var config = new ConfigurationBuilder()
                        .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
                        .AddJsonFile("appsettings.json", optional: false, reloadOnChange: false)
                        .Build();
                    _connectionString = config.GetConnectionString("DefaultConnection");
                }
                catch { /* fallback nếu không đọc được file */ }

                // Fallback mặc định nếu appsettings.json không tồn tại
                return _connectionString
                    ?? "Server=.;Database=QuanLyKhachSan;Trusted_Connection=True;TrustServerCertificate=True;";
            }
            set => _connectionString = value;
        }

        public DbSet<NhanVien>          NhanViens          { get; set; }
        public DbSet<NhanVienQuyen>     NhanVienQuyens     { get; set; }
        public DbSet<LoaiNhanVien>      LoaiNhanViens      { get; set; }
        public DbSet<LoaiNhanVienQuyen> LoaiNhanVienQuyens { get; set; }
        public DbSet<LoaiPhong>         LoaiPhongs         { get; set; }
        public DbSet<Phong>             Phongs             { get; set; }
        public DbSet<KhachHang>         KhachHangs         { get; set; }
        public DbSet<DatPhong>          DatPhongs          { get; set; }
        public DbSet<DatPhongKhachHang> DatPhongKhachHangs { get; set; }
        public DbSet<HoaDon>            HoaDons            { get; set; }
        public DbSet<CauHinh>           CauHinhs           { get; set; }
        public DbSet<LoaiKhachHang>     LoaiKhachHangs     { get; set; }
        public DbSet<LoaiDichVu>        LoaiDichVus        { get; set; }
        public DbSet<DichVuPhong>       DichVuPhongs       { get; set; }
        public DbSet<MaGiamGia>         MaGiamGias         { get; set; }
        public DbSet<ChiTietMaGiamGia>  ChiTietMaGiamGias  { get; set; }
        public DbSet<LichSuDungMaGiam>  LichSuDungMaGiams  { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder options)
            => options.UseSqlServer(ConnectionString);

        protected override void OnModelCreating(ModelBuilder m)
        {
            m.Entity<NhanVien>(e =>
            {
                e.HasKey(x => x.MaNV);
                e.Property(x => x.HoTen).IsRequired().HasMaxLength(100);
                e.Property(x => x.TaiKhoan).IsRequired().HasMaxLength(50);
                e.HasIndex(x => x.TaiKhoan).IsUnique();
            });

            m.Entity<NhanVienQuyen>(e =>
            {
                e.HasKey(x => new { x.MaNV, x.MaQuyen });
                e.Property(x => x.MaQuyen).HasMaxLength(100);
                e.HasOne(x => x.NhanVien).WithMany(n => n.Quyens)
                 .HasForeignKey(x => x.MaNV).OnDelete(DeleteBehavior.Cascade);
            });

            m.Entity<LoaiNhanVien>(e =>
            {
                e.HasKey(x => x.MaLoaiNV);
                e.Property(x => x.TenLoai).IsRequired().HasMaxLength(100);
                e.Property(x => x.VaiTroCode).IsRequired().HasMaxLength(50);
                e.HasIndex(x => x.VaiTroCode).IsUnique();
                e.Ignore(x => x.SoNhanVien);
            });

            m.Entity<LoaiNhanVienQuyen>(e =>
            {
                e.HasKey(x => new { x.MaLoaiNV, x.MaQuyen });
                e.Property(x => x.MaQuyen).HasMaxLength(100);
                e.HasOne(x => x.LoaiNhanVien).WithMany(l => l.Quyens)
                 .HasForeignKey(x => x.MaLoaiNV).OnDelete(DeleteBehavior.Cascade);
            });

            m.Entity<LoaiPhong>(e =>
            {
                e.HasKey(x => x.MaLoaiPhong);
                e.Property(x => x.GiaPhong).HasColumnType("decimal(18,2)");
                e.Ignore(x => x.GiaPhongHienTai);
            });

            m.Entity<Phong>(e =>
            {
                e.HasKey(x => x.MaPhong);
                e.HasOne(x => x.LoaiPhong)
                 .WithMany(x => x.Phongs)
                 .HasForeignKey(x => x.MaLoaiPhong)
                 .OnDelete(DeleteBehavior.Restrict);
            });

            m.Entity<KhachHang>(e =>
            {
                e.HasKey(x => x.MaKH);
                // KhachHang.MaLoaiKH (int) FK → LoaiKhachHangs.MaLKH (PK int)
                e.HasOne(x => x.LoaiKhachHang)
                 .WithMany(l => l.KhachHangs)
                 .HasForeignKey(x => x.MaLoaiKH)
                 .OnDelete(DeleteBehavior.Restrict);
            });

            m.Entity<DatPhong>(e =>
            {
                e.HasKey(x => x.MaDatPhong);
                e.Property(x => x.TienCoc).HasColumnType("decimal(18,2)");
                e.HasOne(x => x.KhachHang).WithMany(x => x.DatPhongs)
                 .HasForeignKey(x => x.MaKH).OnDelete(DeleteBehavior.Restrict);
                e.HasOne(x => x.Phong).WithMany(x => x.DatPhongs)
                 .HasForeignKey(x => x.MaPhong).OnDelete(DeleteBehavior.Restrict);
            });

            m.Entity<DatPhongKhachHang>(e =>
            {
                e.HasKey(x => new { x.MaDatPhong, x.MaKH });
                e.HasOne(x => x.DatPhong).WithMany(d => d.DatPhongKhachHangs)
                 .HasForeignKey(x => x.MaDatPhong).OnDelete(DeleteBehavior.Cascade);
                e.HasOne(x => x.KhachHang).WithMany()
                 .HasForeignKey(x => x.MaKH).OnDelete(DeleteBehavior.Restrict);
            });

            m.Entity<CauHinh>(e =>
            {
                e.HasKey(x => x.ConfigKey);
                e.Property(x => x.ConfigKey).HasMaxLength(100);
                e.Property(x => x.ConfigValue).HasMaxLength(500).HasDefaultValue("");
            });

            m.Entity<HoaDon>(e =>
            {
                e.HasKey(x => x.MaHD);
                e.Property(x => x.TienPhong).HasColumnType("decimal(18,2)");
                e.Property(x => x.TienCoc).HasColumnType("decimal(18,2)");
                e.Property(x => x.TongTien).HasColumnType("decimal(18,2)");
                e.Property(x => x.TienGiam).HasColumnType("decimal(18,2)");
                e.Property(x => x.TienVAT).HasColumnType("decimal(18,2)");
                e.Property(x => x.VATPercent).HasColumnType("decimal(5,2)");
                e.Property(x => x.GiaPhongGoc).HasColumnType("decimal(18,2)");
                e.Property(x => x.HeSoLoaiKhach).HasColumnType("decimal(10,4)");
                e.Property(x => x.TiLePhuThuSucChua).HasColumnType("decimal(5,4)");
                e.HasOne(x => x.DatPhong).WithOne(x => x.HoaDon)
                 .HasForeignKey<HoaDon>(x => x.MaDatPhong).OnDelete(DeleteBehavior.Cascade);
                e.HasOne(x => x.NhanVien).WithMany(x => x.HoaDons)
                 .HasForeignKey(x => x.MaNV).OnDelete(DeleteBehavior.Restrict);
                e.HasOne(x => x.MaGiamGia).WithMany()
                 .HasForeignKey(x => x.MaGG).OnDelete(DeleteBehavior.Restrict);
            });

            m.Entity<MaGiamGia>(e =>
            {
                e.HasKey(x => x.MaGG);
                e.Property(x => x.TenMa).IsRequired().HasMaxLength(100);
                // Không dùng UNIQUE DB-level nữa; unique được kiểm tra ở application layer
                // (cho phép trùng tên với mã đã tắt/hết hạn)
                e.HasOne(x => x.NhanVien).WithMany()
                 .HasForeignKey(x => x.MaNVTao).OnDelete(DeleteBehavior.Restrict);
            });

            m.Entity<ChiTietMaGiamGia>(e =>
            {
                e.HasKey(x => x.MaChiTiet);
                e.Property(x => x.TiLeGiam).HasColumnType("decimal(5,2)");
                e.Ignore(x => x.TenLoai);
                e.HasOne(x => x.MaGiamGia).WithMany(g => g.ChiTiets)
                 .HasForeignKey(x => x.MaGG).OnDelete(DeleteBehavior.Cascade);
            });

            m.Entity<LichSuDungMaGiam>(e =>
            {
                e.HasKey(x => x.MaSuDung);
                e.HasOne(x => x.MaGiamGia).WithMany(g => g.LichSuDungs)
                 .HasForeignKey(x => x.MaGG).OnDelete(DeleteBehavior.Restrict);
                e.HasOne(x => x.HoaDon).WithMany(h => h.LichSuDungMaGiams)
                 .HasForeignKey(x => x.MaHD).OnDelete(DeleteBehavior.Cascade);
            });

            m.Entity<LoaiKhachHang>(e =>
            {
                e.HasKey(x => x.MaLKH);
                e.Property(x => x.MaCode).HasMaxLength(100).IsRequired();
                e.HasIndex(x => x.MaCode).IsUnique();
                e.Property(x => x.HeSoGia).HasColumnType("decimal(10,4)");
            });

            m.Entity<LoaiDichVu>(e =>
            {
                e.HasKey(x => x.MaLoaiDV);
                e.Property(x => x.DonGia).HasColumnType("decimal(18,2)");
                e.Ignore(x => x.DonGiaHienTai);
            });

            m.Entity<DichVuPhong>(e =>
            {
                e.HasKey(x => x.MaDVP);
                e.Property(x => x.DonGia).HasColumnType("decimal(18,2)");
                e.Ignore(x => x.ThanhTien);
                e.HasOne(x => x.DatPhong).WithMany(d => d.DichVuPhongs)
                 .HasForeignKey(x => x.MaDatPhong).OnDelete(DeleteBehavior.Cascade);
                e.HasOne(x => x.LoaiDichVu).WithMany(l => l.DichVuPhongs)
                 .HasForeignKey(x => x.MaLoaiDV).OnDelete(DeleteBehavior.Restrict);
            });
        }
    }
}
