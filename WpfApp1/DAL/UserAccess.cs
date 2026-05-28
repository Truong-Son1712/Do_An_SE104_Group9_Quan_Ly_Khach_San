using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using WpfApp1.DTO;

namespace WpfApp1.DAL
{
    /// <summary>
    /// Truy cập dữ liệu bảng USER_ACCOUNT.
    /// </summary>
    public class UserAccess
    {
        // ----------------------------------------------------------------
        // READ
        // ----------------------------------------------------------------

        public UserAccount? GetByUsername(string username)
        {
            using var conn = DatabaseConnection.GetConnection();
            using var cmd = new SqlCommand("sp_GetUserByUsername", conn)
            {
                CommandType = CommandType.StoredProcedure
            };
            cmd.Parameters.AddWithValue("@Username", username);

            conn.Open();
            using var reader = cmd.ExecuteReader();
            return reader.Read() ? MapRow(reader) : null;
        }

        public List<UserAccount> GetAll()
        {
            var list = new List<UserAccount>();

            using var conn = DatabaseConnection.GetConnection();
            using var cmd = new SqlCommand("sp_GetAllUsers", conn)
            {
                CommandType = CommandType.StoredProcedure
            };

            conn.Open();
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
                list.Add(MapRow(reader));

            return list;
        }

        // ----------------------------------------------------------------
        // CREATE
        // ----------------------------------------------------------------

        public void Create(UserAccount account)
        {
            using var conn = DatabaseConnection.GetConnection();
            using var cmd = new SqlCommand("sp_CreateUser", conn)
            {
                CommandType = CommandType.StoredProcedure
            };
            cmd.Parameters.AddWithValue("@AccountID",    account.AccountID);
            cmd.Parameters.AddWithValue("@Username",     account.Username);
            cmd.Parameters.AddWithValue("@PasswordHash", account.PasswordHash);
            cmd.Parameters.AddWithValue("@StaffID",      account.StaffID);
            cmd.Parameters.AddWithValue("@RoleID",       account.RoleID);

            conn.Open();
            cmd.ExecuteNonQuery();
        }

        // ----------------------------------------------------------------
        // UPDATE
        // ----------------------------------------------------------------

        public void Update(UserAccount account)
        {
            using var conn = DatabaseConnection.GetConnection();
            using var cmd = new SqlCommand("sp_UpdateUser", conn)
            {
                CommandType = CommandType.StoredProcedure
            };
            cmd.Parameters.AddWithValue("@AccountID", account.AccountID);
            cmd.Parameters.AddWithValue("@Username",  account.Username);
            cmd.Parameters.AddWithValue("@RoleID",    account.RoleID);
            cmd.Parameters.AddWithValue("@IsActive",  account.IsActive);

            conn.Open();
            cmd.ExecuteNonQuery();
        }

        public void ChangePassword(string accountId, string newPasswordHash)
        {
            using var conn = DatabaseConnection.GetConnection();
            using var cmd = new SqlCommand("sp_ChangePassword", conn)
            {
                CommandType = CommandType.StoredProcedure
            };
            cmd.Parameters.AddWithValue("@AccountID",       accountId);
            cmd.Parameters.AddWithValue("@NewPasswordHash", newPasswordHash);

            conn.Open();
            cmd.ExecuteNonQuery();
        }

        public void UpdateLastLogin(string accountId)
        {
            using var conn = DatabaseConnection.GetConnection();
            using var cmd = new SqlCommand("sp_UpdateLastLogin", conn)
            {
                CommandType = CommandType.StoredProcedure
            };
            cmd.Parameters.AddWithValue("@AccountID", accountId);

            conn.Open();
            cmd.ExecuteNonQuery();
        }

        // ----------------------------------------------------------------
        // DELETE (soft)
        // ----------------------------------------------------------------

        public void Deactivate(string accountId)
        {
            using var conn = DatabaseConnection.GetConnection();
            using var cmd = new SqlCommand("sp_DeactivateUser", conn)
            {
                CommandType = CommandType.StoredProcedure
            };
            cmd.Parameters.AddWithValue("@AccountID", accountId);

            conn.Open();
            cmd.ExecuteNonQuery();
        }

        // ----------------------------------------------------------------
        // Mapping
        // ----------------------------------------------------------------

        private static UserAccount MapRow(SqlDataReader r) => new()
        {
            AccountID    = r["AccountID"].ToString()!,
            Username     = r["Username"].ToString()!,
            PasswordHash = r["PasswordHash"].ToString()!,
            StaffID      = r["StaffID"].ToString()!,
            RoleID       = Convert.ToInt32(r["RoleID"]),
            AvatarURL    = r["AvatarURL"] as string,
            IsActive     = r["IsActive"] != DBNull.Value && Convert.ToBoolean(r["IsActive"]),
            LastLogin    = r["LastLogin"] as DateTime?,
            FullName     = r["FullName"].ToString()!,
            Position     = r["Position"].ToString()!,
            RoleName     = r["RoleName"].ToString()!,
        };
    }
}