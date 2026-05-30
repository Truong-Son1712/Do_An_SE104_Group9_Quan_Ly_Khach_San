using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;
using WpfApp1.BLL;
using WpfApp1.DTO;
using WpfApp1.GUI;

namespace WpfApp1
{
    public partial class MainWindow : Window
    {
        private readonly SessionManager _session = SessionManager.Current;

        // Binding cho Sidebar – hiển thị menu theo role
        public bool ShowNhanVien  => _session.IsNhanVien || _session.IsAdmin;
        public bool ShowBaoCao    => _session.IsKeToan   || _session.IsGiamDoc || _session.IsAdmin;
        public bool ShowPhanQuyen => _session.IsAdmin    || _session.IsGiamDoc;

        public string UserFullName   => _session.User?.FullName ?? "";
        public string UserRoleDisplay => _session.User?.RoleName switch
        {
            UserRole.Roles.Admin    => "Quản trị viên",
            UserRole.Roles.NhanVien => "Nhân viên lễ tân",
            UserRole.Roles.GiamDoc  => "Giám đốc",
            UserRole.Roles.KeToan   => "Kế toán",
            _ => _session.User?.RoleName ?? ""
        };

        public MainWindow()
        {
            InitializeComponent();
            DataContext = this;

            // Đồng hồ thời gian thực
            var timer = new DispatcherTimer { Interval = System.TimeSpan.FromSeconds(1) };
            timer.Tick += (s, e) => TxtClock.Text = System.DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss");
            timer.Start();
            TxtClock.Text = System.DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss");

            // Trang mặc định theo role
            LoadDefaultPage();
        }

        // -------------------------------------------------------
        // Window Control Handlers (TopBar)
        // -------------------------------------------------------
        private void TopBar_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
                DragMove();
        }

        private void BtnMinimize_Click(object sender, RoutedEventArgs e)
            => WindowState = WindowState.Minimized;

        private void BtnMaximize_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState == WindowState.Maximized 
                ? WindowState.Normal 
                : WindowState.Maximized;
        }

        private void BtnCloseWindow_Click(object sender, RoutedEventArgs e)
            => Application.Current.Shutdown();

        // -------------------------------------------------------
        private void LoadDefaultPage()
        {
            if (_session.IsNhanVien)
                ShowContent(new TextBlock { Text = "Trang Quản lý Phòng (TV2 thực hiện)" }, "Quản lý Phòng");
            else
                ShowContent(new DashboardView(), "Tổng Quan (Dashboard)");
        }

        // -------------------------------------------------------
        // Nav handlers
        // -------------------------------------------------------
        private void BtnPhong_Click(object s, RoutedEventArgs e)
            => ShowContent(new TextBlock { Text = "Trang quản lý phòng (TV2 thực hiện)" }, "Quản Lý Phòng");

        private void BtnLoaiPhong_Click(object s, RoutedEventArgs e)
            => ShowContent(new TextBlock { Text = "Trang Loại Phòng (TV2 thực hiện)" }, "Loại Phòng");

        private void BtnKhachHang_Click(object s, RoutedEventArgs e)
            => ShowContent(new TextBlock { Text = "Trang Khách Hàng (TV3 thực hiện)" }, "Danh Sách Khách Hàng");

        private void BtnBooking_Click(object s, RoutedEventArgs e)
            => ShowContent(new TextBlock { Text = "Trang Phiếu Thuê (TV3 thực hiện)" }, "Phiếu Thuê Phòng");

        private void BtnHoaDon_Click(object s, RoutedEventArgs e)
            => ShowContent(new TextBlock { Text = "Trang Hóa đơn (TV4 thực hiện)" }, "Hóa đơn Thanh toán");

        private void BtnBaoCao_Click(object s, RoutedEventArgs e)
            => ShowContent(new BaoCaoView(), "Báo cáo Doanh thu tháng");

        private void BtnDashboard_Click(object s, RoutedEventArgs e)
            => ShowContent(new DashboardView(), "Tổng Quan (Dashboard)");

        private void BtnCaiDat_Click(object s, RoutedEventArgs e)
            => ShowContent(new SettingView(), "Cài đặt Quy Định Khách Sạn");

        private void BtnPhanQuyen_Click(object s, RoutedEventArgs e)
        {
            var win = new UserManagementWindow { Owner = this };
            win.ShowDialog();
        }

        private void BtnLogout_Click(object s, RoutedEventArgs e)
        {
            new AuthService().Logout();
            new GUI.LoginWindow().Show();
            Close();
        }

        private void BtnClose_Click(object s, RoutedEventArgs e)
            => Application.Current.Shutdown();

        private void ShowContent(object content, string title)
        {
            TxtTitle.Text   = title;
            ContentArea.Content = content;
        }
    }
}