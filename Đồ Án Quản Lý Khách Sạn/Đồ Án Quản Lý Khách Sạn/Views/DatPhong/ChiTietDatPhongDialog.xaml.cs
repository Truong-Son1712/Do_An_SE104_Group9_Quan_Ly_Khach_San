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
            public string MaKH        { get; set; } = "";
            public string CMND        { get; set; } = "";
            public string GioiTinh    { get; set; } = "";
            public string NgaySinh    { get; set; } = "";
            public string SDT         { get; set; } = "";
            public string QuocTich    { get; set; } = "";
            public string LoaiKhach   { get; set; } = "";
            public string DiaChi      { get; set; } = "";
            public bool   IsPrimary   { get; set; }
            // Màu header: vàng-cam cho khách đặt chính, xanh cho khách kèm
            public string HeaderColor => IsPrimary ? "#E65100" : "#1565C0";
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

            var rawList = dp.DatPhongKhachHangs.Any()
                ? dp.DatPhongKhachHangs.Select(x => x.KhachHang).Where(k => k != null).Cast<KhachHangModel>().ToList()
                : dp.KhachHang != null ? new List<KhachHangModel> { dp.KhachHang } : new List<KhachHangModel>();

            // Khách đặt chính lên đầu danh sách
            var khachList = rawList
                .OrderByDescending(k => k.MaKH == dp.MaKH)
                .ToList();

            // Tra cứu tên loại khách từ DB theo MaLKH (int)
            var loaiDict = ctx.LoaiKhachHangs.ToDictionary(l => l.MaLKH, l => l.TenLoai);

            int soThuTu = 1;
            // Tải chi tiết thanh toán
            LoadThanhToan(ctx, dp);

            IcKhachHang.ItemsSource = khachList.Select(k =>
            {
                bool isPrimary = k.MaKH == dp.MaKH;
                string tieuDe  = isPrimary
                    ? $"★ Đặt Chính:  {k.HoTen}"
                    : $"Khách {soThuTu++}:  {k.HoTen}";
                if (isPrimary) soThuTu = 2; // khách kèm bắt đầu từ số 2

                return new KhachDisplay
                {
                    TieuDe    = tieuDe,
                    IsPrimary = isPrimary,
                    MaKH      = $"KH{k.MaKH:D4}",
                    CMND      = string.IsNullOrWhiteSpace(k.CMND) ? "—" : k.CMND,
                    GioiTinh  = k.GioiTinh == "Nu" ? "Nữ" : "Nam",
                    NgaySinh  = k.NgaySinh.HasValue ? k.NgaySinh.Value.ToString("dd/MM/yyyy") : "—",
                    SDT       = k.SDT ?? "—",
                    QuocTich  = k.QuocTich,
                    LoaiKhach = loaiDict.TryGetValue(k.MaLoaiKH, out var ten) ? ten : "—",
                    DiaChi    = k.DiaChi ?? "—"
                };
            }).ToList();
        }

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

            // Tiền phòng
            decimal tienPhong = hd?.TienPhong ?? 0;
            TxtTienPhongCT.Text = $"{tienPhong:N0} ₫";

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
                // Chưa có hóa đơn: chỉ hiện dịch vụ, không hiện tổng
                TxtTienPhongCT.Text = "— (chưa lập hóa đơn)";
                TxtConLaiCT.Text    = "—";
                TxtCocCT.Text       = $"{dp.TienCoc:N0} ₫";
                TxtTongCT.Text      = "—";
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

        private void BtnClose_Click(object sender, RoutedEventArgs e) => Close();
        private void Header_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e) => DragMove();
    }
}
