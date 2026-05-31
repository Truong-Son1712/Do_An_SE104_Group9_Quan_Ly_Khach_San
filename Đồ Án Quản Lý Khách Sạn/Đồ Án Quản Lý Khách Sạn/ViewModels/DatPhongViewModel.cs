using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;
using Đồ_Án_Quản_Lý_Khách_Sạn.Data;
using Đồ_Án_Quản_Lý_Khách_Sạn.Helpers;
using Đồ_Án_Quản_Lý_Khách_Sạn.Models;
using Microsoft.EntityFrameworkCore;

namespace Đồ_Án_Quản_Lý_Khách_Sạn.ViewModels
{
    public class DatPhongViewModel : BaseViewModel
    {
        private ObservableCollection<DatPhong> _datPhongs = new();
        private DatPhong? _selected;
        private string _searchText = string.Empty;
        private string _filterTrangThai = "TatCa";
        private DateTime _tuNgay = DateTime.Today.AddDays(-30);
        private DateTime _denNgay = DateTime.Today;

        public ObservableCollection<DatPhong> DatPhongs
        {
            get => _datPhongs;
            set => Set(ref _datPhongs, value);
        }

        public DatPhong? Selected
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

        public bool CanSua       => Selected?.TrangThai == TrangThaiDatPhong.DaDat
                                  || Selected?.TrangThai == TrangThaiDatPhong.DaNhanPhong;
        public bool CanNhanPhong => Selected?.TrangThai == TrangThaiDatPhong.DaDat;
        public bool CanTraPhong  => Selected?.TrangThai == TrangThaiDatPhong.DaNhanPhong;
        public bool CanHuy       => Selected?.TrangThai == TrangThaiDatPhong.DaDat;

        public ICommand RefreshCommand    { get; }
        public ICommand ThemCommand       { get; }
        public ICommand SuaCommand        { get; }
        public ICommand NhanPhongCommand  { get; }
        public ICommand TraPhongCommand   { get; }
        public ICommand HuyCommand        { get; }
        public ICommand XemChiTietCommand { get; }

        public DatPhongViewModel()
        {
            RefreshCommand    = new RelayCommand(_ => LoadData());
            ThemCommand       = new RelayCommand(_ => TaoDatPhong());
            SuaCommand        = new RelayCommand(_ => SuaDatPhong(), _ => CanSua);
            NhanPhongCommand  = new RelayCommand(_ => NhanPhong(),   _ => CanNhanPhong);
            TraPhongCommand   = new RelayCommand(_ => TraPhong(),    _ => CanTraPhong);
            HuyCommand        = new RelayCommand(_ => HuyDatPhong(), _ => CanHuy);
            XemChiTietCommand = new RelayCommand(_ => XemChiTiet(),  _ => Selected != null);
            LoadData();
        }

        public void LoadData()
        {
            try
            {
                using var ctx = new HotelDbContext();
                var tuNgay  = TuNgay.Date;
                var denNgay = DenNgay.Date;

                var q = ctx.DatPhongs
                    .Include(d => d.KhachHang)
                    .Include(d => d.DatPhongKhachHangs).ThenInclude(x => x.KhachHang)
                    .Include(d => d.Phong).ThenInclude(p => p!.LoaiPhong)
                    .Where(d => d.NgayDat.Date >= tuNgay && d.NgayDat.Date <= denNgay)
                    .AsQueryable();

                if (!string.IsNullOrWhiteSpace(SearchText))
                    q = q.Where(d => (d.KhachHang != null && d.KhachHang.HoTen.Contains(SearchText)) ||
                                     (d.Phong != null && d.Phong.SoPhong.Contains(SearchText)) ||
                                     d.DatPhongKhachHangs.Any(x => x.KhachHang != null && x.KhachHang.HoTen.Contains(SearchText)));

                if (FilterTrangThai != "TatCa" && Enum.TryParse<TrangThaiDatPhong>(FilterTrangThai, out var tt))
                    q = q.Where(d => d.TrangThai == tt);

                DatPhongs = new ObservableCollection<DatPhong>(
                    q.ToList().OrderByDescending(d => d.NgayDat));
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void TaoDatPhong()
        {
            var dlg = new Views.DatPhong.DatPhongDialog();
            if (dlg.ShowDialog() == true) LoadData();
        }

        private void SuaDatPhong()
        {
            if (Selected == null) return;
            var dlg = new Views.DatPhong.DatPhongDialog(Selected.MaDatPhong, readOnly: false);
            if (dlg.ShowDialog() == true) LoadData();
        }

        private void NhanPhong()
        {
            if (Selected == null) return;
            if (MessageBox.Show($"Xác nhận nhận phòng cho khách: {Selected.KhachHang?.HoTen}?",
                "Nhận Phòng", MessageBoxButton.YesNo, MessageBoxImage.Question) != MessageBoxResult.Yes) return;

            try
            {
                using var ctx = new HotelDbContext();
                var dp = ctx.DatPhongs.Include(d => d.Phong).FirstOrDefault(d => d.MaDatPhong == Selected.MaDatPhong);
                if (dp == null) return;

                dp.TrangThai = TrangThaiDatPhong.DaNhanPhong;
                dp.NgayNhanPhong = DateTime.Now;
                if (dp.Phong != null) dp.Phong.TrangThai = TrangThaiPhong.DangSuDung;
                ctx.SaveChanges();
                LoadData();
                MessageBox.Show("Nhận phòng thành công!", "Thành Công", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void TraPhong()
        {
            if (Selected == null) return;
            var dlg = new Views.HoaDon.HoaDonDialog(Selected.MaDatPhong);
            if (dlg.ShowDialog() == true) LoadData();
        }

        private void HuyDatPhong()
        {
            if (Selected == null) return;

            decimal tienCoc  = Selected.TienCoc;
            string tenKhach  = Selected.DanhSachKhachText;
            string soPhong   = Selected.Phong?.SoPhong ?? "";

            string confirmMsg =
                $"Xác nhận hủy đặt phòng?\n\n" +
                $"Phòng : {soPhong}\n" +
                $"Khách : {tenKhach}\n" +
                (tienCoc > 0
                    ? $"Tiền cọc: {tienCoc:N0} ₫  →  KHÔNG hoàn lại\n\nTiền cọc sẽ được ghi nhận vào doanh thu."
                    : "Không có tiền cọc.");

            if (MessageBox.Show(confirmMsg, "Hủy Đặt Phòng",
                    MessageBoxButton.YesNo, MessageBoxImage.Warning) != MessageBoxResult.Yes) return;

            try
            {
                using var ctx = new HotelDbContext();
                var dp = ctx.DatPhongs.Include(d => d.Phong)
                                      .FirstOrDefault(d => d.MaDatPhong == Selected.MaDatPhong);
                if (dp == null) return;

                dp.TrangThai = TrangThaiDatPhong.HuyDat;

                // Chỉ reset phòng về TrongSach nếu không còn booking active nào khác
                bool hasOtherActive = ctx.DatPhongs.Any(d =>
                    d.MaPhong == dp.MaPhong &&
                    d.MaDatPhong != dp.MaDatPhong &&
                    (d.TrangThai == TrangThaiDatPhong.DaDat || d.TrangThai == TrangThaiDatPhong.DaNhanPhong));
                if (!hasOtherActive && dp.Phong?.TrangThai == TrangThaiPhong.DaDat)
                    dp.Phong.TrangThai = TrangThaiPhong.TrongSach;

                // Ghi nhận tiền cọc vào doanh thu nếu có
                if (tienCoc > 0)
                {
                    ctx.HoaDons.Add(new Models.HoaDon
                    {
                        MaDatPhong   = dp.MaDatPhong,
                        MaNV         = Helpers.SessionManager.CurrentUser!.MaNV,
                        TienPhong    = tienCoc,
                        TienCoc      = tienCoc,
                        TongTien     = tienCoc,
                        PhuongThucTT = "TienMat",
                        TrangThai    = "DaThanhToan",
                        NgayThanhToan = DateTime.Now,
                        GhiChu       = "Hủy đặt phòng – tiền cọc không hoàn lại"
                    });
                }

                ctx.SaveChanges();
                LoadData();

                string doneMsg = tienCoc > 0
                    ? $"Hủy đặt phòng thành công!\nTiền cọc {tienCoc:N0} ₫ đã được ghi nhận vào doanh thu."
                    : "Hủy đặt phòng thành công!";
                MessageBox.Show(doneMsg, "Thành Công", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void XemChiTiet()
        {
            if (Selected == null) return;
            var dlg = new Views.DatPhong.ChiTietDatPhongDialog(Selected.MaDatPhong);
            dlg.ShowDialog();
        }
    }
}
