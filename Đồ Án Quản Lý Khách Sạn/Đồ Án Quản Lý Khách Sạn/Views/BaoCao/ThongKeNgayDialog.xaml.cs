using System.Windows;
using System.Windows.Controls;
using Đồ_Án_Quản_Lý_Khách_Sạn.Data;
using Đồ_Án_Quản_Lý_Khách_Sạn.Models;

namespace Đồ_Án_Quản_Lý_Khách_Sạn.Views.BaoCao
{
    public partial class ThongKeNgayDialog : Window
    {
        private readonly int _year;
        private int _selectedMonth;

        private class NgayItem
        {
            public string  NgayText      { get; set; } = "";
            public decimal DoanhThu      { get; set; }
            public int     SoHoaDon      { get; set; }
            public double  TyLe          { get; set; } // % so với ngày cao nhất (scale bar)
            public double  TyLeThang     { get; set; } // % so với tổng tháng
            public string  TyLeThangText => TyLeThang > 0 ? $"{TyLeThang:0.#}%" : "—";
            public string  DoanhThuText  => DoanhThu > 0 ? $"{DoanhThu:N0} ₫" : "0 ₫";
        }

        public ThongKeNgayDialog(int year)
        {
            InitializeComponent();
            _year = year;
            TxtTitle.Text = $"Thống Kê Doanh Thu Theo Ngày – Năm {year}";
            TxtNam.Text   = $"/{year}";

            for (int m = 1; m <= 12; m++)
                CboThang.Items.Add(new ComboBoxItem { Content = $"Tháng {m}", Tag = m });

            _selectedMonth = DateTime.Today.Month;
            CboThang.SelectedIndex = _selectedMonth - 1;
        }

        private void LoadData()
        {
            using var ctx = new HotelDbContext();

            int daysInMonth = DateTime.DaysInMonth(_year, _selectedMonth);

            // Hóa đơn đã thanh toán trong tháng
            var hdsThang = ctx.HoaDons
                .Where(h => h.NgayLap.Year == _year
                         && h.NgayLap.Month == _selectedMonth
                         && h.TrangThai == "DaThanhToan")
                .ToList();

            // Tiền cọc nhận trong tháng (booking chưa có HĐ DaThanhToan)
            var paidIds = ctx.HoaDons
                .Where(h => h.TrangThai == "DaThanhToan")
                .Select(h => h.MaDatPhong)
                .ToHashSet();

            var cocThang = ctx.DatPhongs
                .Where(d => d.NgayDat.Year == _year
                         && d.NgayDat.Month == _selectedMonth
                         && d.TrangThai != TrangThaiDatPhong.HuyDat
                         && d.TienCoc > 0)
                .ToList()
                .Where(d => !paidIds.Contains(d.MaDatPhong))
                .ToList();

            // Tổng tháng
            decimal tongThang = hdsThang.Sum(h => h.TongTien)
                               + cocThang.Sum(d => d.TienCoc);
            TxtTong.Text = $"{tongThang:N0} ₫";

            // Tính doanh thu từng ngày
            var items = new List<NgayItem>();
            for (int day = 1; day <= daysInMonth; day++)
            {
                decimal dtHd  = hdsThang.Where(h => h.NgayLap.Day == day).Sum(h => h.TongTien);
                decimal dtCoc = cocThang.Where(d => d.NgayDat.Day == day).Sum(d => d.TienCoc);
                decimal dt    = dtHd + dtCoc;
                int     soHd  = hdsThang.Count(h => h.NgayLap.Day == day);

                items.Add(new NgayItem
                {
                    NgayText  = day.ToString("00"),
                    DoanhThu  = dt,
                    SoHoaDon  = soHd,
                    TyLeThang = tongThang > 0 ? Math.Round((double)(dt / tongThang * 100), 1) : 0
                });
            }

            // Bar khớp với tỷ lệ % hiển thị (cùng tính trên tổng tháng)
            foreach (var item in items)
                item.TyLe = item.TyLeThang;

            IcNgay.ItemsSource = items;
        }

        private void CboThang_Changed(object sender, SelectionChangedEventArgs e)
        {
            if (CboThang.SelectedItem is ComboBoxItem ci && ci.Tag is int month)
            {
                _selectedMonth = month;
                LoadData();
            }
        }

        private void BtnClose_Click(object sender, RoutedEventArgs e) => Close();
        private void Header_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e) => DragMove();
    }
}
