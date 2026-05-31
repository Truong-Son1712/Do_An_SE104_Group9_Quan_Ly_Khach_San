using System.Windows;
using Đồ_Án_Quản_Lý_Khách_Sạn.Data;
using Đồ_Án_Quản_Lý_Khách_Sạn.Helpers;
using Đồ_Án_Quản_Lý_Khách_Sạn.Models;
using Microsoft.EntityFrameworkCore;

namespace Đồ_Án_Quản_Lý_Khách_Sạn.Views.HoaDon
{
    public partial class HoaDonDialog : Window
    {
        private readonly int _maDatPhong;
        private decimal _tienPhong;
        private decimal _tienCoc;

        public HoaDonDialog(int maDatPhong)
        {
            InitializeComponent();
            _maDatPhong = maDatPhong;
            LoadInfo();
        }

        private void LoadInfo()
        {
            using var ctx = new HotelDbContext();
            var dp = ctx.DatPhongs
                .Include(d => d.KhachHang)
                .Include(d => d.DatPhongKhachHangs).ThenInclude(x => x.KhachHang)
                .Include(d => d.Phong).ThenInclude(p => p!.LoaiPhong)
                .FirstOrDefault(d => d.MaDatPhong == _maDatPhong);
            if (dp == null) return;

            // Hiển thị toàn bộ danh sách khách
            var allKhach = dp.DatPhongKhachHangs.Any()
                ? dp.DatPhongKhachHangs.Select(x => x.KhachHang?.HoTen ?? "").Where(s => s.Length > 0).ToList()
                : new List<string> { dp.KhachHang?.HoTen ?? "" };
            TxtKhachHang.Text = string.Join(", ", allKhach);
            TxtPhong.Text = $"Phòng {dp.Phong?.SoPhong} – {dp.Phong?.LoaiPhong?.TenLoaiPhong}";

            var nhan    = dp.NgayNhanPhong;
            var tra     = dp.NgayTraPhong;
            int soNgay  = Math.Max(1, (tra.Date - nhan.Date).Days);
            decimal gia = dp.Phong?.LoaiPhong?.GiaPhong ?? 0;

            // HeSo áp dụng nếu có bất kỳ khách nước ngoài nào trong nhóm
            bool isNuocNgoai = dp.DatPhongKhachHangs.Any(x => x.KhachHang?.LoaiKhach == "NuocNgoai")
                               || dp.KhachHang?.LoaiKhach == "NuocNgoai";
            decimal heSo = isNuocNgoai ? AppConfig.GetHeSoNuocNgoai() : 1m;

            _tienPhong = gia * soNgay * heSo;
            _tienCoc   = dp.TienCoc;

            TxtNgay.Text    = $"{nhan:dd/MM/yyyy} → {tra:dd/MM/yyyy}";
            TxtSoNgay.Text  = $"{soNgay} đêm";

            string heSoText = isNuocNgoai ? $" ×{heSo:0.##}" : "";
            TxtGiaPhong.Text  = isNuocNgoai
                ? $"{gia:N0} ₫/đêm{heSoText} = {gia * heSo:N0} ₫/đêm"
                : $"{gia:N0} ₫/đêm";
            TxtTienPhong.Text = $"{_tienPhong:N0} ₫";

            decimal thanhToanTraPhong = Math.Max(0, _tienPhong - _tienCoc);
            TxtTienCocDisplay.Text    = $"{_tienCoc:N0} ₫";
            TxtThanhToanTraPhong.Text = $"{thanhToanTraPhong:N0} ₫";
            RowTienCoc.Opacity        = _tienCoc > 0 ? 1.0 : 0.4;
            TxtConLai.Text            = $"{_tienPhong:N0} ₫";
        }

        private void BtnConfirm_Click(object sender, RoutedEventArgs e)
        {
            PnlError.Visibility = Visibility.Collapsed;

            try
            {
                using var ctx = new HotelDbContext();
                var dp = ctx.DatPhongs.Include(d => d.Phong).FirstOrDefault(d => d.MaDatPhong == _maDatPhong);
                if (dp == null) return;

                var hd = new Models.HoaDon
                {
                    MaDatPhong   = _maDatPhong,
                    MaNV         = SessionManager.CurrentUser!.MaNV,
                    TienPhong    = _tienPhong,
                    TienCoc      = _tienCoc,
                    TongTien     = _tienPhong,
                    PhuongThucTT = "TienMat",   // sẽ cập nhật khi thanh toán thực tế
                    TrangThai    = "ChuaThanhToan",
                    GhiChu       = TxtGhiChu.Text.Trim()
                };
                ctx.HoaDons.Add(hd);

                dp.TrangThai    = TrangThaiDatPhong.DaTraPhong;
                dp.NgayTraPhong = DateTime.Now;

                // Chỉ chuyển phòng sang CanDonDep khi không còn booking nào đang hoạt động
                if (dp.Phong != null)
                {
                    bool hasOtherActive = ctx.DatPhongs.Any(d =>
                        d.MaPhong == dp.MaPhong
                        && d.MaDatPhong != dp.MaDatPhong
                        && (d.TrangThai == TrangThaiDatPhong.DaDat || d.TrangThai == TrangThaiDatPhong.DaNhanPhong));

                    dp.Phong.TrangThai = hasOtherActive
                        ? TrangThaiPhong.DangSuDung
                        : TrangThaiPhong.CanDonDep;
                }

                ctx.SaveChanges();
                DialogResult = true;
                MessageBox.Show(
                    $"Trả phòng thành công!\nTiền phòng: {_tienPhong:N0} ₫\nTiền cọc: {_tienCoc:N0} ₫\nCòn phải thu: {Math.Max(0, _tienPhong - _tienCoc):N0} ₫\n\nVui lòng vào tab Hóa Đơn để hoàn tất thanh toán.",
                    "Trả Phòng Thành Công", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex) { ShowError(ex.Message); }
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e) => Close();
        private void ShowError(string msg) { TxtError.Text = msg; PnlError.Visibility = Visibility.Visible; }
        private void Header_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e) => DragMove();
    }
}
