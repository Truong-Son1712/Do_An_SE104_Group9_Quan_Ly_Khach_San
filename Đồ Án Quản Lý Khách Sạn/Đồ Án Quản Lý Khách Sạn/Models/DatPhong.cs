namespace Đồ_Án_Quản_Lý_Khách_Sạn.Models
{
    public enum TrangThaiDatPhong
    {
        DaDat,        // Đã đặt – chờ nhận phòng
        DaNhanPhong,  // Đã check-in
        DaTraPhong,   // Đã check-out
        HuyDat        // Đã hủy
    }

    public class DatPhong
    {
        public int MaDatPhong { get; set; }
        public int MaKH { get; set; }
        public int MaPhong { get; set; }
        public DateTime NgayDat { get; set; } = DateTime.Now;
        public DateTime NgayNhanPhong { get; set; } = DateTime.Today;
        public DateTime NgayTraPhong { get; set; } = DateTime.Today.AddDays(1);
        public TrangThaiDatPhong TrangThai { get; set; } = TrangThaiDatPhong.DaDat;
        public decimal TienCoc { get; set; } = 0;
        public int SoKhach { get; set; } = 1;
        public string? GhiChu { get; set; }

        public virtual KhachHang? KhachHang { get; set; }
        public virtual Phong? Phong { get; set; }
        public virtual HoaDon? HoaDon { get; set; }
        public virtual ICollection<DatPhongKhachHang> DatPhongKhachHangs { get; set; } = new List<DatPhongKhachHang>();

        // Hiển thị danh sách khách trong DataGrid
        public string DanhSachKhachText => DatPhongKhachHangs.Any()
            ? string.Join(", ", DatPhongKhachHangs.Select(x => x.KhachHang?.HoTen ?? "").Where(s => s.Length > 0))
            : KhachHang?.HoTen ?? "";
    }
}
