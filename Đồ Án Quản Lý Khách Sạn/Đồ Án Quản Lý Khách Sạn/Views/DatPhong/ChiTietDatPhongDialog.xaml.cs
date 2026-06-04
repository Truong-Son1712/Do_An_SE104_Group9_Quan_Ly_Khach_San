using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Đồ_Án_Quản_Lý_Khách_Sạn.Data;
using Đồ_Án_Quản_Lý_Khách_Sạn.Models;
using Microsoft.EntityFrameworkCore;
using KhachHangModel = Đồ_Án_Quản_Lý_Khách_Sạn.Models.KhachHang;

namespace Đồ_Án_Quản_Lý_Khách_Sạn.Views.DatPhong
{
    public partial class ChiTietDatPhongDialog : Window
    {
        private class KhachDisplay
        {
            public string TieuDe      { get; set; } = "";
            // Badge "✓ Có sử dụng phòng" hoặc "Không ở phòng này"
            public string BadgeText   { get; set; } = "";
            public string BadgeColor  { get; set; } = "#2E7D32";
            public Visibility BadgeVisibility => string.IsNullOrEmpty(BadgeText)
                ? Visibility.Collapsed : Visibility.Visible;
            public string MaKH        { get; set; } = "";
            public string CMND        { get; set; } = "";
            public string GioiTinh    { get; set; } = "";
            public string NgaySinh    { get; set; } = "";
            public string SDT         { get; set; } = "";
            public string QuocTich    { get; set; } = "";
            public string LoaiKhach   { get; set; } = "";
            public string DiaChi      { get; set; } = "";
            public bool   IsNguoiDat  { get; set; }
            // Cam-đậm = người đặt phòng, xanh dương = khách ở
            public string HeaderColor => IsNguoiDat ? "#BF360C" : "#1565C0";
        }

        public ChiTietDatPhongDialog(int maDatPhong)
        {
            InitializeComponent();
            LoadData(maDatPhong);
        }

        private void LoadData(int id)
        {
            using var ctx = new HotelDbContext();
            // Giữ ctx mở trong toàn bộ method để LoadThanhToan dùng chung
            var dp = ctx.DatPhongs
                .Include(d => d.Phong).ThenInclude(p => p!.LoaiPhong)
                .Include(d => d.KhachHang)
                .Include(d => d.DatPhongKhachHangs).ThenInclude(x => x.KhachHang)
                .FirstOrDefault(d => d.MaDatPhong == id);
            if (dp == null) return;

            TxtTitle.Text = $"Chi Tiết Đặt Phòng #{dp.MaDatPhong}";
            TxtMaDP.Text  = $"#{dp.MaDatPhong}";
            TxtPhong.Text = $"Phòng {dp.Phong?.SoPhong} – {dp.Phong?.LoaiPhong?.TenLoaiPhong}";
            TxtNgayDat.Text = dp.NgayDat.ToString("dd/MM/yyyy HH:mm:ss");

            bool daCheckIn  = dp.TrangThai == TrangThaiDatPhong.DaNhanPhong
                           || dp.TrangThai == TrangThaiDatPhong.DaTraPhong;
            bool daCheckOut = dp.TrangThai == TrangThaiDatPhong.DaTraPhong;

            LblNhanPhong.Text = daCheckIn  ? "Nhận phòng (thực tế):" : "Nhận phòng (dự kiến):";
            LblTraPhong.Text  = daCheckOut ? "Trả phòng (thực tế):"  : "Trả phòng (dự kiến):";
            TxtNgayNhan.Text  = dp.NgayNhanPhong.ToString(daCheckIn  ? "dd/MM/yyyy HH:mm:ss" : "dd/MM/yyyy");
            TxtNgayTra.Text   = dp.NgayTraPhong.ToString(daCheckOut  ? "dd/MM/yyyy HH:mm:ss" : "dd/MM/yyyy");

            TxtTrangThai.Text = dp.TrangThai switch
            {
                TrangThaiDatPhong.DaDat       => "Đã Đặt – chờ nhận phòng",
                TrangThaiDatPhong.DaNhanPhong => "Đã Nhận Phòng",
                TrangThaiDatPhong.DaTraPhong  => "Đã Trả Phòng",
                TrangThaiDatPhong.HuyDat      => "Đã Hủy",
                _                             => dp.TrangThai.ToString()
            };
            TxtTienCoc.Text = dp.TienCoc > 0 ? $"{dp.TienCoc:N0} ₫" : "Không có";
            TxtGhiChu.Text  = string.IsNullOrWhiteSpace(dp.GhiChu) ? "—" : dp.GhiChu;

            var loaiDict = ctx.LoaiKhachHangs.ToDictionary(l => l.MaLKH, l => l.TenLoai);

            // Tải chi tiết thanh toán
            LoadThanhToan(ctx, dp);

            var displayList = new List<KhachDisplay>();

            // ── 1. Người đặt phòng (luôn hiển thị đầu tiên) ───────────────
            if (dp.KhachHang != null)
            {
                bool coOPhong = dp.DatPhongKhachHangs.Any(x => x.MaKH == dp.MaKH);
                displayList.Add(BuildDisplay(
                    tieuDe:      $"Người Đặt Phòng:  {dp.KhachHang.HoTen}",
                    badgeText:   coOPhong ? "✓ Có sử dụng phòng" : "Không ở phòng này",
                    badgeColor:  coOPhong ? "#2E7D32" : "#78909C",
                    isNguoiDat:  true,
                    k:           dp.KhachHang,
                    loaiDict:    loaiDict));
            }

            // ── 2. Khách ở phòng (loại trừ người đặt – đã hiển thị ở trên) ─
            int idx = 1;
            foreach (var x in dp.DatPhongKhachHangs
                         .Where(x => x.MaKH != dp.MaKH && x.KhachHang != null)
                         .OrderBy(x => x.KhachHang!.HoTen))
            {
                displayList.Add(BuildDisplay(
                    tieuDe:      $"Khách {idx++}:  {x.KhachHang!.HoTen}",
                    badgeText:   "",
                    badgeColor:  "",
                    isNguoiDat:  false,
                    k:           x.KhachHang!,
                    loaiDict:    loaiDict));
            }

            // Fallback dữ liệu cũ: nếu không có DatPhongKhachHangs và không có người đặt → không hiển thị thêm
            IcKhachHang.ItemsSource = displayList;
        }

        private static KhachDisplay BuildDisplay(
            string tieuDe, string badgeText, string badgeColor, bool isNguoiDat,
            KhachHangModel k, Dictionary<int, string> loaiDict) => new KhachDisplay
        {
            TieuDe    = tieuDe,
            BadgeText = badgeText,
            BadgeColor = badgeColor,
            IsNguoiDat = isNguoiDat,
            MaKH      = $"KH{k.MaKH:D4}",
            CMND      = string.IsNullOrWhiteSpace(k.CMND) ? "—" : k.CMND,
            GioiTinh  = k.GioiTinh == "Nu" ? "Nữ" : "Nam",
            NgaySinh  = k.NgaySinh.HasValue ? k.NgaySinh.Value.ToString("dd/MM/yyyy") : "—",
            SDT       = k.SDT ?? "—",
            QuocTich  = k.QuocTich,
            LoaiKhach = loaiDict.TryGetValue(k.MaLoaiKH, out var ten) ? ten : "—",
            DiaChi    = k.DiaChi ?? "—"
        };

        private void LoadThanhToan(HotelDbContext ctx, Models.DatPhong dp)
        {
            // Lấy hóa đơn nếu có
            var hd = ctx.HoaDons
                .Include(h => h.MaGiamGia)
                .FirstOrDefault(h => h.MaDatPhong == dp.MaDatPhong);

            // Lấy dịch vụ đã dùng
            var dichVus = ctx.DichVuPhongs
                .Include(d => d.LoaiDichVu)
                .Where(d => d.MaDatPhong == dp.MaDatPhong && d.SoLuong > 0)
                .ToList();

            // Chỉ hiện section nếu có hóa đơn hoặc có dịch vụ
            if (hd == null && !dichVus.Any()) return;
            PnlThanhToan.Visibility = Visibility.Visible;

            // Tiền phòng (với breakdown nếu có hóa đơn)
            decimal tienPhong = hd?.TienPhong ?? 0;
            if (hd != null)
                ApplyRoomPriceBreakdown(hd);
            else
            {
                TxtGiaPhongGoc.Text  = "— (chưa lập hóa đơn)";
                TxtLabelGiaGoc.Text  = "Tiền phòng:";
            }

            // Dịch vụ
            if (dichVus.Any())
            {
                PnlDichVuCT.Visibility = Visibility.Visible;
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
                        Foreground = Brushes.DimGray,
                        VerticalAlignment = VerticalAlignment.Center
                    };
                    Grid.SetColumn(lblTen, 0);

                    var lblGia = new TextBlock
                    {
                        Text = $"{dv.SoLuong * dv.DonGia:N0} ₫",
                        FontSize = 12,
                        Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#F57C00")),
                        FontWeight = FontWeights.SemiBold,
                        VerticalAlignment = VerticalAlignment.Center
                    };
                    Grid.SetColumn(lblGia, 1);
                    row.Children.Add(lblTen);
                    row.Children.Add(lblGia);
                    PnlDichVuRows.Children.Add(row);
                }
                TxtTongDichVu.Text = $"{dichVus.Sum(d => d.SoLuong * d.DonGia):N0} ₫";
            }

            if (hd == null)
            {
                TxtConLaiCT.Text = "—";
                TxtCocCT.Text    = $"{dp.TienCoc:N0} ₫";
                TxtTongCT.Text   = "—";
                return;
            }

            // Giảm giá
            if (hd.TienGiam > 0)
            {
                RowGiamCT.Visibility  = Visibility.Visible;
                string tenMa = hd.MaGiamGia?.TenMa ?? "";
                TxtGiamLabel.Text    = $"Giảm giá ({tenMa}):";
                TxtTienGiamCT.Text   = $"- {hd.TienGiam:N0} ₫";
            }

            // VAT
            if (hd.TienVAT > 0)
            {
                RowVATCT.Visibility  = Visibility.Visible;
                TxtVATLabelCT.Text   = $"Thuế VAT ({hd.VATPercent:0.##}%):";
                TxtTienVATCT.Text    = $"{hd.TienVAT:N0} ₫";
            }

            // Cọc, thanh toán, tổng
            decimal conLai = Math.Max(0, hd.TongTien - hd.TienCoc);
            TxtCocCT.Text     = $"{hd.TienCoc:N0} ₫";
            TxtConLaiCT.Text  = $"{conLai:N0} ₫";
            TxtTongCT.Text    = $"{hd.TongTien:N0} ₫";
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
                TxtLabelPhuThuLoai.Text  = $"+ Phụ thu {hd.TenLoaiKhachMax} (×{hd.HeSoLoaiKhach:0.####}):";
                TxtPhuThuLoai.Text       = $"+{hd.GiaPhongGoc * (hd.HeSoLoaiKhach - 1):N0} ₫";
            }
            if (hasSC)
            {
                RowPhuThuSC.Visibility   = Visibility.Visible;
                TxtLabelPhuThuSC.Text    = $"+ Phụ thu vượt sức chứa ({hd.TiLePhuThuSucChua:P0}):";
                TxtPhuThuSC.Text         = $"+{hd.GiaPhongGoc * hd.TiLePhuThuSucChua:N0} ₫";
            }
            if (hasLoai || hasSC)
            {
                RowTienPhongTotal.Visibility = Visibility.Visible;
                TxtTienPhongCT.Text          = $"{hd.TienPhong:N0} ₫";
            }
            else
            {
                TxtLabelGiaGoc.Text          = "Tiền phòng:";
                RowTienPhongTotal.Visibility  = Visibility.Collapsed;
                TxtTienPhongCT.Text           = $"{hd.TienPhong:N0} ₫";
            }
        }

        private void BtnClose_Click(object sender, RoutedEventArgs e) => Close();
        private void Header_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e) => DragMove();
    }
}
