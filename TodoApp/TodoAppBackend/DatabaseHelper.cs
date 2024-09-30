using Npgsql;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace TodoAppBackend;

public class DatabaseHelper
{
    private string _masterConnectionString;
    private string _connectionString;
    private string _databaseHost;
    private string _databaseName;
    private string _databaseUser;
    private string _databasePassword;

    public DatabaseHelper(string databaseHost, string databaseName, string databaseUser, string databasePassword)
    {
        _databaseHost = databaseHost;
        _databaseName = databaseName;
        _databaseUser = databaseUser;
        _databasePassword = databasePassword;
        _masterConnectionString = $"Host={_databaseHost};Username={_databaseUser};Password={_databasePassword};Database=postgres";
        _connectionString = $"Host={_databaseHost};Username={_databaseUser};Password={_databasePassword};Database={_databaseName}";
    }

    public async Task WaitForDatabaseAvailabilityAsync()
    {
        while (true)
        {
            if (!await IsDatabaseAvailableAsync())
            {
                await Task.Delay(5000);
                continue;
            }

            return;
        }
    }

    public async Task<bool> IsDatabaseAvailableAsync()
    {
        try
        {
            await using var conn = new NpgsqlConnection(_masterConnectionString);
            await conn.OpenAsync();
            return true;
        }
        catch (Exception e)
        {
            Console.WriteLine($"Unable to connect to database. Reason: {e.Message}");
            return false;
        }
    }

    private async Task EnsureDatabaseExistsAsync()
    {
        await using var conn = new NpgsqlConnection(_masterConnectionString);
        await conn.OpenAsync();
        string checkDbQuery = $"SELECT 1 FROM pg_database WHERE datname = '{_databaseName}'";
        await using var cmd = new NpgsqlCommand(checkDbQuery, conn);
        var result = await cmd.ExecuteScalarAsync();

        if (result != null)
        {
            Console.WriteLine("Database already exists");
            return;
        }

        Console.WriteLine("Creating database");
        string createDbQuery = $"CREATE DATABASE {_databaseName}";
        await using var createCmd = new NpgsqlCommand(createDbQuery, conn);
        await createCmd.ExecuteNonQueryAsync();
    }

    public async Task InitializeDatabaseAsync()
    {
        await EnsureDatabaseExistsAsync();

        await using var conn = new NpgsqlConnection(_connectionString);
        await conn.OpenAsync();
        string createTableQuery = @"
            CREATE TABLE IF NOT EXISTS Todos (
                id SERIAL PRIMARY KEY,
                todoText TEXT NOT NULL,
                done BOOLEAN NOT NULL DEFAULT FALSE
            )";
        await using var cmd = new NpgsqlCommand(createTableQuery, conn);
        await cmd.ExecuteNonQueryAsync();
    }

    public async Task<List<Todo>> GetTodosAsync()
    {
        var todos = new List<Todo>();
        await using var conn = new NpgsqlConnection(_connectionString);
        await conn.OpenAsync();
        string selectTodosQuery = "SELECT id, todoText, done FROM Todos";
        await using var cmd = new NpgsqlCommand(selectTodosQuery, conn);
        await using var reader = await cmd.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            var todo = new Todo(reader.GetInt32(0), reader.GetString(1), reader.GetBoolean(2));
            todos.Add(todo);
        }

        return todos;
    }

    public async Task AddTodoAsync(string todo)
    {
        await using var conn = new NpgsqlConnection(_connectionString);
        await conn.OpenAsync();

        string insertTodoQuery = "INSERT INTO Todos (todoText) VALUES (@todoText)";
        await using var cmd = new NpgsqlCommand(insertTodoQuery, conn);
        cmd.Parameters.AddWithValue("todoText", todo);
        await cmd.ExecuteNonQueryAsync();
    }

    public async Task MarkTodoAsDoneAsync(int id)
    {
        await using var conn = new NpgsqlConnection(_connectionString);
        await conn.OpenAsync();

        string updateTodoQuery = "UPDATE Todos SET done = TRUE WHERE id = @id";
        await using var cmd = new NpgsqlCommand(updateTodoQuery, conn);
        cmd.Parameters.AddWithValue("id", id);
        await cmd.ExecuteNonQueryAsync();
    }
}