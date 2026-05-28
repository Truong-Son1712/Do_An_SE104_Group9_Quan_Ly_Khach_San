using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
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

            // Kéo cửa sổ
            MouseDown += (s, e) =>
            {
                if (e.ChangedButton == MouseButton.Left) DragMove();
            };

            // Trang mặc định theo role
            LoadDefaultPage();
        }

        private void LoadDefaultPage()
        {
            if (_session.IsNhanVien)
                ShowContent(new TextBlock { Text = "Trang Quản lý Phòng (TV2 thực hiện)" }, "Quản lý Phòng");
            else if (_session.IsKeToan)
                ShowContent(new TextBlock { Text = "Trang Báo cáo tháng (TV5 thực hiện)" }, "Báo cáo tháng");
            else
                ShowContent(new TextBlock
                {
                    Text = "Chào mừng! Bấm 'Phân Quyền' ở thanh bên để quản lý tài khoản.",
                    FontSize = 16,
                    Margin = new Thickness(10)
                }, "Trang chủ");
        }

        // -------------------------------------------------------
        // Nav handlers
        // -------------------------------------------------------
        private void BtnPhong_Click(object s, RoutedEventArgs e)
            => ShowContent(new TextBlock { Text = "Trang Quản lý Phòng (TV2 thực hiện)" }, "Quản lý Phòng");

        private void BtnBooking_Click(object s, RoutedEventArgs e)
            => ShowContent(new TextBlock { Text = "Trang Phiếu Thuê (TV3 thực hiện)" }, "Phiếu Thuê Phòng");

        private void BtnHoaDon_Click(object s, RoutedEventArgs e)
            => ShowContent(new TextBlock { Text = "Trang Hóa đơn (TV4 thực hiện)" }, "Hóa đơn Thanh toán");

        private void BtnBaoCao_Click(object s, RoutedEventArgs e)
            => ShowContent(new TextBlock { Text = "Trang Báo cáo (TV5 thực hiện)" }, "Báo cáo tháng");

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