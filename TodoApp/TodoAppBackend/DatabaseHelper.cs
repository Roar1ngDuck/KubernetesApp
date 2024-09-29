using Npgsql;
using System;

public static class DatabaseHelper
{
    public static void WaitForDatabaseAvailability(string connectionString)
    {
        while (true)
        {
            if (!IsDatabaseAvailable(connectionString))
            {
                Thread.Sleep(5000);
                continue;
            }

            return;
        }
    }

    public static bool IsDatabaseAvailable(string connectionString)
    {
        try
        {
            using var conn = new NpgsqlConnection(connectionString);
            conn.Open();
            return true;
        }
        catch (Exception e)
        { 
            Console.WriteLine($"Unable to connect to database. Reason: {e.Message}");
            return false;
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
            CREATE TABLE IF NOT EXISTS Todos (
                id SERIAL PRIMARY KEY,
                todoText TEXT NOT NULL
            )";
        using var cmd = new NpgsqlCommand(createTableQuery, conn);
        cmd.ExecuteNonQuery();
    }

    public static List<string> GetTodos(string connectionString)
    {
        var todos = new List<string>();
        using var conn = new NpgsqlConnection(connectionString);
        conn.Open();
        string selectTodosQuery = "SELECT todoText FROM Todos";
        using var cmd = new NpgsqlCommand(selectTodosQuery, conn);
        using var reader = cmd.ExecuteReader();

        while (reader.Read())
        {
            todos.Add(reader.GetString(0));
        }

        return todos;
    }

    public static void AddTodo(string connectionString, string todo)
    {
        using var conn = new NpgsqlConnection(connectionString);
        conn.Open();

        string insertTodoQuery = "INSERT INTO Todos (todoText) VALUES (@todoText)";
        using var cmd = new NpgsqlCommand(insertTodoQuery, conn);
        cmd.Parameters.AddWithValue("todoText", todo);
        cmd.ExecuteNonQuery();
    }
}
