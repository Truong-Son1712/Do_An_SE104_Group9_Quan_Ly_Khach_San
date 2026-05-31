using System.Windows.Input;
using Đồ_Án_Quản_Lý_Khách_Sạn.Data;
using Đồ_Án_Quản_Lý_Khách_Sạn.Helpers;
using Đồ_Án_Quản_Lý_Khách_Sạn.Models;

namespace Đồ_Án_Quản_Lý_Khách_Sạn.ViewModels
{
    public class DashboardViewModel : BaseViewModel
    {
        private int _tongPhong, _phongTrong, _phongDangSuDung, _phongBaoDuong;
        private int _datPhongHomNay, _traPhongHomNay, _tongKhachHang;
        private decimal _doanhThuThang;
        private string _thangNam = string.Empty;
        private string _capNhatLuc = string.Empty;

        public int TongPhong         { get => _tongPhong;        set => Set(ref _tongPhong, value); }
        public int PhongTrong         { get => _phongTrong;       set => Set(ref _phongTrong, value); }
        public int PhongDangSuDung    { get => _phongDangSuDung;  set => Set(ref _phongDangSuDung, value); }
        public int PhongBaoDuong      { get => _phongBaoDuong;    set => Set(ref _phongBaoDuong, value); }
        public int DatPhongHomNay     { get => _datPhongHomNay;   set => Set(ref _datPhongHomNay, value); }
        public int TraPhongHomNay     { get => _traPhongHomNay;   set => Set(ref _traPhongHomNay, value); }
        public int TongKhachHang      { get => _tongKhachHang;    set => Set(ref _tongKhachHang, value); }
        public decimal DoanhThuThang  { get => _doanhThuThang;    set => Set(ref _doanhThuThang, value); }
        public string ThangNam        { get => _thangNam;         set => Set(ref _thangNam, value); }
        public string CapNhatLuc      { get => _capNhatLuc;       set => Set(ref _capNhatLuc, value); }

        public double TyLePhongTrong    => TongPhong > 0 ? (double)PhongTrong / TongPhong * 100 : 0;
        public double TyLePhongSuDung   => TongPhong > 0 ? (double)PhongDangSuDung / TongPhong * 100 : 0;

        public ICommand RefreshCommand { get; }

        public DashboardViewModel()
        {
            RefreshCommand = new RelayCommand(_ => LoadData());
            LoadData();
        }

        public void LoadData()
        {
            try
            {
                var today     = DateTime.Today;
                var firstDay  = new DateTime(today.Year, today.Month, 1);
                ThangNam      = $"Tháng {today.Month}/{today.Year}";

                using var ctx = new HotelDbContext();

                TongPhong       = ctx.Phongs.Count();
                PhongTrong      = ctx.Phongs.Count(p => p.TrangThai == TrangThaiPhong.TrongSach);
                PhongDangSuDung = ctx.Phongs.Count(p => p.TrangThai == TrangThaiPhong.DangSuDung);
                PhongBaoDuong   = ctx.Phongs.Count(p => p.TrangThai == TrangThaiPhong.BaoDuong);
                TongKhachHang   = ctx.KhachHangs.Count();

                DatPhongHomNay  = ctx.DatPhongs.Count(d =>
                    d.NgayNhanPhong.Date == today &&
                    (d.TrangThai == TrangThaiDatPhong.DaDat ||
                     d.TrangThai == TrangThaiDatPhong.DaNhanPhong ||
                     d.TrangThai == TrangThaiDatPhong.DaTraPhong));

                TraPhongHomNay  = ctx.DatPhongs.Count(d =>
                    d.NgayTraPhong.Date == today &&
                    d.TrangThai == TrangThaiDatPhong.DaTraPhong);

                DoanhThuThang   = ctx.HoaDons
                    .Where(h => h.NgayLap >= firstDay && h.TrangThai == "DaThanhToan")
                    .ToList()
                    .Sum(h => h.TongTien);

                OnPropertyChanged(nameof(TyLePhongTrong));
                OnPropertyChanged(nameof(TyLePhongSuDung));
                CapNhatLuc = $"Cập nhật lúc {DateTime.Now:HH:mm:ss}";
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Lỗi tải dashboard: {ex.Message}", "Lỗi", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
            }
        }
    }
}
