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
string masterConnectionString = $"Host={postgresHost};Username={postgresUser};Password={postgresPassword};Database=postgres";
string connectionString = $"Host={postgresHost};Username={postgresUser};Password={postgresPassword};Database=pingpongdb";

DatabaseHelper.EnsureDatabaseExists(masterConnectionString, "pingpongdb");

DatabaseHelper.InitializeDatabase(connectionString);

// Ingress expects 200 OK as health check
app.MapGet("/", () => { return Results.Ok(); });

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