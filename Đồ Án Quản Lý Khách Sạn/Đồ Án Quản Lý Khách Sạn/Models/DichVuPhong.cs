namespace Đồ_Án_Quản_Lý_Khách_Sạn.Models
{
    public class DichVuPhong
    {
        public int      MaDVP      { get; set; }
        public int      MaDatPhong { get; set; }
        public int      MaLoaiDV   { get; set; }
        public int      SoLuong    { get; set; } = 1;
        public decimal  DonGia     { get; set; } // snapshot giá tại thời điểm thêm
        public DateTime NgayThem   { get; set; } = DateTime.Now;
        public string?  GhiChu     { get; set; }

        public virtual DatPhong?   DatPhong   { get; set; }
        public virtual LoaiDichVu? LoaiDichVu { get; set; }

        public decimal ThanhTien => SoLuong * DonGia;
    }
}
