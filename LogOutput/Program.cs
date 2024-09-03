using System.Globalization;

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

var randomString = Guid.NewGuid().ToString();

_ = Task.Run(() => 
{
   while (true) 
   {
        Thread.Sleep(5000);
        var timestamp = DateTime.Now.ToString("u", CultureInfo.InvariantCulture);
        Console.WriteLine($"{timestamp}: {randomString}");
   }
});

app.MapGet("/api/status", () => 
{
    var timestamp = DateTime.Now.ToString("u", CultureInfo.InvariantCulture);

    return Results.Text($"{timestamp}: {randomString}");
});

app.Run();