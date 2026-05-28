using System.Windows;
using System.Windows.Controls;
using WpfApp1.ViewModels;

namespace WpfApp1.GUI
{
    public partial class UserManagementWindow : Window
    {
        private readonly UserManagementViewModel _vm = new();

        public UserManagementWindow()
        {
            InitializeComponent();
            DataContext = _vm;
        }

        // PasswordBox Tab Tạo mới
        private void PwdCreate_Changed(object sender, RoutedEventArgs e)
            => _vm.NewPassword = ((PasswordBox)sender).Password;

        // PasswordBox Tab Đổi mật khẩu
        private void PwdCurrent_Changed(object sender, RoutedEventArgs e)
            => _vm.ChangeCurrent = ((PasswordBox)sender).Password;

        private void PwdNew_Changed(object sender, RoutedEventArgs e)
            => _vm.ChangeNew = ((PasswordBox)sender).Password;

        private void PwdConfirm_Changed(object sender, RoutedEventArgs e)
            => _vm.ChangeConfirm = ((PasswordBox)sender).Password;
    }
}