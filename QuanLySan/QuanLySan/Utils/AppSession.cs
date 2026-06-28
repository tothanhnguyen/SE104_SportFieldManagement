namespace QuanLySan.Utils
{
    public static class AppSession
    {
        public static int CurrentAccountId { get; set; } = 1; // Default to 1 (Admin) until login is complete
        public static string CurrentUsername { get; set; } = "admin";
        public static string CurrentEmail { get; set; } = "admin@example.com";
    }
}
