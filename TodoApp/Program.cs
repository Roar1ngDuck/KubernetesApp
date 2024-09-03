var builder = WebApplication.CreateBuilder(args);

builder.Logging.AddFilter("Microsoft.Hosting.Lifetime", LogLevel.None);
builder.Logging.AddFilter("Microsoft.AspNetCore.Server.Kestrel", LogLevel.None);

int portNumber;

var port = Environment.GetEnvironmentVariable("PORT");
if (!int.TryParse(port, out portNumber))
{
    portNumber = 8080;
}

builder.WebHost.UseKestrel(options =>
{
    options.ListenAnyIP(portNumber);
});

var app = builder.Build();

await app.StartAsync();

System.Console.WriteLine($"Server started in port {portNumber}");

await app.WaitForShutdownAsync();