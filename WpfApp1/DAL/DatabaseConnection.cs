using Microsoft.Data.SqlClient;
namespace WpfApp1.DAL
{
    public static class DatabaseConnection
    {
        // Sửa lại chuỗi kết nối cho phù hợp với máy của bạn
        private const string ConnectionString =
            "Server=localhost;Database=QuanLyKhachSan;Trusted_Connection=True;TrustServerCertificate=True;";

        public static SqlConnection GetConnection()
            => new SqlConnection(ConnectionString);

        public static bool TestConnection()
        {
            try
            {
                using var conn = GetConnection();
                conn.Open();
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
