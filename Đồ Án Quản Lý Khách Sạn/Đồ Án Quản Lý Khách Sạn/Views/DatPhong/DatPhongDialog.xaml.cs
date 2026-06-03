using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using Đồ_Án_Quản_Lý_Khách_Sạn.Data;
using Đồ_Án_Quản_Lý_Khách_Sạn.Helpers;
using Đồ_Án_Quản_Lý_Khách_Sạn.Models;
using Microsoft.EntityFrameworkCore;

namespace Đồ_Án_Quản_Lý_Khách_Sạn.Views.DatPhong
{
    public partial class DatPhongDialog : Window
    {
        private readonly int? _maDatPhong;
        private readonly bool _readOnly;
        private decimal _giaPhong;
        private bool _initialized;

        private List<KhachHangItem> _allKhachHangItems = new();

        // ── Inner class ────────────────────────────────────────────────────
        private class KhachHangItem : INotifyPropertyChanged
        {
            private bool _isChecked;
            public int    MaKH         { get; set; }
            public string HoTen        { get; set; } = "";
            public int    MaLoaiKH     { get; set; }   // FK int → LoaiKhachHangs.MaLKH
            public string TenLoaiKhach { get; set; } = ""; // tên hiển thị
            public string LoaiText     => TenLoaiKhach;

            public bool IsChecked
            {
                get => _isChecked;
                set { _isChecked = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsChecked))); }
            }
            public event PropertyChangedEventHandler? PropertyChanged;
        }

        // ── Constructor ────────────────────────────────────────────────────
        public DatPhongDialog(int? maDatPhong = null, bool readOnly = false)
        {
            InitializeComponent();
            _maDatPhong = maDatPhong;
            _readOnly   = readOnly;
            LoadComboData();

            DpNhan.SelectedDate = DateTime.Today;
            DpTra.SelectedDate  = DateTime.Today.AddDays(1);
            _initialized = true;

            if (maDatPhong.HasValue) LoadData(maDatPhong.Value);
            if (readOnly) SetReadOnly();
        }

        // ── Data loading ───────────────────────────────────────────────────
        private void LoadComboData()
        {
            using var ctx = new HotelDbContext();

            var activeStatuses = new[] { TrangThaiDatPhong.DaDat, TrangThaiDatPhong.DaNhanPhong };

            var busyIds = ctx.DatPhongKhachHangs
                .Where(x => activeStatuses.Contains(x.DatPhong!.TrangThai))
                .Select(x => x.MaKH)
                .ToHashSet();

            if (_maDatPhong.HasValue)
            {
                var thisIds = ctx.DatPhongKhachHangs
                    .Where(x => x.MaDatPhong == _maDatPhong.Value)
                    .Select(x => x.MaKH)
                    .ToHashSet();
                busyIds.ExceptWith(thisIds);
            }

            // Load TenLoai theo MaLKH (int FK)
            var loaiDict = ctx.LoaiKhachHangs
                .ToDictionary(l => l.MaLKH, l => l.TenLoai);

            _allKhachHangItems = ctx.KhachHangs.OrderBy(k => k.HoTen)
                .Select(k => new { k.MaKH, k.HoTen, k.MaLoaiKH })
                .ToList()
                .Where(k => !busyIds.Contains(k.MaKH))
                .Select(k => new KhachHangItem
                {
                    MaKH         = k.MaKH,
                    HoTen        = k.HoTen,
                    MaLoaiKH     = k.MaLoaiKH,
                    TenLoaiKhach = loaiDict.TryGetValue(k.MaLoaiKH, out var t) ? t : k.MaLoaiKH.ToString()
                })
                .ToList();
            LstKhachHang.ItemsSource = _allKhachHangItems;

            CboPhong.ItemsSource = ctx.Phongs
                .Include(p => p.LoaiPhong)
                .Where(p => p.TrangThai == TrangThaiPhong.TrongSach)
                .OrderBy(p => p.SoPhong).ToList();
        }

        private void LoadData(int id)
        {
            using var ctx = new HotelDbContext();
            var dp = ctx.DatPhongs
                .Include(d => d.KhachHang)
                .Include(d => d.Phong).ThenInclude(p => p!.LoaiPhong)
                .Include(d => d.DatPhongKhachHangs)
                .FirstOrDefault(d => d.MaDatPhong == id);
            if (dp == null) return;

            TxtTitle.Text = _readOnly ? "Chi Tiết Đặt Phòng" : "Chỉnh Sửa Đặt Phòng";
            DpNhan.SelectedDate = dp.NgayNhanPhong;
            DpTra.SelectedDate  = dp.NgayTraPhong;
            TxtTienCoc.Text     = dp.TienCoc.ToString("N0");
            TxtGhiChu.Text      = dp.GhiChu;

            var khachIds = dp.DatPhongKhachHangs.Select(x => x.MaKH).ToHashSet();
            if (!khachIds.Any() && dp.MaKH > 0) khachIds.Add(dp.MaKH);

            foreach (var item in _allKhachHangItems)
                item.IsChecked = khachIds.Contains(item.MaKH);

            UpdateKhachSummary();

            // Chọn đúng khách đặt chính (MaKH của DatPhong)
            var primary = _allKhachHangItems.FirstOrDefault(k => k.MaKH == dp.MaKH);
            if (primary != null) CboKhachChinh.SelectedItem = primary;

            using var ctx2 = new HotelDbContext();
            var phongs = ctx2.Phongs.Include(p => p.LoaiPhong).OrderBy(p => p.SoPhong).ToList();
            CboPhong.ItemsSource  = phongs;
            CboPhong.SelectedItem = phongs.FirstOrDefault(p => p.MaPhong == dp.MaPhong);
            CboPhong.IsEnabled    = false;

            if (!_readOnly) BtnSave.Content = "💾  Lưu Thay Đổi";
        }

        private void SetReadOnly()
        {
            TxtTitle.Text          = "Chi Tiết Đặt Phòng";
            BtnSave.Visibility     = Visibility.Collapsed;
            CboPhong.IsEnabled     = false;
            DpNhan.IsEnabled       = DpTra.IsEnabled = false;
            TxtTienCoc.IsReadOnly  = TxtGhiChu.IsReadOnly = true;
            TxtTimKiem.IsReadOnly  = true;
            LstKhachHang.IsEnabled = false;
        }

        // ── Event handlers ─────────────────────────────────────────────────
        private void TxtTimKiem_Changed(object sender, TextChangedEventArgs e)
        {
            var filter = TxtTimKiem.Text.Trim().ToLower();
            LstKhachHang.ItemsSource = string.IsNullOrEmpty(filter)
                ? _allKhachHangItems
                : _allKhachHangItems.Where(k => k.HoTen.ToLower().Contains(filter)).ToList();
        }

        private void KhachHang_CheckChanged(object sender, RoutedEventArgs e)
        {
            if (!_initialized) return;
            UpdateKhachSummary();
            UpdateDuTinh();
        }

        private void CboPhong_SelectionChanged(object sender, SelectionChangedEventArgs e) => UpdateDuTinh();
        private void DpNhan_Changed(object sender, SelectionChangedEventArgs e)             => UpdateDuTinh();
        private void DpTra_Changed(object sender, SelectionChangedEventArgs e)              => UpdateDuTinh();

        // ── Summary ────────────────────────────────────────────────────────
        private void UpdateKhachSummary()
        {
            var selected = _allKhachHangItems.Where(k => k.IsChecked).ToList();

            TxtKhachDaChon.Text = selected.Count == 0
                ? "Chưa chọn khách hàng"
                : string.Join(", ", selected.Select(k => k.HoTen));

            // Lưu khách đang được chọn làm đặt chính (nếu có)
            var currentPrimary = CboKhachChinh.SelectedValue as int? ?? (CboKhachChinh.SelectedItem as KhachHangItem)?.MaKH;

            CboKhachChinh.ItemsSource = selected;

            // Giữ lại khách đặt chính nếu vẫn còn trong danh sách; không thì chọn người đầu
            var restore = selected.FirstOrDefault(k => k.MaKH == currentPrimary)
                          ?? selected.FirstOrDefault();
            CboKhachChinh.SelectedItem = restore;
        }

        // ── Tính tiền dự tính: hệ số cao nhất theo loại khách + phụ thu sức chứa
        private void UpdateDuTinh()
        {
            if (!_initialized) return;

            TxtDuTinh.Text           = "—";
            PnlNuocNgoai.Visibility  = Visibility.Collapsed;
            PnlPhuThu.Visibility     = Visibility.Collapsed;

            if (CboPhong.SelectedItem is not Models.Phong p || p.LoaiPhong == null) return;
            if (!DpNhan.SelectedDate.HasValue || !DpTra.SelectedDate.HasValue) return;
            if (DpTra.SelectedDate.Value.Date <= DpNhan.SelectedDate.Value.Date) return;

            int soNgay  = Math.Max(1, (DpTra.SelectedDate.Value.Date - DpNhan.SelectedDate.Value.Date).Days);
            _giaPhong   = p.LoaiPhong.GiaPhong;
            int soKhach = _allKhachHangItems.Count(k => k.IsChecked);

            // Hệ số: nhân tất cả hệ số của các loại khách khác nhau được chọn (dùng MaLoaiKH int)
            var selectedItems  = _allKhachHangItems.Where(k => k.IsChecked).ToList();
            var distinctMaLKHs = selectedItems.Select(k => k.MaLoaiKH).Distinct().ToList();
            decimal heSo  = AppConfig.GetCombinedHeSo(distinctMaLKHs);
            bool    hasHeSo = heSo > 1m;

            bool    hasPhuThu   = soKhach > 0 && soKhach > p.LoaiPhong.SucChua;
            decimal tiLePhuThu  = hasPhuThu ? AppConfig.GetTiLePhuThu() : 0m;

            decimal total = _giaPhong * soNgay * heSo * (1m + tiLePhuThu);
            TxtDuTinh.Text = $"{total:N0} ₫";

            if (hasHeSo)
            {
                var breakdown = GetHeSoBreakdown(distinctMaLKHs);
                TxtNuocNgoai.Text = $"ℹ  Hệ số giá: {breakdown}  →  ×{heSo:0.####}" +
                                    $"  (giá gốc: {_giaPhong * soNgay:N0} ₫ / {soNgay} đêm).";
                PnlNuocNgoai.Visibility = Visibility.Visible;
            }

            if (hasPhuThu)
            {
                TxtPhuThu.Text = $"⚠  Số khách ({soKhach}) vượt sức chứa phòng ({p.LoaiPhong.SucChua} người). " +
                                 $"Áp dụng phụ thu {tiLePhuThu:P0} trên tổng tiền phòng.";
                PnlPhuThu.Visibility = Visibility.Visible;
            }
        }

        private static string GetHeSoBreakdown(List<int> maLKHs)
        {
            using var ctx = new HotelDbContext();
            var items = ctx.LoaiKhachHangs
                .Where(l => maLKHs.Contains(l.MaLKH) && l.HeSoGia > 1m)
                .Select(l => new { l.TenLoai, l.HeSoGia })
                .ToList();
            return items.Any()
                ? string.Join(" × ", items.Select(i => $"{i.TenLoai}(×{i.HeSoGia:0.####})"))
                : "×1";
        }

        // ── Save ───────────────────────────────────────────────────────────
        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            PnlError.Visibility = Visibility.Collapsed;

            var selectedKhach = _allKhachHangItems.Where(k => k.IsChecked).ToList();
            if (!selectedKhach.Any())
            { ShowError("Vui lòng chọn ít nhất một khách hàng."); return; }

            // Khách đặt chính do user chọn; fallback về người đầu nếu chưa chọn
            var khachChinh = CboKhachChinh.SelectedItem as KhachHangItem
                             ?? selectedKhach.First();
            if (!selectedKhach.Contains(khachChinh))
                khachChinh = selectedKhach.First();

            if (!_maDatPhong.HasValue && CboPhong.SelectedItem is not Models.Phong)
            { ShowError("Vui lòng chọn phòng."); return; }
            var phong = CboPhong.SelectedItem as Models.Phong;

            if (!DpNhan.SelectedDate.HasValue || !DpTra.SelectedDate.HasValue)
            { ShowError("Vui lòng chọn ngày nhận và trả phòng."); return; }
            if (DpTra.SelectedDate.Value.Date <= DpNhan.SelectedDate.Value.Date)
            { ShowError("Ngày trả phòng phải sau ngày nhận phòng."); return; }

            // Kiểm tra vượt sức chứa tối đa – cấm đặt phòng
            int sucChuaToiDa = AppConfig.GetSucChuaToiDa();
            if (selectedKhach.Count > sucChuaToiDa)
            {
                ShowError($"Không thể đặt phòng. Số khách ({selectedKhach.Count}) vượt quá " +
                          $"sức chứa tối đa cho phép ({sucChuaToiDa} người).");
                return;
            }

            // Nếu số khách > sức chứa phòng → cho phép nhưng sẽ áp phụ thu (UpdateDuTinh đã hiển thị)

            decimal.TryParse(TxtTienCoc.Text.Replace(",", ""), out decimal tienCoc);

            try
            {
                using var ctx = new HotelDbContext();

                if (_maDatPhong.HasValue)
                {
                    // ── UPDATE ────────────────────────────────────────────
                    var dp = ctx.DatPhongs.FirstOrDefault(d => d.MaDatPhong == _maDatPhong.Value);
                    if (dp == null) return;

                    dp.MaKH          = khachChinh.MaKH;
                    dp.NgayNhanPhong = DpNhan.SelectedDate.Value.Date;
                    dp.NgayTraPhong  = DpTra.SelectedDate.Value.Date;
                    dp.TienCoc       = tienCoc;
                    dp.SoKhach       = selectedKhach.Count;
                    dp.GhiChu        = TxtGhiChu.Text.Trim();

                    // Đồng bộ HoaDon.TienCoc nếu hóa đơn đã tồn tại (tránh lệch dữ liệu)
                    var hd = ctx.HoaDons.FirstOrDefault(h => h.MaDatPhong == dp.MaDatPhong);
                    if (hd != null) hd.TienCoc = tienCoc;

                    var old = ctx.DatPhongKhachHangs.Where(x => x.MaDatPhong == _maDatPhong.Value).ToList();
                    ctx.DatPhongKhachHangs.RemoveRange(old);
                    foreach (var k in selectedKhach)
                        ctx.DatPhongKhachHangs.Add(new Models.DatPhongKhachHang { MaDatPhong = dp.MaDatPhong, MaKH = k.MaKH });
                }
                else
                {
                    // ── INSERT ────────────────────────────────────────────
                    var dp = new Models.DatPhong
                    {
                        MaKH          = khachChinh.MaKH,
                        MaPhong       = phong!.MaPhong,
                        NgayNhanPhong = DpNhan.SelectedDate.Value.Date,
                        NgayTraPhong  = DpTra.SelectedDate.Value.Date,
                        TienCoc       = tienCoc,
                        SoKhach       = selectedKhach.Count,
                        GhiChu        = TxtGhiChu.Text.Trim(),
                        TrangThai     = TrangThaiDatPhong.DaDat
                    };
                    ctx.DatPhongs.Add(dp);
                    ctx.SaveChanges();

                    foreach (var k in selectedKhach)
                        ctx.DatPhongKhachHangs.Add(new Models.DatPhongKhachHang { MaDatPhong = dp.MaDatPhong, MaKH = k.MaKH });

                    var roomEntity = ctx.Phongs.Find(phong.MaPhong);
                    if (roomEntity != null) roomEntity.TrangThai = TrangThaiPhong.DaDat;
                }

                ctx.SaveChanges();
                DialogResult = true;
            }
            catch (Exception ex) { ShowError(ex.Message); }
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e) => Close();
        private void ShowError(string msg) { TxtError.Text = msg; PnlError.Visibility = Visibility.Visible; }
        private void Header_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e) => DragMove();
    }
}
