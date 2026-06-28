namespace QuanLySan.Models
{
    public static class DatabaseConfig
    {
        // Khai báo public static để dùng chung toàn dự án
        public static string ConnectionString = @"Data Source=localhost;Initial Catalog=QLSanTheThao;Integrated Security=True;Persist Security Info=False;Pooling=False;MultipleActiveResultSets=False;Encrypt=False;TrustServerCertificate=True;Command Timeout=30";
    }
}
