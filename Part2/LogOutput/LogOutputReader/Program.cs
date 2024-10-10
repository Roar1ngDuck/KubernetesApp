var builder = WebApplication.CreateBuilder(args);

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
    var pingpongCount = GetPingPongCount();
    var configInfoText = File.ReadAllText(@"/usr/src/app/config/information.txt");
    var message = Environment.GetEnvironmentVariable("MESSAGE");

    return Results.Text($"file content: {configInfoText}env variable: MESSAGE={message}\r\n{logtext}\r\nPing / Pongs: {pingpongCount}");
});

int GetPingPongCount()
{
    var client = new HttpClient();
    var response = client.GetAsync("http://dwk-pingpongapp-svc:10002/pingpongcount").Result;
    var responseString = response.Content.ReadAsStringAsync().Result;
    return int.Parse(responseString);
}

app.Run();