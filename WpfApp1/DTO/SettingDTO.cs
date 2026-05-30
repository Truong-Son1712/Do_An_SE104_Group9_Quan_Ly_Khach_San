using System;

namespace WpfApp1.DTO
{
    /// <summary>
    /// Đối tượng DTO đại diện cho các quy định cấu hình của khách sạn.
    /// File này được tạo mới cho nhiệm vụ của Thành viên 5.
    /// Đảm bảo tương thích 100% với kiến trúc của main branch.
    /// </summary>
    public class SettingDTO
    {
        // Tỷ lệ phụ thu khi có thêm khách thứ 3 (ví dụ: 0.25 = 25%)
        public decimal PhuThuKhachThu3 { get; set; }

        // Số khách tối đa cho phép trong 1 phòng
        public int SoKhachToiDa { get; set; }

        // Tỷ lệ tiền cọc tối thiểu khi đặt phòng (ví dụ: 0.5 = 50%)
        public decimal TyLeTienCoc { get; set; }

        // Giờ nhận phòng mặc định (ví dụ: 14:00)
        public TimeSpan GioNhanPhong { get; set; }

        // Giờ trả phòng mặc định (ví dụ: 12:00)
        public TimeSpan GioTraPhong { get; set; }
    }
}
