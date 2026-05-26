using System.IO;
using Microsoft.EntityFrameworkCore;
using Đồ_Án_Quản_Lý_Khách_Sạn.Models;

namespace Đồ_Án_Quản_Lý_Khách_Sạn.Data
{
    public class HotelDbContext : DbContext
    {
        private static readonly string DbPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "QuanLyKhachSan", "hotel.db");

        public DbSet<NhanVien> NhanViens { get; set; }
        public DbSet<LoaiPhong> LoaiPhongs { get; set; }
        public DbSet<Phong> Phongs { get; set; }
        public DbSet<KhachHang> KhachHangs { get; set; }
        public DbSet<DatPhong> DatPhongs { get; set; }
        public DbSet<DatPhongKhachHang> DatPhongKhachHangs { get; set; }
        public DbSet<HoaDon> HoaDons { get; set; }
        public DbSet<CauHinh> CauHinhs { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder options)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(DbPath)!);
            options.UseSqlite($"Data Source={DbPath}");
        }

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

            m.Entity<KhachHang>(e => e.HasKey(x => x.MaKH));

            m.Entity<DatPhong>(e =>
            {
                e.HasKey(x => x.MaDatPhong);
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

            m.Entity<CauHinh>(e => e.HasKey(x => x.Khoa));

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
        }
    }
}
