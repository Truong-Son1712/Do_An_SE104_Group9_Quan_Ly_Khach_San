using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using Đồ_Án_Quản_Lý_Khách_Sạn.ViewModels;
using Đồ_Án_Quản_Lý_Khách_Sạn.Views;

namespace Đồ_Án_Quản_Lý_Khách_Sạn
{
    public partial class MainWindow : Window
    {
        private readonly DispatcherTimer _clock = new();

        public MainWindow()
        {
            InitializeComponent();
            var vm = new MainViewModel();
            vm.LogoutRequested += OnLogout;
            DataContext = vm;

            _clock.Interval = TimeSpan.FromSeconds(1);
            _clock.Tick += (_, _) =>
                TxtDate.Text = DateTime.Now.ToString("dddd, dd/MM/yyyy  HH:mm:ss");
            _clock.Start();
            TxtDate.Text = DateTime.Now.ToString("dddd, dd/MM/yyyy  HH:mm:ss");
        }

        private void OnLogout()
        {
            _clock.Stop();
            Đồ_Án_Quản_Lý_Khách_Sạn.Helpers.SessionManager.Logout();
            var login = new LoginWindow();
            login.Show();
            Close();
        }

        private void Header_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed) DragMove();
        }

        private void BtnMinimize_Click(object sender, RoutedEventArgs e) =>
            WindowState = WindowState.Minimized;

        private void BtnMaximize_Click(object sender, RoutedEventArgs e) =>
            WindowState = WindowState == WindowState.Maximized
                ? WindowState.Normal : WindowState.Maximized;

        private void BtnClose_Click(object sender, RoutedEventArgs e) => Close();
    }
}