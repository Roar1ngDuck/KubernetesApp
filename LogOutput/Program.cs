using System.Globalization;

var randomString = Guid.NewGuid().ToString();

while (true)
{
    var timestamp = DateTime.Now.ToString("u", CultureInfo.InvariantCulture);
    Console.WriteLine($"{timestamp}: {randomString}");
    Thread.Sleep(5000);
}