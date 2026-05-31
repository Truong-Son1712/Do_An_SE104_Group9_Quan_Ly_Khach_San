using System.Windows;
using System.Windows.Input;
using Đồ_Án_Quản_Lý_Khách_Sạn.ViewModels;

namespace Đồ_Án_Quản_Lý_Khách_Sạn.Views
{
    public partial class LoginWindow : Window
    {
        private readonly LoginViewModel _vm;

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

        private void BtnClose_Click(object sender, RoutedEventArgs e) => Close();

        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed) DragMove();
        }

        private void BtnLogin_Click(object sender, RoutedEventArgs e)
            => _vm.LoginCommand.Execute(PbMatKhau.Password);

        private void Input_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
                _vm.LoginCommand.Execute(PbMatKhau.Password);
        }
    }
}
