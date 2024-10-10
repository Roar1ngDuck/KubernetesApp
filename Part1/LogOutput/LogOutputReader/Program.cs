var builder = WebApplication.CreateBuilder(args);

builder.Logging.AddFilter("Microsoft.Hosting.Lifetime", LogLevel.Error);
builder.Logging.AddFilter("Microsoft.AspNetCore.Server.Kestrel", LogLevel.Error);

int portNumber;

var port = Environment.GetEnvironmentVariable("PORT");
if (!int.TryParse(port, out portNumber))
{
    portNumber = 9001;
}

builder.WebHost.UseKestrel(options =>
{
    options.ListenAnyIP(portNumber);
});

var app = builder.Build();

app.MapGet("/api/status", () => 
{
    var logtext = File.ReadAllText("/usr/src/app/files/logoutput.txt");
    var pingpongCount = File.ReadAllText("/usr/src/app/files/pingpong.txt");

    return Results.Text($"{logtext}\r\nPing / Pongs: {pingpongCount}");
});

app.Run();