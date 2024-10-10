var builder = WebApplication.CreateBuilder(args);

var port = Environment.GetEnvironmentVariable("PORT");
int portNumber = int.TryParse(port, out var parsedPort) ? parsedPort : 9002;

builder.WebHost.UseKestrel(options =>
{
    options.ListenAnyIP(portNumber);
});

var app = builder.Build();

var postgresHost = Environment.GetEnvironmentVariable("POSTGRES_HOST");
var postgresUser = Environment.GetEnvironmentVariable("POSTGRES_USER");
var postgresPassword = Environment.GetEnvironmentVariable("POSTGRES_PASSWORD");
if (string.IsNullOrEmpty(postgresHost) || string.IsNullOrEmpty(postgresUser) || string.IsNullOrEmpty(postgresPassword))
{
    throw new Exception("POSTGRES_HOST, POSTGRES_USER, POSTGRES_PASSWORD must be set");
}

string masterConnectionString = $"Host={postgresHost};Username={postgresUser};Password={postgresPassword};Database=postgres";
string connectionString = $"Host={postgresHost};Username={postgresUser};Password={postgresPassword};Database=pingpongdb";

_ = Task.Run(() =>
{
    DatabaseHelper.WaitForDatabaseAvailability(masterConnectionString);
    DatabaseHelper.EnsureDatabaseExists(masterConnectionString, "pingpongdb");
    DatabaseHelper.InitializeDatabase(connectionString);
});

// Health check
app.MapGet("/", () => 
{ 
    if (DatabaseHelper.IsConnected)
    {
        return Results.Ok("OK");
    }

    return Results.Problem("Database not available");
});

app.MapGet("/pingpong", () =>
{
    int newCount = DatabaseHelper.IncrementPingPongCount(connectionString);
    return Results.Text($"pong {newCount}");
});

app.MapGet("/pingpongcount", () =>
{
    int currentCount = DatabaseHelper.GetPingPongCount(connectionString);
    return Results.Text($"{currentCount}");
});

app.Run();