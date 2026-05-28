using System;
using System.Windows.Input;
using WpfApp1.BLL;
using WpfApp1.DTO;
using WpfApp1.Helpers;

namespace WpfApp1.ViewModels
{
    public class LoginViewModel : BaseViewModel
    {
        private readonly AuthService _authService = new();

        // -------------------------------------------------------
        // Properties
        // -------------------------------------------------------

        private string _username = string.Empty;
        public string Username
        {
            get => _username;
            set => SetField(ref _username, value);
        }

        private string _errorMessage = string.Empty;
        public string ErrorMessage
        {
            get => _errorMessage;
            set { SetField(ref _errorMessage, value); OnPropertyChanged(nameof(HasError)); }
        }
        public bool HasError => !string.IsNullOrEmpty(_errorMessage);

        private bool _isLoading;
        public bool IsLoading
        {
            get => _isLoading;
            set { SetField(ref _isLoading, value); OnPropertyChanged(nameof(IsNotLoading)); }
        }
        public bool IsNotLoading => !_isLoading;

        // -------------------------------------------------------
        // Event thông báo đăng nhập thành công → View mở MainWindow
        // -------------------------------------------------------
        public event Action<UserDto>? LoginSucceeded;

        // -------------------------------------------------------
        // Commands
        // -------------------------------------------------------
        public ICommand LoginCommand { get; }

        public LoginViewModel()
        {
            LoginCommand = new RelayCommand(ExecuteLogin, () => IsNotLoading);
        }

        // Password truyền thủ công từ code-behind (PasswordBox không binding được)
        private string _password = string.Empty;
        public void SetPassword(string pwd) => _password = pwd;

        // -------------------------------------------------------
        private void ExecuteLogin()
        {
            ErrorMessage = string.Empty;
            IsLoading    = true;

            try
            {
                var (success, message, user) = _authService.Login(new LoginDto
                {
                    Username = Username,
                    Password = _password,
                });

                if (!success)
                {
                    ErrorMessage = message;
                    return;
                }

                LoginSucceeded?.Invoke(user!);
            }
            catch (Exception ex)
            {
                ErrorMessage = ExceptionHandler.Handle(ex, "LoginViewModel");
            }
            finally
            {
                IsLoading = false;
            }
        }
    }
}