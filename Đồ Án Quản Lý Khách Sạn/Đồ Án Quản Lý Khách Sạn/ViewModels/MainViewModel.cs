using System.Windows.Input;
using Đồ_Án_Quản_Lý_Khách_Sạn.Helpers;

namespace Đồ_Án_Quản_Lý_Khách_Sạn.ViewModels
{
    public class MainViewModel : BaseViewModel
    {
        private object? _currentViewModel;
        private string _currentPageTitle = "Tổng Quan";

        public object? CurrentViewModel
        {
            get => _currentViewModel;
            set => Set(ref _currentViewModel, value);
        }

        public string CurrentPageTitle
        {
            get => _currentPageTitle;
            set => Set(ref _currentPageTitle, value);
        }

        // Thông tin user hiện tại
        public string TenNhanVien => SessionManager.CurrentUser?.HoTen ?? "";
        public string TenVaiTro   => SessionManager.TenVaiTro;

        // Quyền hạn hiển thị menu
        public bool IsAdmin   => SessionManager.IsAdmin;
        public bool IsQuanLy  => SessionManager.IsQuanLy;

        // Permission-based — dùng cho từng mục sidebar
        public bool CanXemBaoCao      => SessionManager.HasPermission(Helpers.Quyen.XemBaoCao);
        public bool CanQuanLyLoaiPhong => SessionManager.HasPermission(Helpers.Quyen.QuanLyLoaiPhong);
        public bool CanQuanLyDichVu   => SessionManager.HasPermission(Helpers.Quyen.QuanLyDichVu);
        public bool CanQuanLyNhanVien => SessionManager.HasPermission(Helpers.Quyen.QuanLyNhanVien);
        public bool CanQuanLyMaGiamGia=> SessionManager.HasPermission(Helpers.Quyen.QuanLyMaGiamGia);
        public bool CanQuanLyLoaiKH   => SessionManager.HasPermission(Helpers.Quyen.QuanLyLoaiKH);
        public bool CanCauHinh        => SessionManager.HasPermission(Helpers.Quyen.CauHinhHeThong);

        // Navigation Commands
        public ICommand NavDashboardCommand  { get; }
        public ICommand NavPhongCommand      { get; }
        public ICommand NavLoaiPhongCommand  { get; }
        public ICommand NavKhachHangCommand  { get; }
        public ICommand NavDatPhongCommand   { get; }
        public ICommand NavHoaDonCommand     { get; }
        public ICommand NavBaoCaoCommand     { get; }
        public ICommand NavNhanVienCommand        { get; }
        public ICommand NavLoaiKhachHangCommand   { get; }
        public ICommand NavLoaiDichVuCommand      { get; }
        public ICommand NavDichVuCommand          { get; }
        public ICommand NavMaGiamGiaCommand       { get; }
        public ICommand NavCauHinhCommand         { get; }
        public ICommand NavLoaiNhanVienCommand    { get; }
        public ICommand NavCaiDatCommand          { get; }
        public ICommand LogoutCommand        { get; }

        public event Action? LogoutRequested;

        public MainViewModel()
        {
            NavDashboardCommand = new RelayCommand(_ => Nav(new DashboardViewModel(), "Tổng Quan"));
            NavPhongCommand     = new RelayCommand(_ => Nav(new PhongViewModel(),     "Quản Lý Phòng"));
            NavLoaiPhongCommand = new RelayCommand(_ => Nav(new LoaiPhongViewModel(), "Loại Phòng"),   _ => SessionManager.HasPermission(Helpers.Quyen.QuanLyLoaiPhong));
            NavKhachHangCommand = new RelayCommand(_ => Nav(new KhachHangViewModel(), "Khách Hàng"));
            NavDatPhongCommand  = new RelayCommand(_ => Nav(new DatPhongViewModel(),  "Đặt Phòng"));
            NavHoaDonCommand    = new RelayCommand(_ => Nav(new HoaDonViewModel(),    "Hóa Đơn"));
            NavBaoCaoCommand    = new RelayCommand(_ => Nav(new BaoCaoViewModel(),    "Báo Cáo"),       _ => SessionManager.HasPermission(Helpers.Quyen.XemBaoCao));
            NavNhanVienCommand       = new RelayCommand(_ => Nav(new NhanVienViewModel(),       "Nhân Viên"),          _ => SessionManager.HasPermission(Helpers.Quyen.QuanLyNhanVien));
            NavLoaiKhachHangCommand  = new RelayCommand(_ => Nav(new LoaiKhachHangViewModel(),  "Loại Khách Hàng"),    _ => SessionManager.HasPermission(Helpers.Quyen.QuanLyLoaiKH));
            NavLoaiDichVuCommand     = new RelayCommand(_ => Nav(new LoaiDichVuViewModel(),     "Loại Dịch Vụ"),       _ => SessionManager.HasPermission(Helpers.Quyen.QuanLyDichVu));
            NavDichVuCommand         = new RelayCommand(_ => Nav(new DichVuViewModel(),         "Dịch Vụ Phòng"));
            NavMaGiamGiaCommand      = new RelayCommand(_ => Nav(new MaGiamGiaViewModel(),      "Mã Giảm Giá"),        _ => SessionManager.HasPermission(Helpers.Quyen.QuanLyMaGiamGia));
            NavCauHinhCommand        = new RelayCommand(_ => Nav(new CauHinhHeThongViewModel(), "Cấu Hình Hệ Thống"),  _ => SessionManager.HasPermission(Helpers.Quyen.CauHinhHeThong));
            NavLoaiNhanVienCommand   = new RelayCommand(_ => Nav(new LoaiNhanVienViewModel(),   "Loại Nhân Viên"),     _ => IsAdmin);
            NavCaiDatCommand         = new RelayCommand(_ => Nav(new CaiDatViewModel(),         "Cài Đặt & Tài Khoản"));
            LogoutCommand       = new RelayCommand(_ => LogoutRequested?.Invoke());

            Nav(new DashboardViewModel(), "Tổng Quan");
        }

        private void Nav(object vm, string title)
        {
            CurrentViewModel  = vm;
            CurrentPageTitle  = title;
        }
    }
}
