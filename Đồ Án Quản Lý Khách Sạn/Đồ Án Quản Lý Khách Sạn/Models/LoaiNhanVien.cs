namespace Đồ_Án_Quản_Lý_Khách_Sạn.Models
{
    public class LoaiNhanVien
    {
        public int    MaLoaiNV   { get; set; }
        public string TenLoai    { get; set; } = "";
        public string? MoTa      { get; set; }
        public string VaiTroCode { get; set; } = ""; // Khớp với NhanVien.VaiTro
        public bool   IsBuiltIn  { get; set; } = false; // Admin/QuanLy/LeTan không xóa được

        public virtual ICollection<LoaiNhanVienQuyen> Quyens     { get; set; } = new List<LoaiNhanVienQuyen>();

        // Số nhân viên thuộc loại này (computed, không lưu DB)
        public int SoNhanVien { get; set; }
    }
}
