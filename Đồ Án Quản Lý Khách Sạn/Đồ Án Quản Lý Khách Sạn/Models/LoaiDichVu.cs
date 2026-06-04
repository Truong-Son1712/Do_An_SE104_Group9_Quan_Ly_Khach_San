using Đồ_Án_Quản_Lý_Khách_Sạn.Helpers;

namespace Đồ_Án_Quản_Lý_Khách_Sạn.Models
{
    public class LoaiDichVu
    {
        public int     MaLoaiDV  { get; set; }
        public string  TenLoaiDV { get; set; } = "";
        public decimal DonGia    { get; set; }   // đơn giá gốc lưu DB
        public string  DonViTinh { get; set; } = "";
        public string? MoTa      { get; set; }
        public bool    IsActive  { get; set; } = true;

        // Đơn giá hiện tại sau khi áp dụng tỉ lệ bổ sung (không lưu DB)
        public decimal DonGiaHienTai => AppConfig.GetGiaDichVuHienTai(DonGia);

        public virtual ICollection<DichVuPhong> DichVuPhongs { get; set; } = new List<DichVuPhong>();
    }
}
