using System;

namespace WpfApp1.DTO
{
    /// <summary>Tài khoản đăng nhập – ánh xạ bảng USER_ACCOUNT.</summary>
    public class UserAccount
    {
        public string    AccountID    { get; set; } = string.Empty;
        public string    Username     { get; set; } = string.Empty;
        public string    PasswordHash { get; set; } = string.Empty;
        public string    StaffID      { get; set; } = string.Empty;
        public int       RoleID       { get; set; }
        public string?   AvatarURL    { get; set; }
        public bool      IsActive     { get; set; } = true;
        public DateTime  CreatedAt    { get; set; } = DateTime.Now;
        public DateTime? LastLogin    { get; set; }

        // Populated từ JOIN khi query
        public string FullName { get; set; } = string.Empty;
        public string Position { get; set; } = string.Empty;
        public string RoleName { get; set; } = string.Empty;
    }
}