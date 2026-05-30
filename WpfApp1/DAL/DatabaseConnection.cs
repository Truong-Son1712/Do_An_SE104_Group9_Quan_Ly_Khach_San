using Microsoft.Data.SqlClient;

namespace WpfApp1.DAL
{
    public static class DatabaseConnection
    {
        // Đã sửa lại Data Source thành "." (máy hiện tại - Default Instance) thay vì 3XTER\SQLEXPRESS
        private const string ConnectionString = @"Data Source=.;Initial Catalog=QuanLyKhachSan;Integrated Security=True;TrustServerCertificate=True;";

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