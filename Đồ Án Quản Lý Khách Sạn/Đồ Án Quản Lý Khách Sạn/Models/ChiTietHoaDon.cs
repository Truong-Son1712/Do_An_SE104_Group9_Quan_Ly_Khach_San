namespace Đồ_Án_Quản_Lý_Khách_Sạn.Models
{
    public class ChiTietHoaDon
    {
        public int MaCTHD { get; set; }
        public int MaHD { get; set; }
        public string MoTa { get; set; } = string.Empty;
        public int SoLuong { get; set; } = 1;
        public decimal DonGia { get; set; }

        public decimal ThanhTien => SoLuong * DonGia;

        public virtual HoaDon? HoaDon { get; set; }
    }
}
