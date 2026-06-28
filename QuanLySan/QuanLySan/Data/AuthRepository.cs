using System;
using Microsoft.Data.SqlClient;
using QuanLySan.Models;

namespace QuanLySan.Data
{
    public class AuthRepository
    {
        private readonly string _cs = DatabaseConfig.ConnectionString;

        public (int AccountId, string Username, string Email)? Login(string username, string passwordHash)
        {
            using var conn = new SqlConnection(_cs);
            conn.Open();
            string sql = "SELECT AccountId, Username, Email FROM ACCOUNT WHERE Username = @U AND PasswordHash = @P";
            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@U", username);
            cmd.Parameters.AddWithValue("@P", passwordHash);
            using var reader = cmd.ExecuteReader();
            if (reader.Read())
            {
                return ((int)reader["AccountId"], reader["Username"].ToString(), reader["Email"]?.ToString() ?? "");
            }
            return null;
        }

        public bool Register(string username, string passwordHash, string email)
        {
            using var conn = new SqlConnection(_cs);
            conn.Open();
            
            // Check existing
            string checkSql = "SELECT COUNT(*) FROM ACCOUNT WHERE Username = @U";
            using (var checkCmd = new SqlCommand(checkSql, conn))
            {
                checkCmd.Parameters.AddWithValue("@U", username);
                int count = (int)checkCmd.ExecuteScalar();
                if (count > 0) return false;
            }

            // Insert account
            string insertSql = "INSERT INTO ACCOUNT (Username, PasswordHash, Email) OUTPUT INSERTED.AccountId VALUES (@U, @P, @E)";
            int newAccountId;
            using (var insertCmd = new SqlCommand(insertSql, conn))
            {
                insertCmd.Parameters.AddWithValue("@U", username);
                insertCmd.Parameters.AddWithValue("@P", passwordHash);
                insertCmd.Parameters.AddWithValue("@E", email ?? "");
                newAccountId = (int)insertCmd.ExecuteScalar();
            }

            // Create default THAMSO and TICHDIEM for new account
            // Since RLS is active, we must set SESSION_CONTEXT before inserting, but wait!
            // THAMSO and TICHDIEM inserts will fail if they don't have RLS set.
            // Let's set SESSION_CONTEXT for this connection
            using (var ctxCmd = new SqlCommand("EXEC sp_set_session_context @key=N'AccountId', @value=@A", conn))
            {
                ctxCmd.Parameters.AddWithValue("@A", newAccountId);
                ctxCmd.ExecuteNonQuery();
            }

                using (var paramCmd = new SqlCommand("INSERT INTO THAMSO (MucDiemTichLuyMacDinh, MaLoaiHoiVienMacDinh, TinhTrangKhongDuocDat) VALUES (0, 'DO', 'BT')", conn))
                {
                    paramCmd.ExecuteNonQuery();
                }

                using (var tichDiemCmd = new SqlCommand("INSERT INTO TICHDIEM (HeSoTichDiem) VALUES (100000)", conn))
                {
                    tichDiemCmd.ExecuteNonQuery();
                }

            return true;
        }

        public bool ChangePassword(string username, string oldPasswordHash, string newPasswordHash)
        {
            using var conn = new SqlConnection(_cs);
            conn.Open();
            string sql = "UPDATE ACCOUNT SET PasswordHash = @NewP WHERE Username = @U AND PasswordHash = @OldP";
            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@NewP", newPasswordHash);
            cmd.Parameters.AddWithValue("@U", username);
            cmd.Parameters.AddWithValue("@OldP", oldPasswordHash);
            int rows = cmd.ExecuteNonQuery();
            return rows > 0;
        }
    }
}
