using System;

namespace WpfApp1.DTO
{
    /// <summary>
    /// Đối tượng DTO biểu diễn dữ liệu của 1 tháng trong báo cáo doanh thu.
    /// File này được tạo mới cho nhiệm vụ của Thành viên 5.
    /// </summary>
    public class RevenueReportDTO
    {
        public int Thang { get; set; }
        public int Nam { get; set; }
        public decimal DoanhThu { get; set; }
        
        // Properties hỗ trợ vẽ biểu đồ
        public string ThangHienThi => $"T{Thang}";
        public double TyLe { get; set; } // % chiều cao cột biểu đồ so với tháng cao nhất
    }
}
