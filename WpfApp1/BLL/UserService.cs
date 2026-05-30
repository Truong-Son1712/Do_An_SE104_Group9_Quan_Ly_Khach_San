using System;
using System.Collections.Generic;
using System.Linq;
using WpfApp1.DAL;
using WpfApp1.DTO;
using WpfApp1.Helpers;

namespace WpfApp1.BLL
{
    /// <summary>
    /// Nghiệp vụ quản lý tài khoản người dùng (CRUD).
    /// Chỉ Admin / Giám đốc mới được dùng.
    /// </summary>
    public class UserService
    {
        private readonly UserAccess     _userAccess  = new();
        private readonly UserRoleAccess _roleAccess  = new();
        private readonly StaffAccess    _staffAccess = new();

        // -------------------------------------------------------
        // READ
        // -------------------------------------------------------
        public List<UserAccount> GetAllUsers()          => _userAccess.GetAll();
        public List<UserRole>    GetAllRoles()           => _roleAccess.GetAll();
        public List<Staff>       GetStaffWithoutAccount() => _staffAccess.GetStaffWithoutAccount();

        // -------------------------------------------------------
        // CREATE
        // -------------------------------------------------------
        public void CreateUser(CreateUserDto dto)
        {
            // Validate
            if (string.IsNullOrWhiteSpace(dto.Username))
                throw new ExceptionHandler.ValidationException("Tên đăng nhập không được để trống.");
            if (dto.Username.Length < 4)
                throw new ExceptionHandler.ValidationException("Tên đăng nhập phải có ít nhất 4 ký tự.");
            if (string.IsNullOrWhiteSpace(dto.Password))
                throw new ExceptionHandler.ValidationException("Mật khẩu không được để trống.");
            if (dto.Password.Length < 6)
                throw new ExceptionHandler.ValidationException("Mật khẩu phải có ít nhất 6 ký tự.");
            if (string.IsNullOrWhiteSpace(dto.StaffID))
                throw new ExceptionHandler.ValidationException("Vui lòng chọn nhân viên.");
            if (dto.RoleID <= 0)
                throw new ExceptionHandler.ValidationException("Vui lòng chọn quyền hạn.");

            var account = new UserAccount
            {
                AccountID    = "ACC" + DateTime.Now.ToString("yyMMddHHmmss"),
                Username     = dto.Username.Trim(),
                PasswordHash = BCryptHelper.HashPassword(dto.Password),
                StaffID      = dto.StaffID,
                RoleID       = dto.RoleID,
                IsActive     = true,
            };

            _userAccess.Create(account);
        }

        // -------------------------------------------------------
        // UPDATE
        // -------------------------------------------------------
        public void UpdateUser(UpdateUserDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Username))
                throw new ExceptionHandler.ValidationException("Tên đăng nhập không được để trống.");
            if (dto.RoleID <= 0)
                throw new ExceptionHandler.ValidationException("Vui lòng chọn quyền hạn.");

            _userAccess.Update(new UserAccount
            {
                AccountID = dto.AccountID,
                Username  = dto.Username.Trim(),
                RoleID    = dto.RoleID,
                IsActive  = dto.IsActive,
            });
        }

        // -------------------------------------------------------
        // ĐỔI MẬT KHẨU
        // -------------------------------------------------------
        public void ChangePassword(ChangePasswordDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.NewPassword))
                throw new ExceptionHandler.ValidationException("Mật khẩu mới không được để trống.");
            if (dto.NewPassword.Length < 6)
                throw new ExceptionHandler.ValidationException("Mật khẩu phải có ít nhất 6 ký tự.");
            if (dto.NewPassword != dto.ConfirmPassword)
                throw new ExceptionHandler.ValidationException("Xác nhận mật khẩu không khớp.");

            // Nếu có mật khẩu hiện tại → xác minh trước
            if (!string.IsNullOrEmpty(dto.CurrentPassword))
            {
                var user = _userAccess.GetAll()
                                      .FirstOrDefault(u => u.AccountID == dto.AccountID);
                if (user == null)
                    throw new ExceptionHandler.NotFoundException("Không tìm thấy tài khoản.");
                if (!BCryptHelper.VerifyPassword(dto.CurrentPassword, user.PasswordHash))
                    throw new ExceptionHandler.AuthenticationException("Mật khẩu hiện tại không đúng.");
            }

            _userAccess.ChangePassword(dto.AccountID, BCryptHelper.HashPassword(dto.NewPassword));
        }

        // -------------------------------------------------------
        // VÔ HIỆU HÓA
        // -------------------------------------------------------
        public void DeactivateUser(string accountId)
        {
            if (string.IsNullOrWhiteSpace(accountId))
                throw new ExceptionHandler.ValidationException("Không xác định được tài khoản.");
            _userAccess.Deactivate(accountId);
        }
    }
}