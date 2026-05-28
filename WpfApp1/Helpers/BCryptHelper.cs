// Yêu cầu NuGet: BCrypt.Net-Next
// Tools → NuGet Package Manager → Package Manager Console:
//   Install-Package BCrypt.Net-Next

namespace WpfApp1.Helpers
{
    /// <summary>
    /// Wrapper BCrypt để hash và verify mật khẩu.
    /// Cost factor 12 – cân bằng bảo mật và hiệu năng.
    /// </summary>
    public static class BCryptHelper
    {
        private const int WorkFactor = 12;

        /// <summary>Hash mật khẩu plain-text.</summary>
        public static string HashPassword(string plainPassword)
            => BCrypt.Net.BCrypt.HashPassword(plainPassword, WorkFactor);

        /// <summary>So sánh plain-text với hash đã lưu trong DB.</summary>
        public static bool VerifyPassword(string plainPassword, string hashedPassword)
            => BCrypt.Net.BCrypt.Verify(plainPassword, hashedPassword);
    }
}