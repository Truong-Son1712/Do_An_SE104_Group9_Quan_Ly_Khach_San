using Đồ_Án_Quản_Lý_Khách_Sạn.Helpers;

namespace Đồ_Án_Quản_Lý_Khách_Sạn.Models
{
    public class LoaiPhong
    {
        public int MaLoaiPhong { get; set; }
        public string TenLoaiPhong { get; set; } = string.Empty;
        public decimal GiaPhong { get; set; }   // giá gốc lưu DB
        public int SucChua { get; set; } = 2;
        public string? MoTa { get; set; }

        // Giá hiện tại sau khi áp dụng tỉ lệ bổ sung (không lưu DB)
        public decimal GiaPhongHienTai => AppConfig.GetGiaPhongHienTai(GiaPhong);

        public virtual ICollection<Phong> Phongs { get; set; } = new List<Phong>();
    }
}
