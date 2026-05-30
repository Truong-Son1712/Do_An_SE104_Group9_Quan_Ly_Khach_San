using System;
using WpfApp1.BLL;
using WpfApp1.DTO;
using Xunit;

namespace WpfApp1.Tests
{
    public class SettingsTest
    {
        [Fact]
        public void UpdateSettings_ValidData_ShouldNotThrowException()
        {
            // Arrange
            var service = new SettingService();
            var validSettings = new SettingDTO
            {
                PhuThuKhachThu3 = 0.25m,
                SoKhachToiDa = 3,
                TyLeTienCoc = 0.3m,
                GioNhanPhong = new TimeSpan(14, 0, 0),
                GioTraPhong = new TimeSpan(12, 0, 0)
            };

            // Act & Assert
            var ex = Record.Exception(() => service.UpdateSettings(validSettings));
            Assert.Null(ex);
        }

        [Fact]
        public void UpdateSettings_InvalidPhuThu_ShouldThrowException()
        {
            var service = new SettingService();
            var invalidSettings = new SettingDTO
            {
                PhuThuKhachThu3 = 1.5m, // > 100% (Không hợp lệ)
                SoKhachToiDa = 3,
                TyLeTienCoc = 0.3m,
                GioNhanPhong = new TimeSpan(14, 0, 0),
                GioTraPhong = new TimeSpan(12, 0, 0)
            };

            var ex = Assert.Throws<Exception>(() => service.UpdateSettings(invalidSettings));
            Assert.Contains("Tỷ lệ phụ thu khách thứ 3 phải từ 0% đến 100%.", ex.Message);
        }

        [Fact]
        public void UpdateSettings_InvalidSoKhach_ShouldThrowException()
        {
            var service = new SettingService();
            var invalidSettings = new SettingDTO
            {
                PhuThuKhachThu3 = 0.25m,
                SoKhachToiDa = 0, // <= 0 (Không hợp lệ)
                TyLeTienCoc = 0.3m,
                GioNhanPhong = new TimeSpan(14, 0, 0),
                GioTraPhong = new TimeSpan(12, 0, 0)
            };

            var ex = Assert.Throws<Exception>(() => service.UpdateSettings(invalidSettings));
            Assert.Contains("Số khách tối đa mỗi phòng phải lớn hơn 0.", ex.Message);
        }
    }
}
