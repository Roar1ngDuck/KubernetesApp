using Npgsql;
using System;

public static class DatabaseHelper
{
    public static bool IsConnected { get; private set; } = false;

    public static void WaitForDatabaseAvailability(string connectionString)
    {
        while (true)
        {
            try
            {
                using var conn = new NpgsqlConnection(connectionString);
                conn.Open();
                IsConnected = true;
                return;
            }
            catch (Exception e)
            { 
                Console.WriteLine($"Unable to connect to database. Retrying in 5 seconds. Reason: {e.Message}");
                Thread.Sleep(5000);
            }
        }
    }

    public static void EnsureDatabaseExists(string masterConnectionString, string databaseName)
    {
        using var conn = new NpgsqlConnection(masterConnectionString);
        conn.Open();
        string checkDbQuery = $"SELECT 1 FROM pg_database WHERE datname = '{databaseName}'";
        using var cmd = new NpgsqlCommand(checkDbQuery, conn);
        var result = cmd.ExecuteScalar();

        if (result != null)
        {
            Console.WriteLine("Database already exists");
            return;
        }

        Console.WriteLine("Creating database");
        string createDbQuery = $"CREATE DATABASE {databaseName}";
        using var createCmd = new NpgsqlCommand(createDbQuery, conn);
        createCmd.ExecuteNonQuery();
    }

    public static void InitializeDatabase(string connectionString)
    {
        using var conn = new NpgsqlConnection(connectionString);
        conn.Open();
        string createTableQuery = @"
            CREATE TABLE IF NOT EXISTS PingPongCount (
                id SERIAL PRIMARY KEY,
                count INT NOT NULL
            )";
        using var cmd = new NpgsqlCommand(createTableQuery, conn);
        cmd.ExecuteNonQuery();
    }

    public static int GetPingPongCount(string connectionString)
    {
        using var conn = new NpgsqlConnection(connectionString);
        conn.Open();
        string selectCountQuery = "SELECT COALESCE(MAX(count), 0) FROM PingPongCount";
        using var cmd = new NpgsqlCommand(selectCountQuery, conn);
        return (int)cmd.ExecuteScalar();
    }

    public static int IncrementPingPongCount(string connectionString)
    {
        using var conn = new NpgsqlConnection(connectionString);
        conn.Open();

        int currentCount = GetPingPongCount(connectionString);
        currentCount++;

        string insertCountQuery = "INSERT INTO PingPongCount (count) VALUES (@count)";
        using var cmd = new NpgsqlCommand(insertCountQuery, conn);
        cmd.Parameters.AddWithValue("count", currentCount);
        cmd.ExecuteNonQuery();

        return currentCount;
    }
}
