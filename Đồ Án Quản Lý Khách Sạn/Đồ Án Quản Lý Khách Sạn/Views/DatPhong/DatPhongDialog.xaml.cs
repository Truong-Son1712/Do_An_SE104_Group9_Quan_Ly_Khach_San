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

        private class KhachHangItem : INotifyPropertyChanged
        {
            private bool _isChecked;
            public int    MaKH         { get; set; }
            public string HoTen        { get; set; } = "";
            public string CMND         { get; set; } = "";
            public int    MaLoaiKH     { get; set; }
            public string TenLoaiKhach { get; set; } = "";
            public string LoaiText     => TenLoaiKhach;

            // Đã đặt/đang ở phòng khác → không được chọn làm khách ở
            public bool   IsOccupied   { get; set; } = false;
            public string OccupiedInfo { get; set; } = "";
            public bool   CanStay      => !IsOccupied;
            public string OccupiedText => IsOccupied ? $"  🔒 {OccupiedInfo}" : "";

            public bool IsChecked
            {
                get => _isChecked;
                set { _isChecked = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsChecked))); }
            }
            public event PropertyChangedEventHandler? PropertyChanged;
            public override string ToString() => HoTen;
        }

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

        private void LoadComboData()
        {
            using var ctx = new HotelDbContext();

            var loaiDict = ctx.LoaiKhachHangs
                .ToDictionary(l => l.MaLKH, l => l.TenLoai);

            // Khách đã đặt hoặc đang ở phòng khác (DaDat/DaNhanPhong), trừ booking đang chỉnh sửa
            var occupiedMap = ctx.DatPhongKhachHangs
                .Include(x => x.DatPhong).ThenInclude(d => d!.Phong)
                .Where(x => (x.DatPhong!.TrangThai == TrangThaiDatPhong.DaDat
                          || x.DatPhong!.TrangThai == TrangThaiDatPhong.DaNhanPhong)
                         && (_maDatPhong == null || x.DatPhong.MaDatPhong != _maDatPhong.Value))
                .AsEnumerable()
                .GroupBy(x => x.MaKH)
                .ToDictionary(
                    g => g.Key,
                    g => string.Join(", ", g.Select(x =>
                        x.DatPhong!.TrangThai == TrangThaiDatPhong.DaNhanPhong
                            ? $"Đang ở phòng {x.DatPhong.Phong?.SoPhong}"
                            : $"Đã đặt phòng {x.DatPhong.Phong?.SoPhong}")));

            _allKhachHangItems = ctx.KhachHangs.OrderBy(k => k.HoTen)
                .Select(k => new { k.MaKH, k.HoTen, k.CMND, k.MaLoaiKH })
                .ToList()
                .Select(k => new KhachHangItem
                {
                    MaKH         = k.MaKH,
                    HoTen        = k.HoTen,
                    CMND         = k.CMND,
                    MaLoaiKH     = k.MaLoaiKH,
                    TenLoaiKhach = loaiDict.TryGetValue(k.MaLoaiKH, out var t) ? t : k.MaLoaiKH.ToString(),
                    IsOccupied   = occupiedMap.ContainsKey(k.MaKH),
                    OccupiedInfo = occupiedMap.TryGetValue(k.MaKH, out var info) ? info : ""
                })
                .ToList();

            // Người đặt phòng: tất cả khách hàng
            CboNguoiDat.ItemsSource  = _allKhachHangItems;
            // Khách ở phòng: tất cả khách hàng (kể cả người đặt nếu họ muốn ở)
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

            TxtTitle.Text       = _readOnly ? "Chi Tiết Đặt Phòng" : "Chỉnh Sửa Đặt Phòng";
            DpNhan.SelectedDate = dp.NgayNhanPhong;
            DpTra.SelectedDate  = dp.NgayTraPhong;
            TxtTienCoc.Text     = dp.TienCoc.ToString("N0");
            TxtGhiChu.Text      = dp.GhiChu;

            // Khôi phục người đặt phòng
            var nguoiDat = _allKhachHangItems.FirstOrDefault(k => k.MaKH == dp.MaKH);
            if (nguoiDat != null)
                CboNguoiDat.SelectedItem = nguoiDat;

            // DatPhongKhachHangs = khách thực sự ở phòng (có thể bao gồm người đặt nếu họ cũng ở)
            var stayingIds = dp.DatPhongKhachHangs.Select(x => x.MaKH).ToHashSet();
            if (!stayingIds.Any() && dp.MaKH > 0) stayingIds.Add(dp.MaKH); // backward compat

            foreach (var item in _allKhachHangItems)
                item.IsChecked = stayingIds.Contains(item.MaKH);

            UpdateKhachSummary();

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
            CboNguoiDat.IsEnabled  = false;
            CboPhong.IsEnabled     = false;
            DpNhan.IsEnabled       = DpTra.IsEnabled = false;
            TxtTienCoc.IsReadOnly  = TxtGhiChu.IsReadOnly = true;
            TxtTimKiem.IsReadOnly  = true;
            LstKhachHang.IsEnabled = false;
        }

        // ── Events ─────────────────────────────────────────────────────────
        private void CboNguoiDat_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!_initialized) return;
            UpdateKhachSummary();
            UpdateDuTinh();
        }

        private void TxtTimKiem_Changed(object sender, TextChangedEventArgs e)
        {
            var filter = TxtTimKiem.Text.Trim().ToLower();
            LstKhachHang.ItemsSource = string.IsNullOrEmpty(filter)
                ? _allKhachHangItems
                : _allKhachHangItems.Where(k =>
                    k.HoTen.ToLower().Contains(filter) ||
                    k.CMND.ToLower().Contains(filter)).ToList();
        }

        private void CboNguoiDat_KeyUp(object sender, System.Windows.Input.KeyEventArgs e)
        {
            // Bỏ qua các phím điều hướng để không làm mất selection
            if (e.Key is System.Windows.Input.Key.Down or System.Windows.Input.Key.Up
                      or System.Windows.Input.Key.Enter or System.Windows.Input.Key.Escape) return;

            string text = CboNguoiDat.Text.Trim().ToLower();
            CboNguoiDat.ItemsSource = string.IsNullOrEmpty(text)
                ? _allKhachHangItems
                : _allKhachHangItems.Where(k =>
                    k.HoTen.ToLower().Contains(text) ||
                    k.CMND.ToLower().Contains(text)).ToList();
            CboNguoiDat.IsDropDownOpen = true;
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
            var nguoiDat   = CboNguoiDat.SelectedItem as KhachHangItem;
            var khachO     = _allKhachHangItems.Where(k => k.IsChecked).ToList();

            var parts = new List<string>();
            if (nguoiDat != null) parts.Add($"{nguoiDat.HoTen} (ĐP)");
            parts.AddRange(khachO.Select(k => k.HoTen));

            TxtKhachDaChon.Text = parts.Any()
                ? string.Join(", ", parts)
                : "Chưa chọn khách ở phòng";
        }

        // ── Tính dự tính: chỉ dùng khách được tick trong LstKhachHang ─────
        private void UpdateDuTinh()
        {
            if (!_initialized) return;

            TxtDuTinh.Text          = "—";
            PnlNuocNgoai.Visibility = Visibility.Collapsed;
            PnlPhuThu.Visibility    = Visibility.Collapsed;

            if (CboPhong.SelectedItem is not Models.Phong p || p.LoaiPhong == null) return;
            if (!DpNhan.SelectedDate.HasValue || !DpTra.SelectedDate.HasValue) return;
            if (DpTra.SelectedDate.Value.Date <= DpNhan.SelectedDate.Value.Date) return;

            int soNgay = Math.Max(1, (DpTra.SelectedDate.Value.Date - DpNhan.SelectedDate.Value.Date).Days);
            _giaPhong  = AppConfig.GetGiaPhongHienTai(p.LoaiPhong.GiaPhong);

            // Chỉ tính dựa trên khách thực sự ở phòng (những người được tick)
            var stayingItems   = _allKhachHangItems.Where(k => k.IsChecked).ToList();
            int soKhach        = stayingItems.Count;
            var distinctMaLKHs = stayingItems.Select(k => k.MaLoaiKH).Distinct().ToList();

            decimal heSo    = distinctMaLKHs.Any() ? AppConfig.GetCombinedHeSo(distinctMaLKHs) : 1m;
            bool    hasHeSo = heSo > 1m;

            bool    hasPhuThu  = soKhach > 0 && soKhach > p.LoaiPhong.SucChua;
            decimal tiLePhuThu = hasPhuThu ? AppConfig.GetTiLePhuThu() : 0m;

            decimal total = _giaPhong * soNgay * (heSo + tiLePhuThu);
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
                TxtPhuThu.Text = $"⚠  Số khách ở phòng ({soKhach}) vượt sức chứa ({p.LoaiPhong.SucChua} người). " +
                                 $"Phụ thu {tiLePhuThu:P0} tính trên giá phòng gốc.";
                PnlPhuThu.Visibility = Visibility.Visible;
            }
        }

        private static string GetHeSoBreakdown(List<int> maLKHs)
        {
            using var ctx = new HotelDbContext();
            var maxItem = ctx.LoaiKhachHangs
                .Where(l => maLKHs.Contains(l.MaLKH))
                .OrderByDescending(l => l.HeSoGia)
                .Select(l => new { l.TenLoai, l.HeSoGia })
                .FirstOrDefault();
            return maxItem != null && maxItem.HeSoGia > 1m
                ? $"{maxItem.TenLoai}(×{maxItem.HeSoGia:0.####})"
                : "×1";
        }

        // ── Save ───────────────────────────────────────────────────────────
        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            PnlError.Visibility = Visibility.Collapsed;

            var nguoiDat   = CboNguoiDat.SelectedItem as KhachHangItem;
            if (nguoiDat == null)
            { ShowError("Vui lòng chọn người đặt phòng."); return; }

            // Khách ở phòng: chỉ những người được tick — người đặt KHÔNG tự động thêm vào
            var stayingKhach = _allKhachHangItems.Where(k => k.IsChecked).ToList();

            // Kiểm tra: không ai trong danh sách đã đặt/đang ở phòng khác
            var conflict = stayingKhach.FirstOrDefault(k => k.IsOccupied);
            if (conflict != null)
            { ShowError($"Khách '{conflict.HoTen}' {conflict.OccupiedInfo}. Không thể thêm vào phòng này."); return; }

            if (!_maDatPhong.HasValue && CboPhong.SelectedItem is not Models.Phong)
            { ShowError("Vui lòng chọn phòng."); return; }
            var phong = CboPhong.SelectedItem as Models.Phong;

            if (!DpNhan.SelectedDate.HasValue || !DpTra.SelectedDate.HasValue)
            { ShowError("Vui lòng chọn ngày nhận và trả phòng."); return; }
            if (DpTra.SelectedDate.Value.Date <= DpNhan.SelectedDate.Value.Date)
            { ShowError("Ngày trả phòng phải sau ngày nhận phòng."); return; }

            int sucChuaToiDa = AppConfig.GetSucChuaToiDa();
            if (stayingKhach.Count > sucChuaToiDa)
            {
                ShowError($"Không thể đặt phòng. Số khách ở phòng ({stayingKhach.Count}) vượt quá " +
                          $"sức chứa tối đa cho phép ({sucChuaToiDa} người).");
                return;
            }

            decimal.TryParse(TxtTienCoc.Text.Replace(",", ""), out decimal tienCoc);

            // NguoiDatPhongOPhong: true nếu người đặt cũng nằm trong danh sách khách ở
            bool nguoiDatOPhong = stayingKhach.Any(k => k.MaKH == nguoiDat.MaKH);

            try
            {
                using var ctx = new HotelDbContext();

                if (_maDatPhong.HasValue)
                {
                    var dp = ctx.DatPhongs.FirstOrDefault(d => d.MaDatPhong == _maDatPhong.Value);
                    if (dp == null) return;

                    dp.MaKH                = nguoiDat.MaKH;
                    dp.NguoiDatPhongOPhong = nguoiDatOPhong;
                    dp.NgayNhanPhong       = DpNhan.SelectedDate.Value.Date;
                    dp.NgayTraPhong        = DpTra.SelectedDate.Value.Date;
                    dp.TienCoc             = tienCoc;
                    dp.SoKhach             = stayingKhach.Count;
                    dp.GhiChu              = TxtGhiChu.Text.Trim();

                    var hd = ctx.HoaDons.FirstOrDefault(h => h.MaDatPhong == dp.MaDatPhong);
                    if (hd != null) hd.TienCoc = tienCoc;

                    var old = ctx.DatPhongKhachHangs.Where(x => x.MaDatPhong == _maDatPhong.Value).ToList();
                    ctx.DatPhongKhachHangs.RemoveRange(old);
                    foreach (var k in stayingKhach)
                        ctx.DatPhongKhachHangs.Add(new Models.DatPhongKhachHang { MaDatPhong = dp.MaDatPhong, MaKH = k.MaKH });
                }
                else
                {
                    var dp = new Models.DatPhong
                    {
                        MaKH                = nguoiDat.MaKH,
                        NguoiDatPhongOPhong = nguoiDatOPhong,
                        MaPhong             = phong!.MaPhong,
                        NgayNhanPhong       = DpNhan.SelectedDate.Value.Date,
                        NgayTraPhong        = DpTra.SelectedDate.Value.Date,
                        TienCoc             = tienCoc,
                        SoKhach             = stayingKhach.Count,
                        GhiChu              = TxtGhiChu.Text.Trim(),
                        TrangThai           = TrangThaiDatPhong.DaDat
                    };
                    ctx.DatPhongs.Add(dp);
                    ctx.SaveChanges();

                    foreach (var k in stayingKhach)
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
