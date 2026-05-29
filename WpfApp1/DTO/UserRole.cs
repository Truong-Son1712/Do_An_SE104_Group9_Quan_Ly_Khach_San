namespace WpfApp1.DTO
{
    /// <summary>Vai trò người dùng – ánh xạ bảng USER_ROLE.</summary>
    public class UserRole
    {
        public int    RoleID   { get; set; }
        public string RoleName { get; set; } = string.Empty;

        // Hằng số tên role – dùng để so sánh phân quyền trong code
        public static class Roles
        {
            public const string Admin    = "Admin";
            public const string NhanVien = "NhanVien";
            public const string GiamDoc  = "GiamDoc";
            public const string KeToan   = "KeToan";
        }
    }
}