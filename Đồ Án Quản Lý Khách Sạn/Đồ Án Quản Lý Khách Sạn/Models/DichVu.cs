namespace Đồ_Án_Quản_Lý_Khách_Sạn.Models
{
    public class DichVu
    {
        public int MaDV { get; set; }
        public string TenDV { get; set; } = string.Empty;
        public decimal DonGia { get; set; }
        public string? MoTa { get; set; }
        public bool IsActive { get; set; } = true;
    }
}
