using System.Collections.Generic;
using System.Data;
using Microsoft.Data.SqlClient;
using WpfApp1.DTO;

namespace WpfApp1.DAL
{
    /// <summary>
    /// Truy cập dữ liệu bảng USER_ROLE.
    /// </summary>
    public class UserRoleAccess
    {
        public List<UserRole> GetAll()
        {
            var list = new List<UserRole>();

            using var conn = DatabaseConnection.GetConnection();
            using var cmd = new SqlCommand("sp_GetAllRoles", conn)
            {
                CommandType = CommandType.StoredProcedure
            };

            conn.Open();
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                list.Add(new UserRole
                {
                    RoleID   = (int)reader["RoleID"],
                    RoleName = reader["RoleName"].ToString()!
                });
            }

            return list;
        }
    }
}