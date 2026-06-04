using System.Windows;
using System.Windows.Controls;
using Đồ_Án_Quản_Lý_Khách_Sạn.Data;
using Microsoft.EntityFrameworkCore;

namespace Đồ_Án_Quản_Lý_Khách_Sạn.Views.HoaDon
{
    public partial class XemHoaDonDialog : Window
    {
        public XemHoaDonDialog(int maHD)
        {
            InitializeComponent();
            LoadData(maHD);
        }

        private void LoadData(int maHD)
        {
            using var ctx = new HotelDbContext();
            var hd = ctx.HoaDons
                .Include(h => h.DatPhong).ThenInclude(d => d!.KhachHang)
                .Include(h => h.DatPhong).ThenInclude(d => d!.DatPhongKhachHangs).ThenInclude(x => x.KhachHang)
                .Include(h => h.DatPhong).ThenInclude(d => d!.Phong).ThenInclude(p => p!.LoaiPhong)
                .Include(h => h.NhanVien)
                .FirstOrDefault(h => h.MaHD == maHD);
            if (hd == null) return;

            var dp = hd.DatPhong;
            TxtTitle.Text = $"Hóa Đơn #{hd.MaHD}";
            TxtKhach.Text = BuildKhachText(dp);
            TxtPhong.Text    = $"Phòng {dp?.Phong?.SoPhong} – {dp?.Phong?.LoaiPhong?.TenLoaiPhong}";
            TxtNgayNhan.Text = dp?.NgayNhanPhong.ToString("dd/MM/yyyy HH:mm:ss") ?? "—";
            TxtNgayTra.Text  = dp?.NgayTraPhong.ToString("dd/MM/yyyy HH:mm:ss") ?? "—";

            int soNgay = dp != null
                ? Math.Max(1, (dp.NgayTraPhong.Date - dp.NgayNhanPhong.Date).Days)
                : 1;
            TxtSoNgayThue.Text = $"{soNgay} đêm";
            TxtNgayLap.Text  = hd.NgayThanhToan.HasValue
                ? hd.NgayThanhToan.Value.ToString("dd/MM/yyyy HH:mm:ss")
                : "—";
            TxtNhanVien.Text  = $"NV: {hd.NhanVien?.HoTen}";
            TxtPhuongThuc.Text = hd.PhuongThucTT switch
            {
                "TienMat"     => "Tiền Mặt",
                "ChuyenKhoan" => "Chuyển Khoản",
                "The"         => "Thẻ Ngân Hàng",
                _             => hd.PhuongThucTT
            };

            if (hd.TrangThai == "DaThanhToan")
            {
                BorderTrangThai.Background = new System.Windows.Media.SolidColorBrush(
                    (System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#E8F5E9"));
                TxtTrangThai.Text       = "✔  Đã Thanh Toán";
                TxtTrangThai.Foreground = new System.Windows.Media.SolidColorBrush(
                    (System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#2E7D32"));
            }
            else
            {
                BorderTrangThai.Background = new System.Windows.Media.SolidColorBrush(
                    (System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#FFF3E0"));
                TxtTrangThai.Text       = "⏳  Chưa Thanh Toán";
                TxtTrangThai.Foreground = new System.Windows.Media.SolidColorBrush(
                    (System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#E65100"));
            }

            // Tải và hiển thị chi tiết dịch vụ
            // Tiền phòng
            TxtTienPhong.Text = $"{hd.TienPhong:N0} ₫";

            var dichVus = ctx.DichVuPhongs
                .Include(d => d.LoaiDichVu)
                .Where(d => d.MaDatPhong == dp!.MaDatPhong && d.SoLuong > 0)
                .ToList();

            if (dichVus.Any())
            {
                PnlDichVu.Visibility = Visibility.Visible;
                PnlDichVuRows.Children.Clear();
                foreach (var dv in dichVus)
                {
                    var row = new Grid();
                    row.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
                    row.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
                    row.Margin = new Thickness(0, 0, 0, 4);

                    var lblTen = new TextBlock
                    {
                        Text = $"  • {dv.LoaiDichVu?.TenLoaiDV}  ×{dv.SoLuong} {dv.LoaiDichVu?.DonViTinh}  ({dv.DonGia:N0} ₫/{dv.LoaiDichVu?.DonViTinh})",
                        FontSize = 12,
                        Foreground = System.Windows.Media.Brushes.DimGray,
                        VerticalAlignment = VerticalAlignment.Center
                    };
                    Grid.SetColumn(lblTen, 0);

                    var lblGia = new TextBlock
                    {
                        Text = $"{dv.SoLuong * dv.DonGia:N0} ₫",
                        FontSize = 12,
                        Foreground = new System.Windows.Media.SolidColorBrush(
                            (System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#F57C00")),
                        FontWeight = FontWeights.SemiBold,
                        VerticalAlignment = VerticalAlignment.Center
                    };
                    Grid.SetColumn(lblGia, 1);
                    row.Children.Add(lblTen);
                    row.Children.Add(lblGia);
                    PnlDichVuRows.Children.Add(row);
                }
                decimal tongDV = dichVus.Sum(d => d.SoLuong * d.DonGia);
                TxtTienDichVu.Text = $"{tongDV:N0} ₫";
            }

            // VAT
            if (hd.TienVAT > 0)
            {
                RowVAT.Visibility = Visibility.Visible;
                TxtVATLabel.Text  = $"Thuế VAT ({hd.VATPercent:0.##}%):";
                TxtTienVAT.Text   = $"{hd.TienVAT:N0} ₫";
            }

            // Mã giảm giá
            if (hd.TienGiam > 0 && hd.MaGG.HasValue)
            {
                string tenMa = ctx.MaGiamGias.Find(hd.MaGG)?.TenMa ?? "";
                RowGiamGia.Visibility   = Visibility.Visible;
                TxtMaGiamGiaHD.Text     = $"Giảm giá ({tenMa}):";
                TxtTienGiam.Text        = $"- {hd.TienGiam:N0} ₫";
            }

            decimal thanhToanTraPhong = Math.Max(0, hd.TongTien - hd.TienCoc);
            TxtTienCoc.Text           = $"{hd.TienCoc:N0} ₫";
            TxtThanhToanTraPhong.Text = $"{thanhToanTraPhong:N0} ₫";
            RowCoc.Opacity            = hd.TienCoc > 0 ? 1.0 : 0.4;
            TxtTong.Text              = $"{hd.TongTien:N0} ₫";
        }

        private static string BuildKhachText(Models.DatPhong? dp)
        {
            if (dp == null) return "";
            string primary = dp.KhachHang?.HoTen ?? "";
            var others = dp.DatPhongKhachHangs
                .Where(x => x.MaKH != dp.MaKH)
                .Select(x => x.KhachHang?.HoTen ?? "")
                .Where(s => s.Length > 0)
                .ToList();

            var parts = new List<string>();
            if (primary.Length > 0) parts.Add($"★ {primary}");
            parts.AddRange(others);
            return string.Join(", ", parts);
        }

        private void BtnClose_Click(object sender, RoutedEventArgs e) => Close();
        private void Header_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e) => DragMove();

    }
}
