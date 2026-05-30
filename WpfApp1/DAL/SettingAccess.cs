using System;
using System.Data;
using Microsoft.Data.SqlClient;
using WpfApp1.DTO;

namespace WpfApp1.DAL
{
    /// <summary>
    /// Tầng truy xuất dữ liệu (DAL) cho Cấu hình/Quy định.
    /// Lưu ý: Bảng QUY_DINH chưa có trong CSDL nên file này tạm thời giả lập việc lưu trữ (Mock DB) 
    /// để không làm crash ứng dụng trên main. Khi nào có bảng QUY_DINH thật, chỉ cần mở comment các dòng code SQL.
    /// </summary>
    public class SettingAccess
    {
        // Biến static lưu tạm dữ liệu quy định trong RAM (Mock Database)
        private static SettingDTO _mockDbSettings = new SettingDTO
        {
            PhuThuKhachThu3 = 0.25m,  // 25%
            SoKhachToiDa = 3,         // Tối đa 3 khách
            TyLeTienCoc = 0.3m,       // Cọc 30%
            GioNhanPhong = new TimeSpan(14, 0, 0), // 14:00
            GioTraPhong = new TimeSpan(12, 0, 0)   // 12:00
        };

        /// <summary>
        /// Lấy quy định hiện tại.
        /// Tương lai sẽ dùng "SELECT * FROM QUY_DINH"
        /// </summary>
        public SettingDTO GetSettings()
        {
            /* 
             * Đoạn code kết nối SQL thực tế sẽ được bật lên khi DB hoàn thành.
             * 
            using (var conn = DatabaseConnection.GetConnection())
            {
                conn.Open();
                var cmd = new SqlCommand("SELECT TOP 1 PhuThuKhachThu3, SoKhachToiDa, TyLeTienCoc, GioNhanPhong, GioTraPhong FROM QUY_DINH", conn);
                using (var reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        return new SettingDTO
                        {
                            PhuThuKhachThu3 = reader.GetDecimal(0),
                            SoKhachToiDa = reader.GetInt32(1),
                            TyLeTienCoc = reader.GetDecimal(2),
                            GioNhanPhong = reader.GetTimeSpan(3),
                            GioTraPhong = reader.GetTimeSpan(4)
                        };
                    }
                }
            }
            */

            // Trả về dữ liệu giả lập (Mock) hiện tại để test UI
            return new SettingDTO
            {
                PhuThuKhachThu3 = _mockDbSettings.PhuThuKhachThu3,
                SoKhachToiDa = _mockDbSettings.SoKhachToiDa,
                TyLeTienCoc = _mockDbSettings.TyLeTienCoc,
                GioNhanPhong = _mockDbSettings.GioNhanPhong,
                GioTraPhong = _mockDbSettings.GioTraPhong
            };
        }

        /// <summary>
        /// Cập nhật quy định.
        /// Tương lai sẽ dùng "UPDATE QUY_DINH SET..."
        /// </summary>
        public bool UpdateSettings(SettingDTO settings)
        {
            /*
             * Đoạn code kết nối SQL thực tế sẽ được bật lên khi DB hoàn thành.
             * 
            using (var conn = DatabaseConnection.GetConnection())
            {
                conn.Open();
                var cmd = new SqlCommand("UPDATE QUY_DINH SET PhuThuKhachThu3=@phuThu, SoKhachToiDa=@soKhach, TyLeTienCoc=@coc, GioNhanPhong=@nhanPhong, GioTraPhong=@traPhong", conn);
                cmd.Parameters.AddWithValue("@phuThu", settings.PhuThuKhachThu3);
                cmd.Parameters.AddWithValue("@soKhach", settings.SoKhachToiDa);
                cmd.Parameters.AddWithValue("@coc", settings.TyLeTienCoc);
                cmd.Parameters.AddWithValue("@nhanPhong", settings.GioNhanPhong);
                cmd.Parameters.AddWithValue("@traPhong", settings.GioTraPhong);
                
                return cmd.ExecuteNonQuery() > 0;
            }
            */

            // Lưu trực tiếp vào biến tĩnh để giả lập (Mock) việc update CSDL
            _mockDbSettings.PhuThuKhachThu3 = settings.PhuThuKhachThu3;
            _mockDbSettings.SoKhachToiDa = settings.SoKhachToiDa;
            _mockDbSettings.TyLeTienCoc = settings.TyLeTienCoc;
            _mockDbSettings.GioNhanPhong = settings.GioNhanPhong;
            _mockDbSettings.GioTraPhong = settings.GioTraPhong;

            return true;
        }
    }
}
