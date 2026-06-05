namespace Đồ_Án_Quản_Lý_Khách_Sạn.Models
{
    public class NhanVienQuyen
    {
        public int    MaNV    { get; set; }
        public string MaQuyen { get; set; } = "";

        public virtual NhanVien? NhanVien { get; set; }
    }
}
