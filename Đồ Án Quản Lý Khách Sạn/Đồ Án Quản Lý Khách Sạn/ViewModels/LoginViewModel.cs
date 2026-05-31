using System.Windows.Input;
using Đồ_Án_Quản_Lý_Khách_Sạn.Data;
using Đồ_Án_Quản_Lý_Khách_Sạn.Helpers;

namespace Đồ_Án_Quản_Lý_Khách_Sạn.ViewModels
{
    public class LoginViewModel : BaseViewModel
    {
        private string _taiKhoan = string.Empty;
        private string _errorMessage = string.Empty;
        private bool _isLoading;

        public string TaiKhoan
        {
            get => _taiKhoan;
            set => Set(ref _taiKhoan, value);
        }

        public string ErrorMessage
        {
            get => _errorMessage;
            set => Set(ref _errorMessage, value);
        }

        public bool IsLoading
        {
            get => _isLoading;
            set => Set(ref _isLoading, value);
        }

        public event Action? LoginSuccessful;

        public ICommand LoginCommand { get; }

        public LoginViewModel()
        {
            LoginCommand = new RelayCommand(ExecuteLogin);
        }

        private void ExecuteLogin(object? parameter)
        {
            string password = parameter as string ?? string.Empty;

            if (string.IsNullOrWhiteSpace(TaiKhoan) || string.IsNullOrWhiteSpace(password))
            {
                ErrorMessage = "Vui lòng nhập tài khoản và mật khẩu.";
                return;
            }

            IsLoading = true;
            ErrorMessage = string.Empty;

            try
            {
                using var ctx = new HotelDbContext();
                var user = ctx.NhanViens.FirstOrDefault(u =>
                    u.TaiKhoan == TaiKhoan && u.IsActive);

                if (user == null || !BCrypt.Net.BCrypt.Verify(password, user.MatKhau))
                {
                    ErrorMessage = "Tài khoản hoặc mật khẩu không chính xác.";
                    return;
                }

                SessionManager.Login(user);
                LoginSuccessful?.Invoke();
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Lỗi hệ thống: {ex.Message}";
            }
            finally
            {
                IsLoading = false;
            }
        }
    }
}
