using System.Data;
using Microsoft.Data.SqlClient;
using WpfApp1.DTO;

namespace WpfApp1.DAL
{
    /// <summary>
    /// Truy cập dữ liệu bảng STAFF.
    /// </summary>
    public class StaffAccess
    {
        /// <summary>Lấy danh sách nhân viên chưa có tài khoản.</summary>
        public List<Staff> GetStaffWithoutAccount()
        {
            var list = new List<Staff>();

            using var conn = DatabaseConnection.GetConnection();
            using var cmd = new SqlCommand("sp_GetStaffWithoutAccount", conn)
            {
                CommandType = CommandType.StoredProcedure
            };

            conn.Open();
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                list.Add(new Staff
                {
                    StaffID  = reader["StaffID"].ToString()!,
                    FullName = reader["FullName"].ToString()!,
                    Position = reader["Position"].ToString()!,
                });
            }

            return list;
        }
    }
}