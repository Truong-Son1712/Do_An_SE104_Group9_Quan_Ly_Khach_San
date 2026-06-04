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
            TxtTitle.Text    = $"Hóa Đơn #{hd.MaHD}";
            TxtNguoiDat.Text = dp?.KhachHang?.HoTen ?? "—";
            var stayingNames = dp?.DatPhongKhachHangs
                .Select(x => x.KhachHang?.HoTen ?? "")
                .Where(s => s.Length > 0)
                .ToList() ?? new List<string>();
            TxtKhachO.Text   = stayingNames.Any() ? string.Join(", ", stayingNames) : "—";
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
            ApplyRoomPriceBreakdown(hd);

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

        private void ApplyRoomPriceBreakdown(Models.HoaDon hd)
        {
            bool hasLoai = hd.GiaPhongGoc > 0 && hd.HeSoLoaiKhach > 1m;
            bool hasSC   = hd.GiaPhongGoc > 0 && hd.TiLePhuThuSucChua > 0m;

            TxtGiaPhongGoc.Text = hd.GiaPhongGoc > 0
                ? $"{hd.GiaPhongGoc:N0} ₫"
                : $"{hd.TienPhong:N0} ₫";

            if (hasLoai)
            {
                RowPhuThuLoai.Visibility = Visibility.Visible;
                TxtLabelPhuThuLoai.Text  = $"+ Phụ thu {hd.TenLoaiKhachMax} (×{hd.HeSoLoaiKhach:0.####})";
                TxtPhuThuLoai.Text       = $"+{hd.GiaPhongGoc * (hd.HeSoLoaiKhach - 1):N0} ₫";
            }
            if (hasSC)
            {
                RowPhuThuSC.Visibility   = Visibility.Visible;
                TxtLabelPhuThuSC.Text    = $"+ Phụ thu vượt sức chứa ({hd.TiLePhuThuSucChua:P0})";
                TxtPhuThuSC.Text         = $"+{hd.GiaPhongGoc * hd.TiLePhuThuSucChua:N0} ₫";
            }
            if (hasLoai || hasSC)
            {
                RowTienPhongTotal.Visibility = Visibility.Visible;
                TxtTienPhong.Text            = $"{hd.TienPhong:N0} ₫";
            }
            else
            {
                TxtLabelGiaGoc.Text          = "Tiền phòng";
                RowTienPhongTotal.Visibility  = Visibility.Collapsed;
            }
        }

        private void BtnClose_Click(object sender, RoutedEventArgs e) => Close();
        private void Header_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e) => DragMove();

    }
}
