using System.Windows;
using System.Windows.Input;
using Đồ_Án_Quản_Lý_Khách_Sạn.ViewModels;

namespace Đồ_Án_Quản_Lý_Khách_Sạn.Views
{
    public partial class LoginWindow : Window
    {
        private readonly LoginViewModel _vm;
        private bool _isPasswordVisible;

        public LoginWindow()
        {
            InitializeComponent();
            _vm = new LoginViewModel();
            _vm.LoginSuccessful += OnLoginSuccessful;
            DataContext = _vm;
        }

        private void OnLoginSuccessful()
        {
            var main = new MainWindow();
            main.Show();
            Close();
        }

        private void BtnTogglePass_Click(object sender, RoutedEventArgs e)
        {
            _isPasswordVisible = !_isPasswordVisible;

            if (_isPasswordVisible)
            {
                // Hiện mật khẩu: sao chép sang TextBox rồi ẩn PasswordBox
                TxtMatKhauRo.Text          = PbMatKhau.Password;
                PbMatKhau.Visibility       = Visibility.Collapsed;
                TxtMatKhauRo.Visibility    = Visibility.Visible;
                EyeSlash.Visibility        = Visibility.Collapsed; // mắt mở = không có gạch
                TxtMatKhauRo.CaretIndex    = TxtMatKhauRo.Text.Length;
                TxtMatKhauRo.Focus();
            }
            else
            {
                // Ẩn mật khẩu: sao chép ngược lại PasswordBox
                PbMatKhau.Password         = TxtMatKhauRo.Text;
                TxtMatKhauRo.Visibility    = Visibility.Collapsed;
                PbMatKhau.Visibility       = Visibility.Visible;
                EyeSlash.Visibility        = Visibility.Visible;   // mắt đóng = có gạch
                PbMatKhau.Focus();
            }
        }

        private string GetCurrentPassword()
            => _isPasswordVisible ? TxtMatKhauRo.Text : PbMatKhau.Password;

        private void BtnClose_Click(object sender, RoutedEventArgs e) => Close();

        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed) DragMove();
        }

        private void BtnLogin_Click(object sender, RoutedEventArgs e)
            => _vm.LoginCommand.Execute(GetCurrentPassword());

        private void Input_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
                _vm.LoginCommand.Execute(GetCurrentPassword());
        }
    }
}
