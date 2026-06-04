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
        private decimal _tienDichVu;
        private decimal _tienCoc;
        private decimal _tienGiam;
        private decimal _vatPercent;
        private int? _maGG;
        private int _maLoaiPhong;
        private List<(int MaLoaiDV, decimal ThanhTien)> _dichVuItems = new();

        // Breakdown giá phòng
        private decimal _giaPhongGoc       = 0;
        private decimal _heSoLoaiKhach     = 1;
        private string  _tenLoaiKhachMax   = "";
        private decimal _tiLePhuThuSucChua = 0;

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

            // ── 1. Hiển thị người đặt phòng và khách ở phòng riêng biệt ─────────
            TxtNguoiDat.Text = dp.KhachHang?.HoTen ?? "—";
            var stayingNames = dp.DatPhongKhachHangs
                .Select(x => x.KhachHang?.HoTen ?? "")
                .Where(s => s.Length > 0)
                .ToList();
            TxtKhachO.Text = stayingNames.Any() ? string.Join(", ", stayingNames) : "—";
            TxtPhong.Text  = $"Phòng {dp.Phong?.SoPhong} – {dp.Phong?.LoaiPhong?.TenLoaiPhong}";

            // ── 2. Tính số ngày thuê phòng thực tế (tối thiểu là 1 đêm) ─────────
            var nhan   = dp.NgayNhanPhong;
            var tra    = dp.NgayTraPhong;
            int soNgay = Math.Max(1, (tra.Date - nhan.Date).Days);
            decimal gia = AppConfig.GetGiaPhongHienTai(dp.Phong?.LoaiPhong?.GiaPhong ?? 0);

            // ── 3. Hệ số loại khách: chỉ tính khách thực sự ở phòng (DatPhongKhachHangs)
            // Người đặt phòng không ở → không ảnh hưởng hệ số giá
            var allMaLKHs = dp.DatPhongKhachHangs
                .Select(x => x.KhachHang?.MaLoaiKH ?? 0)
                .Where(id => id > 0)
                .Distinct()
                .ToList();
            // Fallback khi chưa có DatPhongKhachHangs (dữ liệu cũ)
            if (!allMaLKHs.Any() && (dp.KhachHang?.MaLoaiKH ?? 0) > 0)
                allMaLKHs.Add(dp.KhachHang!.MaLoaiKH);
            
            // Lấy hệ số cao nhất trong các loại khách trong phòng (Ví dụ: nội địa=1.0, nước ngoài=1.2 → dùng 1.2)
            decimal heSo    = AppConfig.GetCombinedHeSo(allMaLKHs);
            bool    hasHeSo = heSo > 1m;

            // ── 4. Tính toán phụ thu khi số lượng khách vượt quá sức chứa phòng ───
            int soKhach  = dp.SoKhach > 0 ? dp.SoKhach : stayingNames.Count;
            int sucChua  = dp.Phong?.LoaiPhong?.SucChua ?? int.MaxValue;
            bool hasPhuThu     = soKhach > sucChua;
            decimal tiLePhuThu = hasPhuThu ? AppConfig.GetTiLePhuThu() : 0m;

            // ── 5. Công thức tính tiền phòng ─────────────────────────────────
            _giaPhongGoc       = gia * soNgay;
            _heSoLoaiKhach     = heSo;
            _tiLePhuThuSucChua = tiLePhuThu;
            _tienPhong         = _giaPhongGoc * (heSo + tiLePhuThu);
            _tienCoc           = dp.TienCoc;
            _vatPercent        = AppConfig.GetVAT();

            _maLoaiPhong = dp.Phong?.MaLoaiPhong ?? 0;

            // ── 5b. Tính tiền dịch vụ đã dùng trong phòng ─────────────────
            var dvRaw = ctx.DichVuPhongs.Where(d => d.MaDatPhong == _maDatPhong).ToList();
            _tienDichVu = dvRaw.Sum(d => d.SoLuong * d.DonGia);
            _dichVuItems = dvRaw.Select(d => (d.MaLoaiDV, d.SoLuong * d.DonGia)).ToList();

            // ── 6. Hiển thị thông tin chi tiết lên giao diện người dùng ─────────
            TxtNgay.Text    = $"{nhan:dd/MM/yyyy} → {tra:dd/MM/yyyy}";
            TxtSoNgay.Text  = $"{soNgay} đêm";
            TxtSoKhach.Text = $"{soKhach} khách  (sức chứa: {sucChua})";

            // ── 6. Lấy tên loại khách có hệ số cao nhất ────────────────────
            if (hasHeSo)
            {
                using var hCtx = new HotelDbContext();
                var maxPart = hCtx.LoaiKhachHangs
                    .Where(l => allMaLKHs.Contains(l.MaLKH))
                    .OrderByDescending(l => l.HeSoGia)
                    .Select(l => new { l.TenLoai, l.HeSoGia })
                    .FirstOrDefault();
                _tenLoaiKhachMax = maxPart?.TenLoai ?? "";
            }

            // Hiển thị giá phòng + breakdown trong right column
            TxtSoNgay.Text  = $"{soNgay} đêm";
            TxtGiaPhong.Text = $"{gia:N0} ₫/đêm × {soNgay} đêm = {_giaPhongGoc:N0} ₫";
            if (hasHeSo)
            {
                TxtPhuThuInfo.Text       = $"+ Phụ thu {_tenLoaiKhachMax} (×{heSo:0.####}): +{_giaPhongGoc * (heSo - 1):N0} ₫";
                TxtPhuThuInfo.Visibility = Visibility.Visible;
            }
            if (hasPhuThu)
            {
                TxtPhuThuSucChuaInfo.Text       = $"+ Phụ thu vượt SC ({tiLePhuThu:P0}): +{_giaPhongGoc * tiLePhuThu:N0} ₫";
                TxtPhuThuSucChuaInfo.Visibility = Visibility.Visible;
            }

            TxtTienPhong.Text       = $"{_tienPhong:N0} ₫";
            TxtTienPhongDetail.Text = $"{_tienPhong:N0} ₫";

            // Hiển thị chi tiết từng dịch vụ
            decimal tongTien = _tienPhong + _tienDichVu;
            var dichVus = ctx.DichVuPhongs
                .Include(d => d.LoaiDichVu)
                .Where(d => d.MaDatPhong == _maDatPhong && d.SoLuong > 0)
                .ToList();

            if (dichVus.Any())
            {
                PnlDichVu.Visibility = Visibility.Visible;
                PnlDichVuRows.Children.Clear();
                foreach (var dv in dichVus)
                {
                    var row = new System.Windows.Controls.Grid();
                    row.ColumnDefinitions.Add(new System.Windows.Controls.ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
                    row.ColumnDefinitions.Add(new System.Windows.Controls.ColumnDefinition { Width = GridLength.Auto });
                    row.Margin = new Thickness(0, 0, 0, 4);

                    var lblTen = new System.Windows.Controls.TextBlock
                    {
                        Text = $"  • {dv.LoaiDichVu?.TenLoaiDV} × {dv.SoLuong} {dv.LoaiDichVu?.DonViTinh}",
                        FontSize = 12,
                        Foreground = System.Windows.Media.Brushes.DimGray
                    };
                    System.Windows.Controls.Grid.SetColumn(lblTen, 0);

                    var lblGia = new System.Windows.Controls.TextBlock
                    {
                        Text = $"{dv.SoLuong * dv.DonGia:N0} ₫",
                        FontSize = 12,
                        Foreground = new System.Windows.Media.SolidColorBrush(
                            (System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#F57C00")),
                        FontWeight = FontWeights.SemiBold
                    };
                    System.Windows.Controls.Grid.SetColumn(lblGia, 1);

                    row.Children.Add(lblTen);
                    row.Children.Add(lblGia);
                    PnlDichVuRows.Children.Add(row);
                }
                TxtTienDichVu.Text = $"{_tienDichVu:N0} ₫";
            }

            TxtTienCocDisplay.Text = $"{_tienCoc:N0} ₫";
            RowTienCoc.Opacity     = _tienCoc > 0 ? 1.0 : 0.4;
            ApplyRoomPriceBreakdown();
            UpdateTotals();
        }

        private void ApplyRoomPriceBreakdown()
        {
            bool hasLoai = _heSoLoaiKhach > 1m;
            bool hasSC   = _tiLePhuThuSucChua > 0m;

            TxtGiaPhongGoc.Text = $"{_giaPhongGoc:N0} ₫";

            if (hasLoai)
            {
                RowPhuThuLoai.Visibility    = Visibility.Visible;
                TxtLabelPhuThuLoai.Text     = $"+ Phụ thu {_tenLoaiKhachMax} (×{_heSoLoaiKhach:0.####}):";
                TxtPhuThuLoai.Text          = $"+{_giaPhongGoc * (_heSoLoaiKhach - 1):N0} ₫";
            }
            if (hasSC)
            {
                RowPhuThuSC.Visibility      = Visibility.Visible;
                TxtLabelPhuThuSC.Text       = $"+ Phụ thu vượt sức chứa ({_tiLePhuThuSucChua:P0}):";
                TxtPhuThuSC.Text            = $"+{_giaPhongGoc * _tiLePhuThuSucChua:N0} ₫";
            }
            if (hasLoai || hasSC)
            {
                RowTienPhongTotal.Visibility = Visibility.Visible;
                TxtTienPhongDetail.Text      = $"{_tienPhong:N0} ₫";
            }
            else
            {
                // Không có phụ thu → ẩn dòng tổng riêng, dùng TxtGiaPhongGoc làm dòng duy nhất
                TxtLabelGiaGoc.Text          = "Tiền phòng:";
                RowTienPhongTotal.Visibility  = Visibility.Collapsed;
                TxtTienPhongDetail.Text       = $"{_tienPhong:N0} ₫"; // giữ để UpdateTotals dùng
            }
        }

        private void UpdateTotals()
        {
            decimal tamTinh       = _tienPhong + _tienDichVu - _tienGiam;
            decimal tienVAT       = Math.Round(tamTinh * _vatPercent / 100, 0);
            decimal tongSauVAT    = tamTinh + tienVAT;
            decimal conPhai       = Math.Max(0, tongSauVAT - _tienCoc);

            // Giảm giá
            if (_tienGiam > 0)
            {
                RowTienGiam.Visibility  = Visibility.Visible;
                TxtTenMaGiam.Text       = $"Giảm giá ({TxtMaGiamGia.Text.Trim()}):";
                TxtTienGiamDisplay.Text = $"- {_tienGiam:N0} ₫";
            }
            else
            {
                RowTienGiam.Visibility = Visibility.Collapsed;
            }

            // VAT
            RowVAT.Visibility    = Visibility.Visible;
            TxtVATLabel.Text     = $"Thuế VAT ({_vatPercent:0.##}%):";
            TxtTienVAT.Text      = $"{tienVAT:N0} ₫";

            TxtThanhToanTraPhong.Text = $"{conPhai:N0} ₫";
            TxtConLai.Text            = $"{tongSauVAT:N0} ₫";
        }

        private void BtnKiemTraMa_Click(object sender, RoutedEventArgs e)
        {
            string tenMa = TxtMaGiamGia.Text.Trim().ToUpper();
            TxtKetQuaMa.Visibility = Visibility.Visible;

            if (string.IsNullOrEmpty(tenMa))
            {
                TxtKetQuaMa.Text       = "Vui lòng nhập mã giảm giá.";
                TxtKetQuaMa.Foreground = System.Windows.Media.Brushes.OrangeRed;
                return;
            }

            using var ctx = new HotelDbContext();
            var mg = ctx.MaGiamGias
                .Include(m => m.ChiTiets)
                .FirstOrDefault(m => m.TenMa == tenMa);

            if (mg == null)
            {
                TxtKetQuaMa.Text       = "❌  Mã giảm giá không tồn tại.";
                TxtKetQuaMa.Foreground = System.Windows.Media.Brushes.Crimson;
                return;
            }
            if (mg.TrangThai == "Inactive")
            {
                TxtKetQuaMa.Text       = "❌  Mã giảm giá đã bị vô hiệu hóa.";
                TxtKetQuaMa.Foreground = System.Windows.Media.Brushes.Crimson;
                return;
            }

            // Kiểm tra số lượt
            if (mg.SoLuongToiDa.HasValue)
            {
                int daUsed = ctx.LichSuDungMaGiams.Count(l => l.MaGG == mg.MaGG);
                if (daUsed >= mg.SoLuongToiDa.Value)
                {
                    TxtKetQuaMa.Text       = $"❌  Mã đã hết lượt sử dụng ({daUsed}/{mg.SoLuongToiDa}).";
                    TxtKetQuaMa.Foreground = System.Windows.Media.Brushes.Crimson;
                    return;
                }
            }
            var now = DateTime.Now;
            if (now < mg.NgayBatDau)
            {
                TxtKetQuaMa.Text       = $"⏳  Mã chưa có hiệu lực (bắt đầu từ {mg.NgayBatDau:dd/MM/yyyy}).";
                TxtKetQuaMa.Foreground = System.Windows.Media.Brushes.OrangeRed;
                return;
            }
            if (now > mg.NgayKetThuc)
            {
                TxtKetQuaMa.Text       = $"❌  Mã đã hết hạn (kết thúc {mg.NgayKetThuc:dd/MM/yyyy}).";
                TxtKetQuaMa.Foreground = System.Windows.Media.Brushes.Crimson;
                return;
            }

            // Tính tiền giảm
            decimal giamPhong = 0;
            var ctPhong = mg.ChiTiets.FirstOrDefault(c => c.LoaiApDung == "LoaiPhong" && c.MaLoai == _maLoaiPhong);
            if (ctPhong != null)
                giamPhong = Math.Round(_tienPhong * ctPhong.TiLeGiam / 100, 0);

            decimal giamDV = 0;
            foreach (var (maLoaiDV, thanhTien) in _dichVuItems)
            {
                var ctDV = mg.ChiTiets.FirstOrDefault(c => c.LoaiApDung == "LoaiDichVu" && c.MaLoai == maLoaiDV);
                if (ctDV != null)
                    giamDV += Math.Round(thanhTien * ctDV.TiLeGiam / 100, 0);
            }

            _tienGiam = giamPhong + giamDV;
            _maGG     = mg.MaGG;

            if (_tienGiam == 0)
            {
                TxtKetQuaMa.Text       = "ℹ️  Mã hợp lệ nhưng không có quy tắc giảm giá nào áp dụng cho đặt phòng này.";
                TxtKetQuaMa.Foreground = System.Windows.Media.Brushes.OrangeRed;
                _maGG = null;
            }
            else
            {
                var parts = new List<string>();
                if (ctPhong != null) parts.Add($"phòng -{ctPhong.TiLeGiam:0.##}%");
                foreach (var (maLoaiDV, _) in _dichVuItems)
                {
                    var ctDV = mg.ChiTiets.FirstOrDefault(c => c.LoaiApDung == "LoaiDichVu" && c.MaLoai == maLoaiDV);
                    if (ctDV != null)
                    {
                        string tenDV = ctx.LoaiDichVus.Find(maLoaiDV)?.TenLoaiDV ?? "";
                        parts.Add($"{tenDV} -{ctDV.TiLeGiam:0.##}%");
                    }
                }
                TxtKetQuaMa.Text       = $"✅  Mã hợp lệ! Giảm: {string.Join(", ", parts)}. Tiết kiệm: {_tienGiam:N0} ₫";
                TxtKetQuaMa.Foreground = new System.Windows.Media.SolidColorBrush(
                    (System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#2E7D32"));
                BtnBoMa.Visibility = Visibility.Visible;
            }

            UpdateTotals();
        }

        private void BtnBoMa_Click(object sender, RoutedEventArgs e)
        {
            _tienGiam              = 0;
            _maGG                  = null;
            TxtMaGiamGia.Text      = "";
            TxtKetQuaMa.Visibility = Visibility.Collapsed;
            BtnBoMa.Visibility     = Visibility.Collapsed;
            UpdateTotals();
        }

        private void BtnConfirm_Click(object sender, RoutedEventArgs e)
        {
            PnlError.Visibility = Visibility.Collapsed;

            try
            {
                using var ctx = new HotelDbContext();
                var dp = ctx.DatPhongs.Include(d => d.Phong).FirstOrDefault(d => d.MaDatPhong == _maDatPhong);
                if (dp == null) return;

                decimal tamTinh    = _tienPhong + _tienDichVu - _tienGiam;
                decimal tienVAT    = Math.Round(tamTinh * _vatPercent / 100, 0);
                decimal tongSauVAT = tamTinh + tienVAT;

                var hd = new Models.HoaDon
                {
                    MaDatPhong         = _maDatPhong,
                    MaNV               = SessionManager.CurrentUser!.MaNV,
                    TienPhong          = _tienPhong,
                    TienCoc            = _tienCoc,
                    TongTien           = tongSauVAT,
                    TienGiam           = _tienGiam,
                    TienVAT            = tienVAT,
                    VATPercent         = _vatPercent,
                    MaGG               = _maGG,
                    NgayTraPhongGoc    = dp.NgayTraPhong,
                    PhuongThucTT       = "TienMat",
                    TrangThai          = "ChuaThanhToan",
                    GhiChu             = TxtGhiChu.Text.Trim(),
                    GiaPhongGoc        = _giaPhongGoc,
                    HeSoLoaiKhach      = _heSoLoaiKhach,
                    TenLoaiKhachMax    = _tenLoaiKhachMax,
                    TiLePhuThuSucChua  = _tiLePhuThuSucChua
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

                // Ghi lịch sử dùng mã giảm giá
                if (_maGG.HasValue && _tienGiam > 0)
                {
                    ctx.LichSuDungMaGiams.Add(new Models.LichSuDungMaGiam
                    {
                        MaGG       = _maGG.Value,
                        MaHD       = hd.MaHD,
                        NgaySuDung = DateTime.Now
                    });
                    ctx.SaveChanges();
                }

                DialogResult = true;
                decimal conPhai  = Math.Max(0, tongSauVAT - _tienCoc);
                string giamInfo  = _tienGiam > 0 ? $"\nĐã giảm: {_tienGiam:N0} ₫" : "";
                string vatInfo   = tienVAT > 0   ? $"\nThuế VAT ({_vatPercent:0.##}%): {tienVAT:N0} ₫" : "";
                MessageBox.Show(
                    $"Trả phòng thành công!\nTiền phòng: {_tienPhong:N0} ₫{giamInfo}{vatInfo}\nTiền cọc: {_tienCoc:N0} ₫\n" +
                    $"Còn phải thu: {conPhai:N0} ₫\n\nVui lòng vào tab Hóa Đơn để hoàn tất thanh toán.",
                    "Trả Phòng Thành Công", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex) { ShowError(ex.Message); }
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e) => Close();
        private void ShowError(string msg) { TxtError.Text = msg; PnlError.Visibility = Visibility.Visible; }
        private void Header_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e) => DragMove();
    }
}
