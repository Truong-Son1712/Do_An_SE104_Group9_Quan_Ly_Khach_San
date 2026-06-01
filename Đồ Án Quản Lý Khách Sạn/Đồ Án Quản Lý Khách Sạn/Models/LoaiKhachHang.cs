namespace Đồ_Án_Quản_Lý_Khách_Sạn.Models
{
    /// <summary>
    /// Loại khách hàng với hệ số giá riêng.
    /// MaLKH: khoá chính int (IDENTITY).
    /// MaCode: mã định danh chuỗi duy nhất, dùng trong code ứng dụng (backward-compatible).
    /// </summary>
    public class LoaiKhachHang
    {
        public int     MaLKH   { get; set; }       // PK int IDENTITY
        public string  MaCode  { get; set; } = "";  // UNIQUE – "NoiDia", "NuocNgoai", …
        public string  TenLoai { get; set; } = "";  // Tên hiển thị
        public decimal HeSoGia { get; set; } = 1.0m;
    }
}
