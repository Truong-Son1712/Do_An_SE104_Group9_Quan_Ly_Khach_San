using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;
using Đồ_Án_Quản_Lý_Khách_Sạn.Data;
using Đồ_Án_Quản_Lý_Khách_Sạn.Helpers;
using Đồ_Án_Quản_Lý_Khách_Sạn.Models;
using Microsoft.EntityFrameworkCore;

namespace Đồ_Án_Quản_Lý_Khách_Sạn.ViewModels
{
    public class HoaDonViewModel : BaseViewModel
    {
        private ObservableCollection<HoaDon> _hoaDons = new();
        private HoaDon? _selected;
        private string  _searchText      = string.Empty;
        private string  _filterTrangThai = "TatCa";
        private int     _filterMaNV;
        private int     _filterMaPhong;
        private int     _filterMaLoaiPhong;
        private int     _filterSoNgay;
        private int     _filterPhuongThucIndex; // 0=Tất cả, 1=Tiền Mặt, 2=Chuyển Khoản, 3=Thẻ

        // Mặc định: từ ngày đầu tháng hiện tại → hôm nay
        private DateTime _tuNgay  = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
        private DateTime _denNgay = DateTime.Today;

        public ObservableCollection<HoaDon> HoaDons
        {
            get => _hoaDons;
            set => Set(ref _hoaDons, value);
        }

        public HoaDon? Selected
        {
            get => _selected;
            set => Set(ref _selected, value);
        }

        public string SearchText
        {
            get => _searchText;
            set { Set(ref _searchText, value); LoadData(); }
        }

        public string FilterTrangThai
        {
            get => _filterTrangThai;
            set { Set(ref _filterTrangThai, value); LoadData(); }
        }

        public DateTime TuNgay
        {
            get => _tuNgay;
            set { Set(ref _tuNgay, value); LoadData(); }
        }

        public DateTime DenNgay
        {
            get => _denNgay;
            set { Set(ref _denNgay, value); LoadData(); }
        }

        public int FilterMaNV
        {
            get => _filterMaNV;
            set { Set(ref _filterMaNV, value); LoadData(); }
        }

        public int FilterMaPhong
        {
            get => _filterMaPhong;
            set { Set(ref _filterMaPhong, value); LoadData(); }
        }

        public int FilterMaLoaiPhong
        {
            get => _filterMaLoaiPhong;
            set { Set(ref _filterMaLoaiPhong, value); LoadData(); }
        }

        public int FilterSoNgay
        {
            get => _filterSoNgay;
            set { Set(ref _filterSoNgay, value); LoadData(); }
        }

        public int FilterPhuongThucIndex
        {
            get => _filterPhuongThucIndex;
            set { Set(ref _filterPhuongThucIndex, value); LoadData(); }
        }

        public ObservableCollection<KeyValuePair<int, string>> NhanVienOptions  { get; private set; } = new();
        public ObservableCollection<KeyValuePair<int, string>> PhongOptions      { get; private set; } = new();
        public ObservableCollection<KeyValuePair<int, string>> LoaiPhongOptions  { get; private set; } = new();
        public ObservableCollection<KeyValuePair<int, string>> SoNgayOptions     { get; private set; } = new();

        private decimal _tongDoanhThu;
        public decimal TongDoanhThu { get => _tongDoanhThu; set => Set(ref _tongDoanhThu, value); }

        public bool IsQuanLy => SessionManager.IsQuanLy;

        public ICommand RefreshCommand    { get; }
        public ICommand XemChiTietCommand { get; }
        public ICommand ThanhToanCommand  { get; }
        public ICommand HuyHoaDonCommand  { get; }

        public HoaDonViewModel()
        {
            RefreshCommand    = new RelayCommand(_ => Refresh());
            XemChiTietCommand = new RelayCommand(_ => XemChiTiet(), _ => Selected != null);
            ThanhToanCommand  = new RelayCommand(_ => ThanhToan(),  _ => Selected?.TrangThai == "ChuaThanhToan");
            HuyHoaDonCommand  = new RelayCommand(_ => HuyHoaDon(),  _ => Selected != null && IsQuanLy);
            Refresh();
        }

        private void Refresh() { LoadFilterOptions(); LoadData(); }

        private void LoadFilterOptions()
        {
            using var ctx = new HotelDbContext();
            var all = new KeyValuePair<int, string>(0, "Tất cả");

            NhanVienOptions = new ObservableCollection<KeyValuePair<int, string>>(
                new[] { all }.Concat(
                    ctx.NhanViens.OrderBy(n => n.HoTen)
                        .Select(n => new KeyValuePair<int, string>(n.MaNV, n.HoTen))
                        .ToList()));

            PhongOptions = new ObservableCollection<KeyValuePair<int, string>>(
                new[] { all }.Concat(
                    ctx.Phongs.OrderBy(p => p.SoPhong)
                        .Select(p => new KeyValuePair<int, string>(p.MaPhong, p.SoPhong))
                        .ToList()));

            LoaiPhongOptions = new ObservableCollection<KeyValuePair<int, string>>(
                new[] { all }.Concat(
                    ctx.LoaiPhongs.OrderBy(l => l.TenLoaiPhong)
                        .Select(l => new KeyValuePair<int, string>(l.MaLoaiPhong, l.TenLoaiPhong))
                        .ToList()));

            var distinctSoNgay = ctx.DatPhongs.ToList()
                .Select(d => Math.Max(1, (d.NgayTraPhong.Date - d.NgayNhanPhong.Date).Days))
                .Distinct().OrderBy(s => s).ToList();
            SoNgayOptions = new ObservableCollection<KeyValuePair<int, string>>(
                new[] { all }.Concat(
                    distinctSoNgay.Select(s => new KeyValuePair<int, string>(s, $"{s} đêm"))));

            OnPropertyChanged(nameof(NhanVienOptions));
            OnPropertyChanged(nameof(PhongOptions));
            OnPropertyChanged(nameof(LoaiPhongOptions));
            OnPropertyChanged(nameof(SoNgayOptions));
        }

        public void LoadData()
        {
            try
            {
                using var ctx = new HotelDbContext();

                // ── Danh sách hóa đơn trong khoảng ngày ────────────────────
                var q = ctx.HoaDons
                    .Include(h => h.DatPhong).ThenInclude(d => d!.KhachHang)
                    .Include(h => h.DatPhong).ThenInclude(d => d!.Phong).ThenInclude(p => p!.LoaiPhong)
                    .Include(h => h.NhanVien)
                    .Where(h => h.NgayLap.Date >= TuNgay.Date && h.NgayLap.Date <= DenNgay.Date)
                    .AsQueryable();

                if (!string.IsNullOrWhiteSpace(SearchText))
                    q = q.Where(h =>
                        (h.DatPhong != null && h.DatPhong.KhachHang != null &&
                         (h.DatPhong.KhachHang.HoTen.Contains(SearchText) ||
                          h.DatPhong.KhachHang.CMND.Contains(SearchText))) ||
                        (h.DatPhong != null && h.DatPhong.Phong != null &&
                         h.DatPhong.Phong.SoPhong.Contains(SearchText)) ||
                        h.MaHD.ToString().Contains(SearchText));

                if (FilterTrangThai != "TatCa")
                    q = q.Where(h => h.TrangThai == FilterTrangThai);

                if (FilterMaNV > 0)
                    q = q.Where(h => h.MaNV == FilterMaNV);

                if (FilterMaPhong > 0)
                    q = q.Where(h => h.DatPhong != null && h.DatPhong.MaPhong == FilterMaPhong);

                if (FilterMaLoaiPhong > 0)
                    q = q.Where(h => h.DatPhong != null && h.DatPhong.Phong != null &&
                                     h.DatPhong.Phong.MaLoaiPhong == FilterMaLoaiPhong);

                if (FilterPhuongThucIndex > 0)
                {
                    string pt = FilterPhuongThucIndex switch { 1 => "TienMat", 2 => "ChuyenKhoan", _ => "The" };
                    q = q.Where(h => h.PhuongThucTT == pt);
                }

                var list = q.OrderByDescending(h => h.NgayLap).ToList();

                if (FilterSoNgay > 0)
                    list = list.Where(h => h.SoNgay == FilterSoNgay).ToList();

                HoaDons = new ObservableCollection<HoaDon>(list);

                // ── Tổng đã thu = HĐ đã thanh toán + tiền cọc đặt phòng ───
                // Tiền cọc nhận trong khoảng ngày (theo NgayDat) của các
                // booking chưa có HoaDon DaThanhToan → cộng vào doanh thu ngay
                decimal dtHoaDon = list
                    .Where(h => h.TrangThai == "DaThanhToan")
                    .Sum(h => h.TongTien);

                var startDate = TuNgay.Date;
                var endDate   = DenNgay.Date.AddDays(1);

                // Chỉ loại trừ deposit của các booking đã paid TRONG khoảng ngày lọc
                // (tránh mất deposit khi booking trong range nhưng thanh toán ngoài range)
                var paidIdsInRange = ctx.HoaDons
                    .Where(h => h.TrangThai == "DaThanhToan"
                             && h.NgayLap >= startDate
                             && h.NgayLap < endDate)
                    .Select(h => h.MaDatPhong)
                    .ToHashSet();

                decimal dtCoc = ctx.DatPhongs
                    .Where(d => d.NgayDat >= startDate
                             && d.NgayDat < endDate
                             && d.TrangThai != TrangThaiDatPhong.HuyDat)
                    .ToList()
                    .Where(d => d.TienCoc > 0 && !paidIdsInRange.Contains(d.MaDatPhong))
                    .Sum(d => d.TienCoc);

                TongDoanhThu = dtHoaDon + dtCoc;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void XemChiTiet()
        {
            if (Selected == null) return;
            var dlg = new Views.HoaDon.XemHoaDonDialog(Selected.MaHD);
            dlg.ShowDialog();
        }

        private void ThanhToan()
        {
            if (Selected == null) return;
            var dlg = new Views.HoaDon.ThanhToanDialog(Selected.MaHD);
            if (dlg.ShowDialog() == true)
                LoadData();
        }

        private void HuyHoaDon()
        {
            if (Selected == null) return;

            // Dialog xác nhận + nhập lý do (gộp 1 bước)
            bool daThanhToan = Selected.TrangThai == "DaThanhToan";
            var dlgLyDo = new Views.HoaDon.LyDoHuyDialog(daThanhToan);
            if (dlgLyDo.ShowDialog() != true) return;
            string lyDo = dlgLyDo.LyDo;

            try
            {
                using var ctx = new HotelDbContext();
                var hd = ctx.HoaDons
                    .Include(h => h.DatPhong).ThenInclude(d => d!.Phong)
                    .Include(h => h.LichSuDungMaGiams)
                    .FirstOrDefault(h => h.MaHD == Selected.MaHD);
                if (hd == null) return;

                var dp = hd.DatPhong;
                if (dp == null) return;

                // 1. Xóa lịch sử dùng mã giảm giá của hóa đơn này
                ctx.LichSuDungMaGiams.RemoveRange(hd.LichSuDungMaGiams);

                // 2. Phục hồi DatPhong → DaNhanPhong + khôi phục NgayTraPhong dự kiến
                dp.TrangThai    = TrangThaiDatPhong.DaNhanPhong;
                dp.NgayTraPhong = hd.NgayTraPhongGoc ?? dp.NgayTraPhong; // về ngày dự kiến ban đầu
                dp.GhiChu       = $"[Hủy HĐ #{hd.MaHD} - {DateTime.Now:dd/MM/yyyy HH:mm}] {lyDo}";

                // 3. Phục hồi Phòng → DangSuDung
                if (dp.Phong != null)
                    dp.Phong.TrangThai = TrangThaiPhong.DangSuDung;

                // 4. Xóa hóa đơn
                ctx.HoaDons.Remove(hd);

                ctx.SaveChanges();

                MessageBox.Show(
                    $"Đã hủy hóa đơn #{hd.MaHD}.\nPhòng {dp.Phong?.SoPhong} đã trở về trạng thái Đang Nhận Phòng.",
                    "Hủy Thành Công", MessageBoxButton.OK, MessageBoxImage.Information);
                LoadData();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi hủy hóa đơn: {ex.Message}", "Lỗi",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
