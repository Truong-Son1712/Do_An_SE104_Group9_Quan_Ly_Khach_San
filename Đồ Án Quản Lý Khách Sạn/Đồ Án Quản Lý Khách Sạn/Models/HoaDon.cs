namespace Đồ_Án_Quản_Lý_Khách_Sạn.Models
{
    public class HoaDon
    {
        public int MaHD { get; set; }
        public int MaDatPhong { get; set; }
        public int MaNV { get; set; }
        public DateTime NgayLap { get; set; } = DateTime.Now;
        public decimal TienPhong { get; set; }
        public decimal TienCoc { get; set; }
        public decimal TongTien { get; set; }
        public string PhuongThucTT { get; set; } = "TienMat"; // TienMat | ChuyenKhoan | The
        public string TrangThai { get; set; } = "ChuaThanhToan"; // ChuaThanhToan | DaThanhToan
        public DateTime? NgayThanhToan { get; set; }
        public string? GhiChu { get; set; }

        public virtual DatPhong? DatPhong { get; set; }
        public virtual NhanVien? NhanVien { get; set; }

        public int SoNgay => DatPhong == null ? 1
            : Math.Max(1, (DatPhong.NgayTraPhong.Date - DatPhong.NgayNhanPhong.Date).Days);

        public decimal ConLai => Math.Max(0, TienPhong - TienCoc);
    }
}
