using Microsoft.Extensions.FileProviders;
using System.Globalization;

var builder = WebApplication.CreateBuilder(args);

builder.Logging.AddFilter("Microsoft.Hosting.Lifetime", LogLevel.Error);
builder.Logging.AddFilter("Microsoft.AspNetCore.Server.Kestrel", LogLevel.Error);

int portNumber;

var port = Environment.GetEnvironmentVariable("PORT");
if (!int.TryParse(port, out portNumber))
{
    portNumber = 9000;
}

builder.WebHost.UseKestrel(options =>
{
    options.ListenAnyIP(portNumber);
});

var app = builder.Build();

app.UseDefaultFiles(new DefaultFilesOptions
{
    FileProvider = new PhysicalFileProvider(Path.Combine(Directory.GetCurrentDirectory(), "public")),
    DefaultFileNames = new List<string> { "index.html" }
});

app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(Path.Combine(Directory.GetCurrentDirectory(), "public")),
    RequestPath = ""
});

app.MapGet("/randomimage", async () => 
{
    var cachedImagePath = "/usr/src/app/files/randomimage.jpg";
    var cachedImageTimestampPath = "/usr/src/app/files/randomimage-timestamp";
    var imageCacheTime = DateTime.MinValue;
    if (File.Exists(cachedImagePath) && File.Exists(cachedImageTimestampPath))
    {
        imageCacheTime = DateTime.Parse(File.ReadAllText(cachedImageTimestampPath));
    }

     if ((DateTime.Now - imageCacheTime) > TimeSpan.FromMinutes(60))
     {
        var httpClient = new HttpClient();
        var apiResponse = await httpClient.GetAsync(new Uri("https://picsum.photos/1200"));
        using (var fileStream = new FileStream(cachedImagePath, FileMode.OpenOrCreate, FileAccess.Write))
        {
            await apiResponse.Content.CopyToAsync(fileStream);
        }

        var timestamp = DateTime.Now.ToString("u", CultureInfo.InvariantCulture);
        File.WriteAllText(cachedImageTimestampPath, timestamp);
     }

    return Results.File(cachedImagePath, "image/jpeg");
});

await app.StartAsync();

Console.WriteLine($"Server started in port {portNumber}");

await app.WaitForShutdownAsync();