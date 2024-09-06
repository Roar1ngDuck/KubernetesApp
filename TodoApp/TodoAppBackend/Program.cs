var builder = WebApplication.CreateBuilder(args);

builder.Logging.AddFilter("Microsoft.Hosting.Lifetime", LogLevel.Error);
builder.Logging.AddFilter("Microsoft.AspNetCore.Server.Kestrel", LogLevel.Error);

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

var todos = new List<string>();

app.MapGet("/todos", () => 
{
    return Results.Ok(todos);
});

app.MapPost("/todos", async (HttpContext httpContext) => 
{
    var form = await httpContext.Request.ReadFormAsync();
    var todo = form["todo"].ToString();
    todos.Add(todo);
    return Results.Redirect("/");
});

app.Run();