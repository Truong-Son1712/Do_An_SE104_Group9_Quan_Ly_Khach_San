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
        public bool NguoiDatPhongOPhong { get; set; } = true;
        public string? GhiChu { get; set; }

        public virtual KhachHang? KhachHang { get; set; }
        public virtual Phong? Phong { get; set; }
        public virtual HoaDon? HoaDon { get; set; }
        public virtual ICollection<DatPhongKhachHang> DatPhongKhachHangs { get; set; } = new List<DatPhongKhachHang>();
        public virtual ICollection<DichVuPhong>       DichVuPhongs       { get; set; } = new List<DichVuPhong>();

        // Hiển thị trong DataGrid: người đặt phòng (ĐP) + danh sách khách ở phòng
        public string DanhSachKhachText
        {
            get
            {
                string nguoiDat = KhachHang?.HoTen ?? "";
                if (!DatPhongKhachHangs.Any())
                    return string.IsNullOrEmpty(nguoiDat) ? "" : $"{nguoiDat} (ĐP)";

                // DatPhongKhachHangs chứa các khách thực sự ở phòng
                bool nguoiDatOPhong = DatPhongKhachHangs.Any(x => x.MaKH == MaKH);
                var parts = new List<string>();

                if (!string.IsNullOrEmpty(nguoiDat))
                    parts.Add(nguoiDatOPhong ? nguoiDat + " ★" : nguoiDat + " (ĐP)");

                foreach (var x in DatPhongKhachHangs.Where(x => x.MaKH != MaKH && (x.KhachHang?.HoTen?.Length ?? 0) > 0))
                    parts.Add(x.KhachHang!.HoTen);

                return string.Join(", ", parts);
            }
        }
    }
}
