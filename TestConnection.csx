#r "nuget: Npgsql"

using Npgsql;

var connString = "Host=127.0.0.1;Port=5432;Database=saas_db;Username=postgres;Password=123;Timeout=5;SSL Mode=Disable";

Console.WriteLine("Connecting...");
try
{
    using var conn = new NpgsqlConnection(connString);
    conn.Open();
    Console.WriteLine("✅ Connection opened successfully!");
}
catch (Exception ex)
{
    Console.WriteLine($"❌ {ex.GetType().Name}: {ex.Message}");
}