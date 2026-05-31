namespace Đồ_Án_Quản_Lý_Khách_Sạn.Models
{
    public enum TrangThaiPhong
    {
        TrongSach,    // Trống sạch – sẵn sàng đón khách
        DangSuDung,   // Đang có khách
        CanDonDep,    // Đã trả phòng, chưa dọn
        BaoDuong,     // Đang bảo dưỡng / sửa chữa
        DaDat         // Đã được đặt trước
    }

    public class Phong
    {
        public int MaPhong { get; set; }
        public string SoPhong { get; set; } = string.Empty;
        public int MaLoaiPhong { get; set; }
        public int Tang { get; set; } = 1;
        public TrangThaiPhong TrangThai { get; set; } = TrangThaiPhong.TrongSach;
        public string? MoTa { get; set; }

        public virtual LoaiPhong? LoaiPhong { get; set; }
        public virtual ICollection<DatPhong> DatPhongs { get; set; } = new List<DatPhong>();
    }
}
