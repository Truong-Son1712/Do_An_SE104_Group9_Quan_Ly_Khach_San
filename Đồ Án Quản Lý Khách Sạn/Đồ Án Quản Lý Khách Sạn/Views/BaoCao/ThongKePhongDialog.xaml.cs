using System.Windows;
using System.Windows.Controls;
using Đồ_Án_Quản_Lý_Khách_Sạn.Data;
using Microsoft.EntityFrameworkCore;

namespace Đồ_Án_Quản_Lý_Khách_Sạn.Views.BaoCao
{
    public partial class ThongKePhongDialog : Window
    {
        private readonly int _year;
        private int _selectedMonth = 0; // 0 = cả năm

        private class PhongItem
        {
            public string  SoPhong      { get; set; } = "";
            public string  TenLoaiPhong { get; set; } = "";
            public int     SoLuotDat    { get; set; }
            public decimal DoanhThu     { get; set; }
            public double  TyLe         { get; set; }
            public string  TyLeText     => $"{TyLe:0.#}%";
            public string  DoanhThuText => $"{DoanhThu:N0} ₫";
        }

        public ThongKePhongDialog(int year)
        {
            InitializeComponent();
            _year = year;
            TxtTitle.Text = $"Thống Kê Doanh Thu Theo Phòng – Năm {year}";

            CboThang.Items.Add(new ComboBoxItem { Content = "Cả năm", Tag = 0 });
            for (int m = 1; m <= 12; m++)
                CboThang.Items.Add(new ComboBoxItem { Content = $"Tháng {m}", Tag = m });
            CboThang.SelectedIndex = 0;

            LoadData();
        }

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
                .Where(h => h.DatPhong?.Phong != null)
                .GroupBy(h => new
                {
                    SoPhong      = h.DatPhong!.Phong!.SoPhong,
                    TenLoaiPhong = h.DatPhong.Phong.LoaiPhong?.TenLoaiPhong ?? ""
                })
                .Select(g =>
                {
                    decimal dt = g.Sum(h => h.TongTien);
                    return new PhongItem
                    {
                        SoPhong      = g.Key.SoPhong,
                        TenLoaiPhong = g.Key.TenLoaiPhong,
                        SoLuotDat    = g.Count(),
                        DoanhThu     = dt,
                        TyLe         = tong > 0 ? Math.Round((double)(dt / tong * 100), 1) : 0
                    };
                })
                .OrderByDescending(x => x.DoanhThu)
                .ToList();

            DgPhong.ItemsSource = items;
        }

        private void CboThang_Changed(object sender, SelectionChangedEventArgs e)
        {
            if (CboThang.SelectedItem is ComboBoxItem item && item.Tag is int month)
            {
                _selectedMonth = month;
                LoadData();
            }
        }

        private void BtnClose_Click(object sender, RoutedEventArgs e) => Close();
        private void Header_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e) => DragMove();
    }
}
