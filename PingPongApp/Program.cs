var builder = WebApplication.CreateBuilder(args);

builder.Logging.AddFilter("Microsoft.Hosting.Lifetime", LogLevel.Error);
builder.Logging.AddFilter("Microsoft.AspNetCore.Server.Kestrel", LogLevel.Error);

int portNumber;

var port = Environment.GetEnvironmentVariable("PORT");
if (!int.TryParse(port, out portNumber))
{
    portNumber = 9002;
}

builder.WebHost.UseKestrel(options =>
{
    options.ListenAnyIP(portNumber);
});

var app = builder.Build();

var counter = 0;
app.MapGet("/pingpong", () => 
{
    return Results.Text($"pong {counter++}");
});

app.Run();