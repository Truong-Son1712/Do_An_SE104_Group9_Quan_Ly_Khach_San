using System.Windows;
using System.Windows.Input;
using WpfApp1.DTO;
using WpfApp1.ViewModels;

namespace WpfApp1.GUI
{
    public partial class LoginWindow : Window
    {
        private readonly LoginViewModel _vm = new();

        public LoginWindow()
        {
            InitializeComponent();
            DataContext = _vm;

            _vm.LoginSucceeded += OnLoginSucceeded;

            // Kéo cửa sổ
            MouseDown += (s, e) =>
            {
                if (e.ChangedButton == MouseButton.Left) DragMove();
            };

            // Enter = Đăng nhập
            KeyDown += (s, e) =>
            {
                if (e.Key == Key.Enter && _vm.LoginCommand.CanExecute(null))
                    _vm.LoginCommand.Execute(null);
            };

            TxtUsername.Focus();
        }

        // PasswordBox không binding được → truyền thủ công
        private void PwdPassword_PasswordChanged(object sender, RoutedEventArgs e)
            => _vm.SetPassword(PwdPassword.Password);

        private void OnLoginSucceeded(UserDto user)
        {
            var main = new MainWindow();
            main.Show();
            Close();
        }

        private void BtnClose_Click(object sender, RoutedEventArgs e)
            => Application.Current.Shutdown();
    }
}