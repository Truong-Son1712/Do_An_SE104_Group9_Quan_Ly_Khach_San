namespace WpfApp1.DTO
{
    /// <summary>Dữ liệu để tạo tài khoản mới.</summary>
    public class CreateUserDto
    {
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string StaffID  { get; set; } = string.Empty;
        public int    RoleID   { get; set; }
    }
}