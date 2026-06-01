namespace Đồ_Án_Quản_Lý_Khách_Sạn.Models
{
    public class KhachHang
    {
        public int      MaKH      { get; set; }
        public string   HoTen     { get; set; } = string.Empty;
        public string   CMND      { get; set; } = string.Empty;
        public string?  SDT       { get; set; }
        public string?  Email     { get; set; }
        public string?  DiaChi    { get; set; }
        public string   QuocTich  { get; set; } = "Việt Nam";
        public int      MaLoaiKH  { get; set; } = 1;   // FK → LoaiKhachHangs.MaLKH
        public DateTime? NgaySinh { get; set; }
        public string   GioiTinh  { get; set; } = "Nam";
        public DateTime NgayTao   { get; set; } = DateTime.Now;

        public virtual LoaiKhachHang?       LoaiKhachHang { get; set; }
        public virtual ICollection<DatPhong> DatPhongs    { get; set; } = new List<DatPhong>();
    }
}
