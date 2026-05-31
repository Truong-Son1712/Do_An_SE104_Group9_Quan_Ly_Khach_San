namespace Đồ_Án_Quản_Lý_Khách_Sạn.Models
{
    public class LoaiPhong
    {
        public int MaLoaiPhong { get; set; }
        public string TenLoaiPhong { get; set; } = string.Empty;
        public decimal GiaPhong { get; set; }
        public int SucChua { get; set; } = 2;
        public string? MoTa { get; set; }

        public virtual ICollection<Phong> Phongs { get; set; } = new List<Phong>();
    }
}
