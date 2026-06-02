namespace Đồ_Án_Quản_Lý_Khách_Sạn.Models
{
    /// <summary>
    /// Lớp biểu diễn thực thể Hóa đơn thanh toán trong hệ thống.
    /// Lưu trữ thông tin tiền phòng, tiền cọc, tổng doanh thu và phương thức thanh toán.
    /// </summary>
    public class HoaDon
    {
        /// <summary>
        /// Mã định danh hóa đơn (Khóa chính, tự động tăng).
        /// </summary>
        public int MaHD { get; set; }

        /// <summary>
        /// Mã đặt phòng liên kết với hóa đơn này (Khóa ngoại).
        /// </summary>
        public int MaDatPhong { get; set; }

        /// <summary>
        /// Mã nhân viên thực hiện lập hoặc thu tiền hóa đơn (Khóa ngoại).
        /// </summary>
        public int MaNV { get; set; }

        /// <summary>
        /// Ngày lập hóa đơn (Mặc định là ngày giờ hiện tại).
        /// </summary>
        public DateTime NgayLap { get; set; } = DateTime.Now;

        /// <summary>
        /// Tiền thuê phòng (Đã bao gồm phụ thu sức chứa và hệ số khách nước ngoài).
        /// </summary>
        public decimal TienPhong { get; set; }

        /// <summary>
        /// Tiền đặt cọc trước của khách hàng (Được khấu trừ khi thanh toán).
        /// </summary>
        public decimal TienCoc { get; set; }

        /// <summary>
        /// Tổng số tiền thực tế ghi nhận của hóa đơn này.
        /// </summary>
        public decimal TongTien { get; set; }

        /// <summary>
        /// Phương thức thanh toán (TienMat, ChuyenKhoan, The).
        /// </summary>
        public string PhuongThucTT { get; set; } = "TienMat"; // TienMat | ChuyenKhoan | The

        /// <summary>
        /// Trạng thái của hóa đơn (ChuaThanhToan, DaThanhToan).
        /// </summary>
        public string TrangThai { get; set; } = "ChuaThanhToan"; // ChuaThanhToan | DaThanhToan

        /// <summary>
        /// Ngày giờ thực tế khách hàng thực hiện thanh toán hóa đơn.
        /// </summary>
        public DateTime? NgayThanhToan { get; set; }

        /// <summary>
        /// Ghi chú kèm theo hóa đơn.
        /// </summary>
        public string? GhiChu { get; set; }

        /// <summary>
        /// Đối tượng liên kết biểu diễn chi tiết đặt phòng tương ứng.
        /// </summary>
        public virtual DatPhong? DatPhong { get; set; }

        /// <summary>
        /// Đối tượng liên kết biểu diễn nhân viên lập hóa đơn.
        /// </summary>
        public virtual NhanVien? NhanVien { get; set; }

        /// <summary>
        /// Tính toán tổng số ngày thuê dựa trên ngày nhận và trả phòng thực tế.
        /// </summary>
        public int SoNgay => DatPhong == null ? 1
            : Math.Max(1, (DatPhong.NgayTraPhong.Date - DatPhong.NgayNhanPhong.Date).Days);

        /// <summary>
        /// Số tiền còn lại mà khách hàng phải thanh toán sau khi trừ tiền cọc.
        /// </summary>
        public decimal ConLai => Math.Max(0, TienPhong - TienCoc);
    }
}

