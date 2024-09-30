using NATS.Client.Core;
using System.Text.Json;
using TodoAppBackend;

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
var natsUrl = Environment.GetEnvironmentVariable("NATS_URL");
if (string.IsNullOrEmpty(postgresHost) || string.IsNullOrEmpty(postgresUser) || string.IsNullOrEmpty(postgresPassword) || string.IsNullOrEmpty(todosDb) || string.IsNullOrEmpty(natsUrl))
{
    throw new Exception("POSTGRES_HOST, POSTGRES_USER, POSTGRES_PASSWORD, TODOS_DB, NATS_URL must be set");
}
var databaseHelper = new DatabaseHelper(postgresHost, todosDb, postgresUser, postgresPassword);

_ = Task.Run(async () =>
{
    await databaseHelper.WaitForDatabaseAvailabilityAsync();
    await databaseHelper.InitializeDatabaseAsync();
});

var opts = new NatsOpts { Url = natsUrl };
await using var nats = new NatsConnection(opts);

// Health check
app.MapGet("/", async () => 
{ 
    if (await databaseHelper.IsDatabaseAvailableAsync())
    {
        return Results.Ok("OK");
    }

    return Results.Problem("Database not available");
});

app.MapGet("/todos", async () =>
{
    var todos = await databaseHelper.GetTodosAsync();
    return Results.Ok(todos);
});

app.MapPost("/todos", async (HttpContext httpContext) =>
{
    var form = await httpContext.Request.ReadFormAsync();
    var todoText = form["todo"].ToString();
    Console.WriteLine($"Received todo: {todoText}");
    if (todoText.Length > 140)
    {
        Console.WriteLine($"ERROR: Received todo was too long");
        return Results.Redirect("/");
    }
    var todo = await databaseHelper.AddTodoAsync(todoText);
    await nats.PublishAsync("todo.created", JsonSerializer.Serialize(todo));
    return Results.Redirect("/");
});

app.MapPut("/todos/{id}", async (HttpContext httpContext, int id) =>
{
    var todo = await databaseHelper.MarkTodoAsDoneAsync(id);
    await nats.PublishAsync("todo.updated", JsonSerializer.Serialize(todo));
    return Results.Ok();
});

app.MapDelete("/todos/{id}", async (HttpContext httpContext, int id) =>
{
    var todo = await databaseHelper.GetTodoByIdAsync(id);
    await databaseHelper.DeleteTodoAsync(id);
    await nats.PublishAsync("todo.deleted", JsonSerializer.Serialize(todo));
    return Results.Ok();
});

app.Run();