//DBHelper.cs
public static class DBHelper
{
    private static string connectionString =
        "Server=localhost\\SQLEXPRESS1;Database=WinCook;Trusted_Connection=True;TrustServerCertificate=True";

    public static string ConnectionString { get => connectionString; set => connectionString = value; }
}
