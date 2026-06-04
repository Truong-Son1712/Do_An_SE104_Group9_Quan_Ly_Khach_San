using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using Đồ_Án_Quản_Lý_Khách_Sạn.Data;
using Đồ_Án_Quản_Lý_Khách_Sạn.Models;

namespace Đồ_Án_Quản_Lý_Khách_Sạn.Views.DichVu
{
    public partial class QuanLyDichVuPhongDialog : Window
    {
        private readonly int _maDatPhong;
        private List<DichVuRow> _rows = new();

        private class DichVuRow : INotifyPropertyChanged
        {
            private int _soLuong;
            public int    MaLoaiDV    { get; set; }
            public string TenLoaiDV   { get; set; } = "";
            public decimal DonGia     { get; set; }
            public string DonViTinh   { get; set; } = "";

            public int SoLuong
            {
                get => _soLuong;
                set { _soLuong = value < 0 ? 0 : value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SoLuong))); }
            }
            public event PropertyChangedEventHandler? PropertyChanged;
        }

        public QuanLyDichVuPhongDialog(int maDatPhong, string soPhong, string loaiPhong)
        {
            InitializeComponent();
            _maDatPhong   = maDatPhong;
            TxtTitle.Text = $"Dịch Vụ – Phòng {soPhong} ({loaiPhong})";
            LoadData();
        }

        private void LoadData()
        {
            using var ctx = new HotelDbContext();
            var loaiDVs  = ctx.LoaiDichVus.Where(l => l.IsActive).OrderBy(l => l.TenLoaiDV).ToList();
            var existing = ctx.DichVuPhongs.Where(d => d.MaDatPhong == _maDatPhong).ToList();

            _rows = loaiDVs.Select(l => new DichVuRow
            {
                MaLoaiDV   = l.MaLoaiDV,
                TenLoaiDV  = l.TenLoaiDV,
                DonGia     = l.DonGia,
                DonViTinh  = l.DonViTinh,
                SoLuong    = existing.FirstOrDefault(e => e.MaLoaiDV == l.MaLoaiDV)?.SoLuong ?? 0
            }).ToList();

            BuildUI();
        }

        private void BuildUI()
        {
            PnlItems.Children.Clear();
            foreach (var row in _rows)
            {
                var grid = new Grid { Margin = new Thickness(0, 0, 0, 6) };
                grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
                grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(100) });
                grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(110) });

                // Tên + đơn giá
                var pnlInfo = new StackPanel { VerticalAlignment = VerticalAlignment.Center };
                pnlInfo.Children.Add(new TextBlock
                {
                    Text = row.TenLoaiDV, FontSize = 13, FontWeight = FontWeights.SemiBold,
                    Foreground = System.Windows.Media.Brushes.DarkSlateBlue
                });
                pnlInfo.Children.Add(new TextBlock
                {
                    Text = $"{row.DonGia:N0} ₫/{row.DonViTinh}", FontSize = 11,
                    Foreground = System.Windows.Media.Brushes.Gray
                });
                Grid.SetColumn(pnlInfo, 0);

                // Nút − số lượng +
                var pnlQty = new StackPanel { Orientation = Orientation.Horizontal,
                    HorizontalAlignment = HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center };

                var btnMinus = new Button
                {
                    Content = "−", Width = 30, Height = 30, FontSize = 16,
                    Background = System.Windows.Media.Brushes.White,
                    BorderBrush = new System.Windows.Media.SolidColorBrush(
                        (System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#90A4AE")),
                    BorderThickness = new Thickness(1),
                    Cursor = System.Windows.Input.Cursors.Hand
                };
                var txtQty = new TextBlock
                {
                    Text = row.SoLuong.ToString(), Width = 36, TextAlignment = TextAlignment.Center,
                    FontSize = 14, FontWeight = FontWeights.Bold, VerticalAlignment = VerticalAlignment.Center
                };
                var btnPlus = new Button
                {
                    Content = "+", Width = 30, Height = 30, FontSize = 16,
                    Background = new System.Windows.Media.SolidColorBrush(
                        (System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#1565C0")),
                    Foreground = System.Windows.Media.Brushes.White,
                    BorderThickness = new Thickness(0),
                    Cursor = System.Windows.Input.Cursors.Hand
                };

                var capturedRow = row;
                var capturedTxt = txtQty;
                btnMinus.Click += (_, _) => { capturedRow.SoLuong--; capturedTxt.Text = capturedRow.SoLuong.ToString(); UpdateTotal(); };
                btnPlus.Click  += (_, _) => { capturedRow.SoLuong++; capturedTxt.Text = capturedRow.SoLuong.ToString(); UpdateTotal(); };

                pnlQty.Children.Add(btnMinus);
                pnlQty.Children.Add(txtQty);
                pnlQty.Children.Add(btnPlus);
                Grid.SetColumn(pnlQty, 1);

                // Thành tiền
                var lblTien = new TextBlock
                {
                    Text = row.SoLuong > 0 ? $"{row.SoLuong * row.DonGia:N0} ₫" : "—",
                    FontSize = 13, FontWeight = FontWeights.SemiBold,
                    HorizontalAlignment = HorizontalAlignment.Right,
                    VerticalAlignment = VerticalAlignment.Center,
                    Foreground = row.SoLuong > 0
                        ? new System.Windows.Media.SolidColorBrush(
                            (System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#F57C00"))
                        : System.Windows.Media.Brushes.Gray
                };
                Grid.SetColumn(lblTien, 2);

                var capturedLbl  = lblTien;
                var capturedRow2 = row;
                btnMinus.Click += (_, _) => RefreshTienLabel(capturedLbl, capturedRow2);
                btnPlus.Click  += (_, _) => RefreshTienLabel(capturedLbl, capturedRow2);

                grid.Children.Add(pnlInfo);
                grid.Children.Add(pnlQty);
                grid.Children.Add(lblTien);

                PnlItems.Children.Add(grid);
                PnlItems.Children.Add(new System.Windows.Shapes.Rectangle { Height = 1,
                    Fill = new System.Windows.Media.SolidColorBrush(
                        (System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#F0F0F0")),
                    Margin = new Thickness(0, 0, 0, 6) });
            }
            UpdateTotal();
        }

        private void RefreshTienLabel(TextBlock lbl, DichVuRow row)
        {
            if (row.SoLuong > 0)
            {
                lbl.Text = $"{row.SoLuong * row.DonGia:N0} ₫";
                lbl.Foreground = new System.Windows.Media.SolidColorBrush(
                    (System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#F57C00"));
            }
            else
            {
                lbl.Text = "—";
                lbl.Foreground = System.Windows.Media.Brushes.Gray;
            }
        }

        private void UpdateTotal()
        {
            decimal total = _rows.Sum(r => r.SoLuong * r.DonGia);
            TxtTong.Text = $"{total:N0} ₫";
        }

        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                using var ctx = new HotelDbContext();
                var old = ctx.DichVuPhongs.Where(d => d.MaDatPhong == _maDatPhong).ToList();
                ctx.DichVuPhongs.RemoveRange(old);
                foreach (var row in _rows.Where(r => r.SoLuong > 0))
                    ctx.DichVuPhongs.Add(new DichVuPhong { MaDatPhong = _maDatPhong, MaLoaiDV = row.MaLoaiDV, SoLuong = row.SoLuong, DonGia = row.DonGia, NgayThem = DateTime.Now });
                ctx.SaveChanges();
                DialogResult = true;
            }
            catch (Exception ex) { MessageBox.Show($"Lỗi: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error); }
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e) => Close();
        private void Header_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e) => DragMove();
    }
}
