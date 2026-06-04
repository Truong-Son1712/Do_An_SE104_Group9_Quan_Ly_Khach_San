namespace Đồ_Án_Quản_Lý_Khách_Sạn.Models
{
    public class ChiTietMaGiamGia
    {
        public int MaChiTiet { get; set; }
        public int MaGG { get; set; }
        public string LoaiApDung { get; set; } = "LoaiPhong"; // LoaiPhong | LoaiDichVu
        public int MaLoai { get; set; }
        public decimal TiLeGiam { get; set; } // 0..100 (%)

        public virtual MaGiamGia? MaGiamGia { get; set; }

        // Không map DB – dùng để hiển thị tên trong dialog
        public string? TenLoai { get; set; }
    }
}
