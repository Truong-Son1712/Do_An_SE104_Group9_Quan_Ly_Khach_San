using System.Windows;
using System.Windows.Controls;
using Đồ_Án_Quản_Lý_Khách_Sạn.Data;
using Microsoft.EntityFrameworkCore;

namespace Đồ_Án_Quản_Lý_Khách_Sạn.Views.HoaDon
{
    public partial class ThanhToanDialog : Window
    {
        private readonly int _maHD;

        public ThanhToanDialog(int maHD)
        {
            InitializeComponent();
            _maHD = maHD;
            LoadInfo();
        }

        private void LoadInfo()
        {
            using var ctx = new HotelDbContext();
            var hd = ctx.HoaDons
                .Include(h => h.DatPhong).ThenInclude(d => d!.KhachHang)
                .Include(h => h.DatPhong).ThenInclude(d => d!.DatPhongKhachHangs).ThenInclude(x => x.KhachHang)
                .Include(h => h.DatPhong).ThenInclude(d => d!.Phong).ThenInclude(p => p!.LoaiPhong)
                .FirstOrDefault(h => h.MaHD == _maHD);
            if (hd == null) return;

            var dp = hd.DatPhong;
            var allKhach = dp?.DatPhongKhachHangs?.Any() == true
                ? dp.DatPhongKhachHangs.Select(x => x.KhachHang?.HoTen ?? "").Where(s => s.Length > 0).ToList()
                : new List<string> { dp?.KhachHang?.HoTen ?? "" };

            TxtMaHD.Text       = $"Mã hóa đơn: #{hd.MaHD}";
            TxtKhachHang.Text  = string.Join(", ", allKhach);
            TxtPhong.Text      = $"Phòng {dp?.Phong?.SoPhong} – {dp?.Phong?.LoaiPhong?.TenLoaiPhong}";
            TxtTienPhong.Text  = $"{hd.TienPhong:N0} ₫";
            TxtTienCoc.Text    = $"- {hd.TienCoc:N0} ₫";
            TxtConLai.Text     = $"{hd.ConLai:N0} ₫";
            RowTienCoc.Opacity = hd.TienCoc > 0 ? 1.0 : 0.4;

            // Giữ phương thức cũ nếu đã chọn trước đó
            foreach (ComboBoxItem item in CboPhuongThuc.Items)
                if (item.Tag?.ToString() == hd.PhuongThucTT)
                    item.IsSelected = true;
        }

        private void BtnConfirm_Click(object sender, RoutedEventArgs e)
        {
            PnlError.Visibility = Visibility.Collapsed;
            string pt = (CboPhuongThuc.SelectedItem as ComboBoxItem)?.Tag?.ToString() ?? "TienMat";

            try
            {
                using var ctx = new HotelDbContext();
                var hd = ctx.HoaDons.FirstOrDefault(h => h.MaHD == _maHD);
                if (hd == null) return;

                hd.TrangThai     = "DaThanhToan";
                hd.NgayThanhToan = DateTime.Now;
                hd.PhuongThucTT  = pt;
                ctx.SaveChanges();

                DialogResult = true;
                MessageBox.Show("Thanh toán thành công!", "Thành Công",
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                TxtError.Text = ex.Message;
                PnlError.Visibility = Visibility.Visible;
            }
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e) => Close();
        private void Header_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e) => DragMove();
    }
}
