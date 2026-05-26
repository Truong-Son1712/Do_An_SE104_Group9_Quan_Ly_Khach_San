namespace Đồ_Án_Quản_Lý_Khách_Sạn.Models
{
    public class NhanVien
    {
        public int MaNV { get; set; }
        public string HoTen { get; set; } = string.Empty;
        public string TaiKhoan { get; set; } = string.Empty;
        public string MatKhau { get; set; } = string.Empty;
        public string VaiTro { get; set; } = "LeTan"; // Admin | QuanLy | LeTan
        public string? Email { get; set; }
        public string? SDT { get; set; }
        public string? DiaChi { get; set; }
        public DateTime NgayTao { get; set; } = DateTime.Now;
        public bool IsActive { get; set; } = true;

        public virtual ICollection<HoaDon> HoaDons { get; set; } = new List<HoaDon>();
    }
}
