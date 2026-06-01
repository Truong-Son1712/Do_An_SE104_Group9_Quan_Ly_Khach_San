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

            // ── Thông tin khách ─────────────────────────────────────────────
            var allKhach = dp.DatPhongKhachHangs.Any()
                ? dp.DatPhongKhachHangs.Select(x => x.KhachHang?.HoTen ?? "").Where(s => s.Length > 0).ToList()
                : new List<string> { dp.KhachHang?.HoTen ?? "" };
            TxtKhachHang.Text = string.Join(", ", allKhach);
            TxtPhong.Text     = $"Phòng {dp.Phong?.SoPhong} – {dp.Phong?.LoaiPhong?.TenLoaiPhong}";

            var nhan   = dp.NgayNhanPhong;
            var tra    = dp.NgayTraPhong;
            int soNgay = Math.Max(1, (tra.Date - nhan.Date).Days);
            decimal gia = dp.Phong?.LoaiPhong?.GiaPhong ?? 0;

            // ── Nhân tất cả hệ số của các loại khách khác nhau trong booking ─
            var allCodes = dp.DatPhongKhachHangs
                .Select(x => x.KhachHang?.LoaiKhach ?? "")
                .Append(dp.KhachHang?.LoaiKhach ?? "")
                .Where(c => !string.IsNullOrEmpty(c))
                .Distinct()
                .ToList();
            decimal heSo    = AppConfig.GetCombinedHeSo(allCodes);
            bool    hasHeSo = heSo > 1m;

            // ── Phụ thu khi số khách vượt sức chứa phòng ───────────────────
            int soKhach  = dp.SoKhach > 0 ? dp.SoKhach : allKhach.Count;
            int sucChua  = dp.Phong?.LoaiPhong?.SucChua ?? int.MaxValue;
            bool hasPhuThu    = soKhach > sucChua;
            decimal tiLePhuThu = hasPhuThu ? AppConfig.GetTiLePhuThu() : 0m;
            decimal phuThuMul  = 1m + tiLePhuThu;

            _tienPhong = gia * soNgay * heSo * phuThuMul;
            _tienCoc   = dp.TienCoc;

            // ── Hiển thị thông tin ──────────────────────────────────────────
            TxtNgay.Text    = $"{nhan:dd/MM/yyyy} → {tra:dd/MM/yyyy}";
            TxtSoNgay.Text  = $"{soNgay} đêm";
            TxtSoKhach.Text = $"{soKhach} khách  (sức chứa: {sucChua})";

            // Tạo chuỗi mô tả hệ số (vd: "Nước ngoài(×1.2) × Khách du lịch(×1.3) = ×1.56")
            string heSoText = "";
            if (hasHeSo)
            {
                using var hCtx = new HotelDbContext();
                var parts = hCtx.LoaiKhachHangs
                    .Where(l => allCodes.Contains(l.MaCode) && l.HeSoGia > 1m)
                    .Select(l => new { l.TenLoai, l.HeSoGia })
                    .ToList();
                heSoText = parts.Any()
                    ? " × " + string.Join(" × ", parts.Select(p => $"{p.TenLoai}(×{p.HeSoGia:0.####})"))
                    + $" = ×{heSo:0.####}"
                    : $" ×{heSo:0.####}";
            }
            TxtGiaPhong.Text = hasHeSo
                ? $"{gia:N0} ₫/đêm{heSoText}"
                : $"{gia:N0} ₫/đêm";

            if (hasPhuThu)
            {
                TxtPhuThuInfo.Text       = $"Phụ thu {tiLePhuThu:P0} (vượt sức chứa {sucChua} người)";
                TxtPhuThuInfo.Visibility = Visibility.Visible;
            }

            TxtTienPhong.Text = $"{_tienPhong:N0} ₫";

            decimal thanhToanTraPhong     = Math.Max(0, _tienPhong - _tienCoc);
            TxtTienCocDisplay.Text        = $"{_tienCoc:N0} ₫";
            TxtThanhToanTraPhong.Text     = $"{thanhToanTraPhong:N0} ₫";
            RowTienCoc.Opacity            = _tienCoc > 0 ? 1.0 : 0.4;
            TxtConLai.Text                = $"{_tienPhong:N0} ₫";
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
                    PhuongThucTT = "TienMat",
                    TrangThai    = "ChuaThanhToan",
                    GhiChu       = TxtGhiChu.Text.Trim()
                };
                ctx.HoaDons.Add(hd);

                dp.TrangThai    = TrangThaiDatPhong.DaTraPhong;
                dp.NgayTraPhong = DateTime.Now;

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
                    $"Trả phòng thành công!\nTiền phòng: {_tienPhong:N0} ₫\nTiền cọc: {_tienCoc:N0} ₫\n" +
                    $"Còn phải thu: {Math.Max(0, _tienPhong - _tienCoc):N0} ₫\n\n" +
                    $"Vui lòng vào tab Hóa Đơn để hoàn tất thanh toán.",
                    "Trả Phòng Thành Công", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex) { ShowError(ex.Message); }
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e) => Close();
        private void ShowError(string msg) { TxtError.Text = msg; PnlError.Visibility = Visibility.Visible; }
        private void Header_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e) => DragMove();
    }
}
