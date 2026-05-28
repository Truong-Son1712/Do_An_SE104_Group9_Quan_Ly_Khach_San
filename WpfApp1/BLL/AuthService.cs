using WpfApp1.DAL;
using WpfApp1.DTO;
using WpfApp1.Helpers;

namespace WpfApp1.BLL
{
    /// <summary>
    /// Xử lý nghiệp vụ đăng nhập và đăng xuất.
    /// </summary>
    public class AuthService
    {
        private readonly UserAccess _userAccess = new();

        // -------------------------------------------------------
        // ĐĂNG NHẬP
        // Trả về (success, message, userDto)
        // -------------------------------------------------------
        public (bool Success, string Message, UserDto? User) Login(LoginDto dto)
        {
            // 1. Validate đầu vào
            if (string.IsNullOrWhiteSpace(dto.Username))
                return (false, "Vui lòng nhập tên đăng nhập.", null);

            if (string.IsNullOrWhiteSpace(dto.Password))
                return (false, "Vui lòng nhập mật khẩu.", null);

            // 2. Tìm tài khoản trong DB
            UserAccount? account = _userAccess.GetByUsername(dto.Username.Trim());

            if (account == null)
                return (false, "Tên đăng nhập không tồn tại.", null);

            // 3. Kiểm tra tài khoản bị khóa
            if (!account.IsActive)
                return (false, "Tài khoản đã bị vô hiệu hóa. Liên hệ Admin.", null);

            // 4. Xác minh mật khẩu bằng BCrypt
            if (!BCryptHelper.VerifyPassword(dto.Password, account.PasswordHash))
                return (false, "Mật khẩu không đúng.", null);

            // 5. Cập nhật LastLogin
            try { _userAccess.UpdateLastLogin(account.AccountID); }
            catch { /* không block login */ }

            // 6. Map sang UserDto và lưu Session
            var userDto = new UserDto
            {
                AccountID = account.AccountID,
                Username  = account.Username,
                FullName  = account.FullName,
                Position  = account.Position,
                RoleName  = account.RoleName,
                RoleID    = account.RoleID,
                AvatarURL = account.AvatarURL,
                LastLogin = account.LastLogin,
            };

            SessionManager.Current.SetUser(userDto);

            return (true, $"Chào mừng, {userDto.FullName}!", userDto);
        }

        // -------------------------------------------------------
        // ĐĂNG XUẤT
        // -------------------------------------------------------
        public void Logout()
        {
            SessionManager.Current.Clear();
        }
    }
}