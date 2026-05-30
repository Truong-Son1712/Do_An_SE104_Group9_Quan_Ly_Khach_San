using System;
using WpfApp1.BLL;
using WpfApp1.DTO;
using Xunit;

namespace WpfApp1.Tests
{
    public class AuthTest
    {
        [Fact]
        public void HashPassword_ShouldReturnHashedString()
        {
            // Arrange
            string password = "123";

            // Act
            string hash = BCrypt.Net.BCrypt.HashPassword(password);

            // Assert
            Assert.NotNull(hash);
            Assert.NotEqual(password, hash);
            Assert.True(BCrypt.Net.BCrypt.Verify(password, hash));
        }

        [Fact]
        public void Login_EmptyCredentials_ShouldReturnFalse()
        {
            // Arrange
            var authService = new AuthService();
            var dto = new LoginDto { Username = "", Password = "" };

            // Act
            var result = authService.Login(dto);

            // Assert
            Assert.False(result.Success);
            Assert.Equal("Vui lòng nhập tên đăng nhập.", result.Message);
        }
    }
}
