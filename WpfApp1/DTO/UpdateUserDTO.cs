namespace WpfApp1.DTO
{
    /// <summary>Dữ liệu để cập nhật tài khoản.</summary>
    public class UpdateUserDto
    {
        public string AccountID { get; set; } = string.Empty;
        public string Username  { get; set; } = string.Empty;
        public int    RoleID    { get; set; }
        public bool   IsActive  { get; set; }
    }
}