using System;

namespace WpfApp1.DTO
{
    /// <summary>Thông tin người dùng đang đăng nhập (lưu trong Session).</summary>
    public class UserDto
    {
        public string    AccountID { get; set; } = string.Empty;
        public string    Username  { get; set; } = string.Empty;
        public string    FullName  { get; set; } = string.Empty;
        public string    Position  { get; set; } = string.Empty;
        public string    RoleName  { get; set; } = string.Empty;
        public int       RoleID    { get; set; }
        public string?   AvatarURL { get; set; }
        public DateTime? LastLogin { get; set; }

        // Helper kiểm tra quyền nhanh
        public bool IsAdmin    => RoleName == UserRole.Roles.Admin;
        public bool IsNhanVien => RoleName == UserRole.Roles.NhanVien;
        public bool IsGiamDoc  => RoleName == UserRole.Roles.GiamDoc;
        public bool IsKeToan   => RoleName == UserRole.Roles.KeToan;
    }
}