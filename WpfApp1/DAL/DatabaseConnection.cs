using Microsoft.Data.SqlClient;

namespace WpfApp1.DAL
{
    public static class DatabaseConnection
    {
        // Chữ @ nằm ngoài dấu ngoặc kép, và chỉ dùng 1 dấu \ cho SQLEXPRESS
        private const string ConnectionString = @"Data Source=localhost\SQLEXPRESS;Initial Catalog=QuanLyKhachSan;Integrated Security=True;TrustServerCertificate=True;";

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