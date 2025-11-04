using System.Configuration;

namespace WinCook
{
    public static class DBHelper
    {
        private static string connectionString = "Server=localhost;Database=WinCook;Trusted_Connection=True;TrustServerCertificate=True";

        public static string ConnectionString { get => connectionString; set => connectionString = value; }
    }
}
