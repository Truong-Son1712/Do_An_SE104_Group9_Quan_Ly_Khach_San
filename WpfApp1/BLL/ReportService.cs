using System;
using System.Collections.Generic;
using System.Linq;
using WpfApp1.DTO;

namespace WpfApp1.BLL
{
    /// <summary>
    /// Tầng nghiệp vụ lấy dữ liệu thống kê.
    /// HIỆN TẠI ĐANG DÙNG MOCK DATA vì DB chưa có bảng HOADON, PHONG.
    /// Khi các thành viên khác hoàn thiện DAL, ta sẽ thay Mock Data bằng gọi hàm DAL.
    /// </summary>
    public class ReportService
    {
        public DashboardDTO GetDashboardData()
        {
            // Trả về dữ liệu giả lập (Mock) cho giao diện đẹp
            return new DashboardDTO
            {
                TongPhong = 50,
                PhongTrong = 15,
                PhongDangSuDung = 30,
                PhongBaoDuong = 5,
                DatPhongHomNay = 12,
                TraPhongHomNay = 8,
                TongKhachHang = 1250,
                DoanhThuThangHienTai = 450000000 // 450 triệu
            };
        }

        public List<RevenueReportDTO> GetDoanhThuTheoNam(int year)
        {
            var list = new List<RevenueReportDTO>();
            var random = new Random(year);
            
            // Tạo mock doanh thu cho 12 tháng
            for (int i = 1; i <= 12; i++)
            {
                // Giả lập doanh thu từ 50tr đến 200tr
                decimal dt = random.Next(50, 200) * 1000000m;
                list.Add(new RevenueReportDTO
                {
                    Thang = i,
                    Nam = year,
                    DoanhThu = dt
                });
            }

            // Tính tỷ lệ % so với tháng cao nhất để vẽ biểu đồ cột
            decimal maxDt = list.Max(x => x.DoanhThu);
            if (maxDt > 0)
            {
                foreach (var item in list)
                {
                    item.TyLe = (double)(item.DoanhThu / maxDt) * 100;
                }
            }

            return list;
        }
    }
}
