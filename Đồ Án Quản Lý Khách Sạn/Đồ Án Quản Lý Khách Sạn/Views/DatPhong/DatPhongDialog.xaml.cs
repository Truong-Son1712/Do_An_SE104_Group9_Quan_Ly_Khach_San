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
            public int    MaKH      { get; set; }
            public string HoTen     { get; set; } = "";
            public string LoaiKhach { get; set; } = "";
            public string LoaiText  => LoaiKhach == "NuocNgoai" ? "Nước ngoài" : "Nội địa";
            public bool   IsNuocNgoai => LoaiKhach == "NuocNgoai";

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

            // Lấy danh sách MaKH đang có trong booking hoạt động
            var busyIds = ctx.DatPhongKhachHangs
                .Where(x => activeStatuses.Contains(x.DatPhong!.TrangThai))
                .Select(x => x.MaKH)
                .ToHashSet();

            // Khi chỉnh sửa: giữ lại các khách thuộc chính booking này
            if (_maDatPhong.HasValue)
            {
                var thisIds = ctx.DatPhongKhachHangs
                    .Where(x => x.MaDatPhong == _maDatPhong.Value)
                    .Select(x => x.MaKH)
                    .ToHashSet();
                busyIds.ExceptWith(thisIds);
            }

            _allKhachHangItems = ctx.KhachHangs.OrderBy(k => k.HoTen)
                .Select(k => new KhachHangItem { MaKH = k.MaKH, HoTen = k.HoTen, LoaiKhach = k.LoaiKhach ?? "" })
                .ToList()
                .Where(k => !busyIds.Contains(k.MaKH))
                .ToList();
            LstKhachHang.ItemsSource = _allKhachHangItems;

            // Chỉ hiện phòng đang trống sạch (1 phòng = 1 đặt phòng tại 1 thời điểm)
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

            using var ctx2 = new HotelDbContext();
            var phongs = ctx2.Phongs.Include(p => p.LoaiPhong).OrderBy(p => p.SoPhong).ToList();
            CboPhong.ItemsSource  = phongs;
            CboPhong.SelectedItem = phongs.FirstOrDefault(p => p.MaPhong == dp.MaPhong);
            CboPhong.IsEnabled    = false; // phòng không đổi khi chỉnh sửa

            if (!_readOnly)
            {
                BtnSave.Content = "💾  Lưu Thay Đổi";
            }
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
        }

        // ── Price estimate: GiaPhong × SoNgay × HeSo ──────────────────────
        private void UpdateDuTinh()
        {
            if (!_initialized) return;

            TxtDuTinh.Text = "—";

            if (CboPhong.SelectedItem is not Models.Phong p || p.LoaiPhong == null) return;
            if (!DpNhan.SelectedDate.HasValue || !DpTra.SelectedDate.HasValue) return;
            if (DpTra.SelectedDate.Value.Date <= DpNhan.SelectedDate.Value.Date) return;

            int soNgay = Math.Max(1, (DpTra.SelectedDate.Value.Date - DpNhan.SelectedDate.Value.Date).Days);
            _giaPhong  = p.LoaiPhong.GiaPhong;

            bool isNuocNgoai = _allKhachHangItems.Any(k => k.IsChecked && k.IsNuocNgoai);
            decimal heSo     = isNuocNgoai ? AppConfig.GetHeSoNuocNgoai() : 1m;
            decimal total    = _giaPhong * soNgay * heSo;

            string heSoText = isNuocNgoai ? $" ×{heSo:0.##}" : "";
            TxtDuTinh.Text  = $"{total:N0} ₫  ({soNgay} đêm{heSoText})";
        }

        // ── Save ───────────────────────────────────────────────────────────
        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            PnlError.Visibility = Visibility.Collapsed;

            var selectedKhach = _allKhachHangItems.Where(k => k.IsChecked).ToList();
            if (!selectedKhach.Any())
            { ShowError("Vui lòng chọn ít nhất một khách hàng."); return; }

            // Khi chỉnh sửa, phòng đã cố định — bỏ qua validation phòng
            if (!_maDatPhong.HasValue && CboPhong.SelectedItem is not Models.Phong)
            { ShowError("Vui lòng chọn phòng."); return; }
            var phong = CboPhong.SelectedItem as Models.Phong;
            if (!DpNhan.SelectedDate.HasValue || !DpTra.SelectedDate.HasValue)
            { ShowError("Vui lòng chọn ngày nhận và trả phòng."); return; }
            if (DpTra.SelectedDate.Value.Date <= DpNhan.SelectedDate.Value.Date)
            { ShowError("Ngày trả phòng phải sau ngày nhận phòng."); return; }

            // Kiểm tra sức chứa phòng
            if (phong?.LoaiPhong != null && selectedKhach.Count > phong.LoaiPhong.SucChua)
            { ShowError($"Phòng này chỉ chứa tối đa {phong.LoaiPhong.SucChua} khách. Bạn đã chọn {selectedKhach.Count} người."); return; }

            decimal.TryParse(TxtTienCoc.Text.Replace(",", ""), out decimal tienCoc);

            try
            {
                using var ctx = new HotelDbContext();

                if (_maDatPhong.HasValue)
                {
                    // ── UPDATE ────────────────────────────────────────────
                    var dp = ctx.DatPhongs.FirstOrDefault(d => d.MaDatPhong == _maDatPhong.Value);
                    if (dp == null) return;

                    dp.MaKH          = selectedKhach.First().MaKH;
                    dp.NgayNhanPhong = DpNhan.SelectedDate.Value.Date;
                    dp.NgayTraPhong  = DpTra.SelectedDate.Value.Date;
                    dp.TienCoc       = tienCoc;
                    dp.SoKhach       = selectedKhach.Count;
                    dp.GhiChu        = TxtGhiChu.Text.Trim();

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
                        MaKH          = selectedKhach.First().MaKH,
                        MaPhong       = phong.MaPhong,
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
