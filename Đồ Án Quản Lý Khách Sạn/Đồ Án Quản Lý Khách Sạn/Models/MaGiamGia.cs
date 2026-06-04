namespace Đồ_Án_Quản_Lý_Khách_Sạn.Models
{
    public class MaGiamGia
    {
        public int MaGG { get; set; }
        public string TenMa { get; set; } = "";
        public string? MoTa { get; set; }
        public DateTime NgayBatDau { get; set; }
        public DateTime NgayKetThuc { get; set; }
        public string TrangThai { get; set; } = "Active"; // Active | Inactive
        public int? SoLuongToiDa { get; set; } // null = vô hạn
        public DateTime NgayTao { get; set; } = DateTime.Now;
        public int? MaNVTao { get; set; }

        public virtual NhanVien? NhanVien { get; set; }
        public virtual ICollection<ChiTietMaGiamGia> ChiTiets { get; set; } = new List<ChiTietMaGiamGia>();
        public virtual ICollection<LichSuDungMaGiam> LichSuDungs { get; set; } = new List<LichSuDungMaGiam>();

        public bool DaCoNguoiDung => LichSuDungs.Any();

        public int SoLuongDaDung => LichSuDungs.Count;

        public bool ConLuot => SoLuongToiDa == null || LichSuDungs.Count < SoLuongToiDa;

        public string SoLuongHienThi =>
            SoLuongToiDa == null ? $"{SoLuongDaDung} / ∞" : $"{SoLuongDaDung} / {SoLuongToiDa}";

        public string TrangThaiHienThi
        {
            get
            {
                if (TrangThai == "Inactive") return "Vô hiệu";
                var now = DateTime.Now;
                if (now < NgayBatDau) return "Chưa đến hạn";
                if (now > NgayKetThuc) return "Hết hạn";
                if (!ConLuot) return "Hết lượt";
                return "Đang hoạt động";
            }
        }
    }
}
