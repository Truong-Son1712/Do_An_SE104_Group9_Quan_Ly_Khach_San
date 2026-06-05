namespace Đồ_Án_Quản_Lý_Khách_Sạn.Models
{
    public class LoaiNhanVienQuyen
    {
        public int    MaLoaiNV { get; set; }
        public string MaQuyen  { get; set; } = "";

        public virtual LoaiNhanVien? LoaiNhanVien { get; set; }
    }
}
