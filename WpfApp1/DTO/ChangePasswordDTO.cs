namespace WpfApp1.DTO
{
    /// <summary>Dữ liệu để đổi mật khẩu.</summary>
    public class ChangePasswordDto
    {
        public string AccountID       { get; set; } = string.Empty;
        public string CurrentPassword { get; set; } = string.Empty;
        public string NewPassword     { get; set; } = string.Empty;
        public string ConfirmPassword { get; set; } = string.Empty;
    }
}