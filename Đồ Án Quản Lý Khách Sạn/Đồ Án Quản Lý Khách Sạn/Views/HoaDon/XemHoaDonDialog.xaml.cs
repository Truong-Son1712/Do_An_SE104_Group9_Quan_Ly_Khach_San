using System.Windows;
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
            var allKhach = dp?.DatPhongKhachHangs.Any() == true
                ? dp.DatPhongKhachHangs.Select(x => x.KhachHang?.HoTen ?? "").Where(s => s.Length > 0).ToList()
                : new List<string> { dp?.KhachHang?.HoTen ?? "" };

            TxtTitle.Text    = $"Hóa Đơn #{hd.MaHD}";
            TxtKhach.Text    = string.Join(", ", allKhach);
            TxtPhong.Text    = $"Phòng {dp?.Phong?.SoPhong} – {dp?.Phong?.LoaiPhong?.TenLoaiPhong}";
            TxtNgayNhan.Text = dp?.NgayNhanPhong.ToString("dd/MM/yyyy HH:mm:ss") ?? "—";
            TxtNgayTra.Text  = dp?.NgayTraPhong.ToString("dd/MM/yyyy HH:mm:ss") ?? "—";
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

            decimal thanhToanTraPhong = Math.Max(0, hd.TienPhong - hd.TienCoc);
            TxtTienCoc.Text           = $"{hd.TienCoc:N0} ₫";
            TxtThanhToanTraPhong.Text = $"{thanhToanTraPhong:N0} ₫";
            RowCoc.Opacity            = hd.TienCoc > 0 ? 1.0 : 0.4;
            TxtTong.Text              = $"{hd.TongTien:N0} ₫";
        }

        private void BtnClose_Click(object sender, RoutedEventArgs e) => Close();
        private void Header_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e) => DragMove();

    }
}
