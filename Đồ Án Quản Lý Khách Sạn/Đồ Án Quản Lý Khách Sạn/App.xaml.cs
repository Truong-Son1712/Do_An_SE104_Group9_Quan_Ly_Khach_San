using System.Windows;
using System.Windows.Threading;
using Đồ_Án_Quản_Lý_Khách_Sạn.Data;
using Đồ_Án_Quản_Lý_Khách_Sạn.Views;

namespace Đồ_Án_Quản_Lý_Khách_Sạn
{
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            AppDomain.CurrentDomain.UnhandledException += (s, ex) =>
                MessageBox.Show(ex.ExceptionObject?.ToString(), "Lỗi Khởi Động", MessageBoxButton.OK, MessageBoxImage.Error);
            DispatcherUnhandledException += (s, ex) =>
            {
                MessageBox.Show(ex.Exception?.ToString(), "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                ex.Handled = true;
            };

            base.OnStartup(e);

            // Khởi tạo DB và seed dữ liệu mẫu
            using var ctx = new HotelDbContext();
            DatabaseInitializer.Initialize(ctx);

            // Mở màn hình đăng nhập (không dùng StartupUri)
            var login = new LoginWindow();
            login.Show();
        }
    }
}
