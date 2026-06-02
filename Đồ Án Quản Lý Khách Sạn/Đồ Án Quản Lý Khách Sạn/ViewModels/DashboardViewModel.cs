using System.Windows.Input;
using Đồ_Án_Quản_Lý_Khách_Sạn.Data;
using Đồ_Án_Quản_Lý_Khách_Sạn.Helpers;
using Đồ_Án_Quản_Lý_Khách_Sạn.Models;

namespace Đồ_Án_Quản_Lý_Khách_Sạn.ViewModels
{
    /// ViewModel quản lý dữ liệu hiển thị trên màn hình Dashboard (Tổng quan thống kê)
    public class DashboardViewModel : BaseViewModel
    {
        #region 1. Private Fields - Các biến thành viên
        private int _tongPhong, _phongTrong, _phongDangSuDung, _phongBaoDuong;
        private int _datPhongHomNay, _traPhongHomNay, _tongKhachHang;
        private decimal _doanhThuThang;
        private string _thangNam = string.Empty;
        private string _capNhatLuc = string.Empty;
        #endregion

        #region 2. Public Properties - Các thuộc tính Binding dữ liệu
        /// Tổng số lượng phòng hiện có trong khách sạn
        public int TongPhong         { get => _tongPhong;        set => Set(ref _tongPhong, value); }

        /// Số lượng phòng đang ở trạng thái Trống Sạch
        public int PhongTrong         { get => _phongTrong;       set => Set(ref _phongTrong, value); }

        /// Số lượng phòng đang có khách lưu trú
        public int PhongDangSuDung    { get => _phongDangSuDung;  set => Set(ref _phongDangSuDung, value); }

        /// Số lượng phòng đang trong quá trình bảo dưỡng, sửa chữa
        public int PhongBaoDuong      { get => _phongBaoDuong;    set => Set(ref _phongBaoDuong, value); }

        /// Số lượt nhận phòng (Check-in) dự kiến hoặc thực tế trong hôm nay
        public int DatPhongHomNay     { get => _datPhongHomNay;   set => Set(ref _datPhongHomNay, value); }

        /// Số lượt trả phòng (Check-out) trong hôm nay
        public int TraPhongHomNay     { get => _traPhongHomNay;   set => Set(ref _traPhongHomNay, value); }

        /// Tổng số lượng khách hàng đã lưu trong hệ thống
        public int TongKhachHang      { get => _tongKhachHang;    set => Set(ref _tongKhachHang, value); }

        /// Tổng doanh thu ghi nhận trong tháng hiện tại (gồm hóa đơn và tiền cọc)
        public decimal DoanhThuThang  { get => _doanhThuThang;    set => Set(ref _doanhThuThang, value); }

        /// Chuỗi hiển thị tháng và năm thống kê (Ví dụ: "Tháng 6/2026")
        public string ThangNam        { get => _thangNam;         set => Set(ref _thangNam, value); }

        /// Chuỗi hiển thị thời điểm cập nhật dữ liệu gần nhất
        public string CapNhatLuc      { get => _capNhatLuc;       set => Set(ref _capNhatLuc, value); }

        /// Tỉ lệ phần trăm phòng còn trống
        public double TyLePhongTrong    => TongPhong > 0 ? (double)PhongTrong / TongPhong * 100 : 0;

        /// Tỉ lệ phần trăm phòng đang sử dụng
        public double TyLePhongSuDung   => TongPhong > 0 ? (double)PhongDangSuDung / TongPhong * 100 : 0;
        #endregion

        #region 3. Commands - Lệnh tương tác
        /// Lệnh thực hiện tải lại (làm mới) dữ liệu thống kê
        public ICommand RefreshCommand { get; }
        #endregion

        #region 4. Constructor & Logic Methods - Hàm dựng và xử lý dữ liệu
        /// Khởi tạo DashboardViewModel và thực hiện tải dữ liệu lần đầu
        public DashboardViewModel()
        {
            RefreshCommand = new RelayCommand(_ => LoadData());
            LoadData();
        }

        /// Truy vấn cơ sở dữ liệu để lấy các số liệu thống kê phòng và doanh thu
        public void LoadData()
        {
            try
            {
                var today    = DateTime.Today;
                var firstDay = new DateTime(today.Year, today.Month, 1);
                ThangNam     = $"Tháng {today.Month}/{today.Year}";

                using var ctx = new HotelDbContext();

                TongPhong       = ctx.Phongs.Count();
                PhongTrong      = ctx.Phongs.Count(p => p.TrangThai == TrangThaiPhong.TrongSach);
                PhongDangSuDung = ctx.Phongs.Count(p => p.TrangThai == TrangThaiPhong.DangSuDung);
                PhongBaoDuong   = ctx.Phongs.Count(p => p.TrangThai == TrangThaiPhong.BaoDuong);
                TongKhachHang   = ctx.KhachHangs.Count();

                DatPhongHomNay = ctx.DatPhongs.Count(d =>
                    d.NgayNhanPhong.Date == today &&
                    (d.TrangThai == TrangThaiDatPhong.DaDat ||
                     d.TrangThai == TrangThaiDatPhong.DaNhanPhong ||
                     d.TrangThai == TrangThaiDatPhong.DaTraPhong));

                TraPhongHomNay = ctx.DatPhongs.Count(d =>
                    d.NgayTraPhong.Date == today &&
                    d.TrangThai == TrangThaiDatPhong.DaTraPhong);

                // ── Doanh thu tháng ──────────────────────────────────────────
                // 1) Hóa đơn đã thanh toán trong tháng
                decimal dtHoaDon = ctx.HoaDons
                    .Where(h => h.NgayLap >= firstDay && h.TrangThai == "DaThanhToan")
                    .ToList()
                    .Sum(h => h.TongTien);

                // 2) Tiền cọc nhận trong tháng của các booking chưa có HoaDon DaThanhToan
                //    (cọc được tính vào doanh thu ngay khi xác nhận đặt phòng)
                var paidIds = ctx.HoaDons
                    .Where(h => h.TrangThai == "DaThanhToan")
                    .Select(h => h.MaDatPhong)
                    .ToHashSet();

                decimal dtCoc = ctx.DatPhongs
                    .Where(d => d.NgayDat >= firstDay
                             && d.TrangThai != TrangThaiDatPhong.HuyDat)
                    .ToList()
                    .Where(d => d.TienCoc > 0 && !paidIds.Contains(d.MaDatPhong))
                    .Sum(d => d.TienCoc);

                DoanhThuThang = dtHoaDon + dtCoc;

                OnPropertyChanged(nameof(TyLePhongTrong));
                OnPropertyChanged(nameof(TyLePhongSuDung));
                CapNhatLuc = $"Cập nhật lúc {DateTime.Now:HH:mm:ss}";
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Lỗi tải dashboard: {ex.Message}", "Lỗi",
                    System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
            }
        }
        #endregion
    }
}
