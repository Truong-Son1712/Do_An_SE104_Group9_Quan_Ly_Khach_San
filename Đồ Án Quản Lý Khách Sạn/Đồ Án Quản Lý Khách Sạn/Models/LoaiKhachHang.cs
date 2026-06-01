namespace Đồ_Án_Quản_Lý_Khách_Sạn.Models
{
    public class LoaiKhachHang
    {
        public int     MaLKH   { get; set; }       // PK int IDENTITY
        public string  MaCode  { get; set; } = "";  // mã ngắn dùng nội bộ
        public string  TenLoai { get; set; } = "";  // tên hiển thị
        public decimal HeSoGia { get; set; } = 1.0m;

        public virtual ICollection<KhachHang> KhachHangs { get; set; } = new List<KhachHang>();
    }
}
