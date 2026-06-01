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
        public DbSet<LoaiPhong>         LoaiPhongs         { get; set; }
        public DbSet<Phong>             Phongs             { get; set; }
        public DbSet<KhachHang>         KhachHangs         { get; set; }
        public DbSet<DatPhong>          DatPhongs          { get; set; }
        public DbSet<DatPhongKhachHang> DatPhongKhachHangs { get; set; }
        public DbSet<HoaDon>            HoaDons            { get; set; }
        public DbSet<CauHinh>           CauHinhs           { get; set; }
        public DbSet<LoaiKhachHang>     LoaiKhachHangs     { get; set; }

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

            m.Entity<LoaiPhong>(e =>
            {
                e.HasKey(x => x.MaLoaiPhong);
                e.Property(x => x.GiaPhong).HasColumnType("decimal(18,2)");
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
                // KhachHang.LoaiKhach (string) tham chiếu LoaiKhachHang.MaCode (UNIQUE)
                e.HasOne<LoaiKhachHang>()
                 .WithMany()
                 .HasForeignKey(x => x.LoaiKhach)
                 .HasPrincipalKey(l => l.MaCode)
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
                e.HasOne(x => x.DatPhong).WithOne(x => x.HoaDon)
                 .HasForeignKey<HoaDon>(x => x.MaDatPhong).OnDelete(DeleteBehavior.Cascade);
                e.HasOne(x => x.NhanVien).WithMany(x => x.HoaDons)
                 .HasForeignKey(x => x.MaNV).OnDelete(DeleteBehavior.Restrict);
            });

            m.Entity<LoaiKhachHang>(e =>
            {
                e.HasKey(x => x.MaLKH);
                e.Property(x => x.MaCode).HasMaxLength(100).IsRequired();
                e.HasIndex(x => x.MaCode).IsUnique();
                e.Property(x => x.HeSoGia).HasColumnType("decimal(10,4)");
            });
        }
    }
}
