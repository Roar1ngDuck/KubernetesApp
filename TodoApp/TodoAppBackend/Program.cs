var builder = WebApplication.CreateBuilder(args);

int portNumber;

var port = Environment.GetEnvironmentVariable("PORT");
if (!int.TryParse(port, out portNumber))
{
    portNumber = 9003;
}

builder.WebHost.UseKestrel(options =>
{
    options.ListenAnyIP(portNumber);
});

var app = builder.Build();

var postgresHost = Environment.GetEnvironmentVariable("POSTGRES_HOST");
var postgresUser = Environment.GetEnvironmentVariable("POSTGRES_USER");
var postgresPassword = Environment.GetEnvironmentVariable("POSTGRES_PASSWORD");
var todosDb = Environment.GetEnvironmentVariable("TODOS_DB");
if (string.IsNullOrEmpty(postgresHost) || string.IsNullOrEmpty(postgresUser) || string.IsNullOrEmpty(postgresPassword) || string.IsNullOrEmpty(todosDb))
{
    throw new Exception("POSTGRES_HOST, POSTGRES_USER, POSTGRES_PASSWORD, TODOS_DB must be set");
}
string masterConnectionString = $"Host={postgresHost};Username={postgresUser};Password={postgresPassword};Database=postgres";
string connectionString = $"Host={postgresHost};Username={postgresUser};Password={postgresPassword};Database={todosDb}";

_ = Task.Run(() =>
{
    DatabaseHelper.WaitForDatabaseAvailability(masterConnectionString);
    DatabaseHelper.EnsureDatabaseExists(masterConnectionString, todosDb);
    DatabaseHelper.InitializeDatabase(connectionString);
});

// Health check
app.MapGet("/", () => 
{ 
    if (DatabaseHelper.IsDatabaseAvailable(connectionString))
    {
        return Results.Ok("OK");
    }

    return Results.Problem("Database not available");
});

app.MapGet("/todos", () =>
{
    var todos = DatabaseHelper.GetTodos(connectionString);
    return Results.Ok(todos);
});

app.MapPost("/todos", async (HttpContext httpContext) =>
{
    var form = await httpContext.Request.ReadFormAsync();
    var todo = form["todo"].ToString();
    Console.WriteLine($"Received todo: {todo}");
    if (todo.Length > 140)
    {
        Console.WriteLine($"ERROR: Received todo was too long");
        return Results.Redirect("/");
    }
    DatabaseHelper.AddTodo(connectionString, todo);
    return Results.Redirect("/");
});

app.Run();