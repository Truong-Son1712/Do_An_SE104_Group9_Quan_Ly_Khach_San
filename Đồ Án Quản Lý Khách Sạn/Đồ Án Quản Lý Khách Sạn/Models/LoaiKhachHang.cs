namespace Đồ_Án_Quản_Lý_Khách_Sạn.Models
{
    /// <summary>
    /// Loại khách hàng với hệ số giá riêng.
    /// MaCode là khóa chính dạng chuỗi (ví dụ: "NoiDia", "NuocNgoai", "VIP")
    /// khớp với KhachHang.LoaiKhach để backward-compatible.
    /// </summary>
    public class LoaiKhachHang
    {
        public string  MaCode   { get; set; } = "";   // PK: "NoiDia", "NuocNgoai", ...
        public string  TenLoai  { get; set; } = "";   // Tên hiển thị
        public decimal HeSoGia  { get; set; } = 1.0m; // Hệ số nhân vào giá phòng
    }
}
