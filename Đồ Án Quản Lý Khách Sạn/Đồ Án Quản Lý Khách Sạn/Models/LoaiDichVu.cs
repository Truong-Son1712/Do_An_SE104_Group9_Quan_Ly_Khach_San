namespace Đồ_Án_Quản_Lý_Khách_Sạn.Models
{
    public class LoaiDichVu
    {
        public int     MaLoaiDV  { get; set; }
        public string  TenLoaiDV { get; set; } = "";
        public decimal DonGia    { get; set; }
        public string  DonViTinh { get; set; } = ""; // chai, bữa, chuyến, giờ...
        public string? MoTa      { get; set; }
        public bool    IsActive  { get; set; } = true;

        public virtual ICollection<DichVuPhong> DichVuPhongs { get; set; } = new List<DichVuPhong>();
    }
}
