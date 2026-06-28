using System.Data;
using Microsoft.Data.SqlClient;

namespace QuanLySan.Utils
{
    public static class DbHelper
    {
        public static void OpenConnection(SqlConnection conn)
        {
            if (conn.State != ConnectionState.Open)
            {
                conn.Open();
            }
            
            // Apply Row-Level Security session context for multi-tenancy
            using var cmd = new SqlCommand("EXEC sp_set_session_context @key=N'AccountId', @value=@AccountId", conn);
            cmd.Parameters.AddWithValue("@AccountId", AppSession.CurrentAccountId);
            cmd.ExecuteNonQuery();
        }
    }
}
