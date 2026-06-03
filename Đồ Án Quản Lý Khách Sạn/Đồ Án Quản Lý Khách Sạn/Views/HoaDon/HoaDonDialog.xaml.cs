using System.Windows;
using Đồ_Án_Quản_Lý_Khách_Sạn.Data;
using Đồ_Án_Quản_Lý_Khách_Sạn.Helpers;
using Đồ_Án_Quản_Lý_Khách_Sạn.Models;
using Microsoft.EntityFrameworkCore;

namespace Đồ_Án_Quản_Lý_Khách_Sạn.Views.HoaDon
{
    /// <summary>
    /// Hộp thoại xử lý checkout phòng và lập hóa đơn thanh toán tạm tính.
    /// Thực hiện các nghiệp vụ tính phụ thu sức chứa (25% cho khách vượt định mức)
    /// và tính hệ số loại khách (khách nước ngoài).
    /// </summary>
    public partial class HoaDonDialog : Window
    {
        private readonly int _maDatPhong;
        private decimal _tienPhong;
        private decimal _tienCoc;

        /// <summary>
        /// Khởi tạo màn hình lập hóa đơn dựa trên mã đặt phòng được chọn.
        /// </summary>
        /// <param name="maDatPhong">Mã đặt phòng cần thực hiện checkout</param>
        public HoaDonDialog(int maDatPhong)
        {
            InitializeComponent();
            _maDatPhong = maDatPhong;
            LoadInfo();
        }

        /// <summary>
        /// Nạp thông tin đặt phòng, tính toán tiền thuê phòng, phụ thu sức chứa và hệ số khách hàng.
        /// </summary>
        private void LoadInfo()
        {
            using var ctx = new HotelDbContext();
            var dp = ctx.DatPhongs
                .Include(d => d.KhachHang)
                .Include(d => d.DatPhongKhachHangs).ThenInclude(x => x.KhachHang)
                .Include(d => d.Phong).ThenInclude(p => p!.LoaiPhong)
                .FirstOrDefault(d => d.MaDatPhong == _maDatPhong);
            if (dp == null) return;

            // ── 1. Gom danh sách khách hàng thuê phòng ───────────────────────────
            var allKhach = dp.DatPhongKhachHangs.Any()
                ? dp.DatPhongKhachHangs.Select(x => x.KhachHang?.HoTen ?? "").Where(s => s.Length > 0).ToList()
                : new List<string> { dp.KhachHang?.HoTen ?? "" };
            TxtKhachHang.Text = string.Join(", ", allKhach);
            TxtPhong.Text     = $"Phòng {dp.Phong?.SoPhong} – {dp.Phong?.LoaiPhong?.TenLoaiPhong}";

            // ── 2. Tính số ngày thuê phòng thực tế (tối thiểu là 1 đêm) ─────────
            var nhan   = dp.NgayNhanPhong;
            var tra    = dp.NgayTraPhong;
            int soNgay = Math.Max(1, (tra.Date - nhan.Date).Days);
            decimal gia = dp.Phong?.LoaiPhong?.GiaPhong ?? 0;

            // ── 3. Lấy hệ số loại khách hàng (Tính theo quy định có khách nước ngoài) ──
            // Lấy danh sách các mã loại khách hàng không trùng lặp trong phòng
            var allMaLKHs = dp.DatPhongKhachHangs
                .Select(x => x.KhachHang?.MaLoaiKH ?? 0)
                .Append(dp.KhachHang?.MaLoaiKH ?? 0)
                .Where(id => id > 0)
                .Distinct()
                .ToList();
            
            // Hàm nhân tất cả các hệ số loại khách khác nhau trong phòng (Ví dụ: Nội địa = 1.0, Nước ngoài = 1.2)
            decimal heSo    = AppConfig.GetCombinedHeSo(allMaLKHs);
            bool    hasHeSo = heSo > 1m;

            // ── 4. Tính toán phụ thu khi số lượng khách vượt quá sức chứa phòng ───
            int soKhach  = dp.SoKhach > 0 ? dp.SoKhach : allKhach.Count;
            int sucChua  = dp.Phong?.LoaiPhong?.SucChua ?? int.MaxValue;
            bool hasPhuThu    = soKhach > sucChua;
            decimal tiLePhuThu = hasPhuThu ? AppConfig.GetTiLePhuThu() : 0m;
            decimal phuThuMul  = 1m + tiLePhuThu;

            // ── 5. Công thức tính tiền phòng chung cuộc ────────────────────────
            // Tiền phòng = (Giá phòng gốc * Số ngày) * Hệ số khách * (1 + Tỉ lệ phụ thu)
            _tienPhong = gia * soNgay * heSo * phuThuMul;
            _tienCoc   = dp.TienCoc;

            // ── 6. Hiển thị thông tin chi tiết lên giao diện người dùng ─────────
            TxtNgay.Text    = $"{nhan:dd/MM/yyyy} → {tra:dd/MM/yyyy}";
            TxtSoNgay.Text  = $"{soNgay} đêm";
            TxtSoKhach.Text = $"{soKhach} khách  (sức chứa: {sucChua})";

            // Hiển thị chi tiết hệ số giá nếu có phụ thu loại khách nước ngoài
            string heSoText = "";
            if (hasHeSo)
            {
                using var hCtx = new HotelDbContext();
                var parts = hCtx.LoaiKhachHangs
                    .Where(l => allMaLKHs.Contains(l.MaLKH) && l.HeSoGia > 1m)
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

            // Hiển thị thông tin phụ thu vượt sức chứa phòng (25%) nếu có
            if (hasPhuThu)
            {
                TxtPhuThuInfo.Text       = $"Phụ thu {tiLePhuThu:P0} (vượt sức chứa {sucChua} người)";
                TxtPhuThuInfo.Visibility = Visibility.Visible;
            }

            TxtTienPhong.Text = $"{_tienPhong:N0} ₫";

            // Trừ đi số tiền khách đã đặt cọc trước
            decimal thanhToanTraPhong     = Math.Max(0, _tienPhong - _tienCoc);
            TxtTienCocDisplay.Text        = $"{_tienCoc:N0} ₫";
            TxtThanhToanTraPhong.Text     = $"{thanhToanTraPhong:N0} ₫";
            RowTienCoc.Opacity            = _tienCoc > 0 ? 1.0 : 0.4;
            TxtConLai.Text                = $"{_tienPhong:N0} ₫";
        }

        /// <summary>
        /// Xử lý sự kiện khi nhân viên xác nhận hoàn tất thủ tục trả phòng (Lập hóa đơn chưa thanh toán).
        /// </summary>
        private void BtnConfirm_Click(object sender, RoutedEventArgs e)
        {
            PnlError.Visibility = Visibility.Collapsed;

            try
            {
                using var ctx = new HotelDbContext();
                var dp = ctx.DatPhongs.Include(d => d.Phong).FirstOrDefault(d => d.MaDatPhong == _maDatPhong);
                if (dp == null) return;

                // Tạo đối tượng hóa đơn lưu trữ thông tin tiền phòng, tiền cọc của đợt đặt phòng này
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

                // Cập nhật trạng thái đặt phòng thành đã trả phòng
                dp.TrangThai    = TrangThaiDatPhong.DaTraPhong;
                dp.NgayTraPhong = DateTime.Now;

                // Giải phóng phòng hoặc chuyển trạng thái dọn dẹp
                if (dp.Phong != null)
                {
                    // Kiểm tra xem phòng này có bất kỳ đặt phòng active nào kế tiếp hay không
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
