namespace Đồ_Án_Quản_Lý_Khách_Sạn.Models
{
    public class DatPhongKhachHang
    {
        public int MaDatPhong { get; set; }
        public int MaKH { get; set; }

        public virtual DatPhong?   DatPhong   { get; set; }
        public virtual KhachHang?  KhachHang  { get; set; }
    }
}
