using System.Windows;
using System.Windows.Controls;
using Đồ_Án_Quản_Lý_Khách_Sạn.Data;
using Microsoft.EntityFrameworkCore;

namespace Đồ_Án_Quản_Lý_Khách_Sạn.Views.BaoCao
{
    /// Hộp thoại hiển thị báo cáo thống kê doanh thu theo Loại phòng
    public partial class ThongKeLoaiPhongDialog : Window
    {
        private readonly int _year;
        private int _selectedMonth = 0;

        /// Lớp biểu diễn dòng thông tin thống kê của một loại phòng
        private class LoaiPhongItem
        {
            /// Tên loại phòng
            public string  TenLoaiPhong { get; set; } = "";
            /// Số lượt đặt của loại phòng này
            public int     SoLuotDat    { get; set; }
            /// Tổng doanh thu thu về từ loại phòng này
            public decimal DoanhThu     { get; set; }
            /// Tỉ lệ phần trăm doanh thu đóng góp
            public double  TyLe         { get; set; }
            /// Chuỗi hiển thị tỉ lệ phần trăm đóng góp
            public string  TyLeText     => $"{TyLe:0.#}%";
            /// Chuỗi định dạng hiển thị doanh thu bằng VNĐ
            public string  DoanhThuText => $"{DoanhThu:N0} ₫";
        }

        /// Khởi tạo hộp thoại thống kê loại phòng theo năm được truyền vào
        public ThongKeLoaiPhongDialog(int year)
        {
            InitializeComponent();
            _year = year;
            TxtTitle.Text = $"Thống Kê Doanh Thu Theo Loại Phòng – Năm {year}";

            CboThang.Items.Add(new ComboBoxItem { Content = "Cả năm", Tag = 0 });
            for (int m = 1; m <= 12; m++)
                CboThang.Items.Add(new ComboBoxItem { Content = $"Tháng {m}", Tag = m });
            CboThang.SelectedIndex = 0;

            LoadData();
        }

        /// Thực hiện truy xuất hóa đơn đã thanh toán để tổng hợp doanh thu theo loại phòng
        private void LoadData()
        {
            using var ctx = new HotelDbContext();
            var hds = ctx.HoaDons
                .Include(h => h.DatPhong).ThenInclude(d => d!.Phong).ThenInclude(p => p!.LoaiPhong)
                .Where(h => h.NgayLap.Year == _year && h.TrangThai == "DaThanhToan")
                .ToList();

            if (_selectedMonth > 0)
                hds = hds.Where(h => h.NgayLap.Month == _selectedMonth).ToList();

            decimal tong = hds.Sum(h => h.TongTien);
            TxtTong.Text = $"{tong:N0} ₫";

            var items = hds
                .Where(h => h.DatPhong?.Phong?.LoaiPhong != null)
                .GroupBy(h => h.DatPhong!.Phong!.LoaiPhong!.TenLoaiPhong)
                .Select(g =>
                {
                    decimal dt = g.Sum(h => h.TongTien);
                    return new LoaiPhongItem
                    {
                        TenLoaiPhong = g.Key,
                        SoLuotDat   = g.Count(),
                        DoanhThu    = dt,
                        TyLe        = tong > 0 ? Math.Round((double)(dt / tong * 100), 1) : 0
                    };
                })
                .OrderByDescending(x => x.DoanhThu)
                .ToList();

            DgLoaiPhong.ItemsSource = items;
        }

        /// Xử lý sự kiện khi thay đổi bộ lọc tháng thống kê trên ComboBox
        private void CboThang_Changed(object sender, SelectionChangedEventArgs e)
        {
            if (CboThang.SelectedItem is ComboBoxItem item && item.Tag is int month)
            {
                _selectedMonth = month;
                LoadData();
            }
        }

        /// Đóng cửa sổ hộp thoại thống kê
        private void BtnClose_Click(object sender, RoutedEventArgs e) => Close();

        /// Cho phép kéo di chuyển cửa sổ khi nhấp giữ chuột trái vào header
        private void Header_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e) => DragMove();
    }
}
