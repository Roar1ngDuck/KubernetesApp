using System.Text.Json;
using NATS.Client.Core;
using System.Text;

var natsUrl = Environment.GetEnvironmentVariable("NATS_URL");

if (string.IsNullOrEmpty(natsUrl))
{
    throw new Exception("NATS_URL must be set");
}

var env = Environment.GetEnvironmentVariable("ENVIRONMENT");
env = string.IsNullOrEmpty(env) ? "DEV" : env;

Console.WriteLine($"Environment: {env}");

var discordWebhookUrl = Environment.GetEnvironmentVariable("DISCORD_WEBHOOK_URL");

if (string.IsNullOrEmpty(discordWebhookUrl) && env == "PROD")
{
    throw new Exception("DISCORD_WEBHOOK_URL must be set");
}

var hostname = System.Net.Dns.GetHostName();

var opts = new NatsOpts { Url = natsUrl };
await using var nats = new NatsConnection(opts);
await using var sub = await nats.SubscribeCoreAsync<string>("todo.>", queueGroup: "todoapp");

HttpClient client = new HttpClient();

Console.WriteLine("Connected to NATS");

await foreach (var msg in sub.Msgs.ReadAllAsync())
{
    var doc = JsonDocument.Parse(msg.Data);
    var formatted = JsonSerializer.Serialize(doc.RootElement, new JsonSerializerOptions { WriteIndented = true });
    Console.WriteLine(msg.Subject);
    Console.WriteLine(formatted);

    if (env != "PROD")
    {
        continue;
    }

    formatted = $"```json\n{formatted}```";

    var status = msg.Subject switch
    {
        "todo.created" => "A todo was created:",
        "todo.updated" => "A todo was updated:",
        "todo.deleted" => "A todo was deleted:",
    };

    var payload = new
    {
        content = $"{status}\n{formatted}\nBroadcasted by {hostname}"
    };

    var jsonPayload = JsonSerializer.Serialize(payload);
    var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

    var response = await client.PostAsync(discordWebhookUrl, content);

    if (!response.IsSuccessStatusCode)
    {
        Console.WriteLine($"Error: {response.StatusCode}: {await response.Content.ReadAsStringAsync()}");
    }
}