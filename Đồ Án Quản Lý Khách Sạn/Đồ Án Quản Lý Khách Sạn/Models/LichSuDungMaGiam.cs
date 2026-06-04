namespace Đồ_Án_Quản_Lý_Khách_Sạn.Models
{
    public class LichSuDungMaGiam
    {
        public int MaSuDung { get; set; }
        public int MaGG { get; set; }
        public int MaHD { get; set; }
        public DateTime NgaySuDung { get; set; } = DateTime.Now;

        public virtual MaGiamGia? MaGiamGia { get; set; }
        public virtual HoaDon? HoaDon { get; set; }
    }
}
