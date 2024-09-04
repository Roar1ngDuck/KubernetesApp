using System.Globalization;

while (true) 
{
    Thread.Sleep(5000);
    var timestamp = DateTime.Now.ToString("u", CultureInfo.InvariantCulture);
    var randomString = Guid.NewGuid().ToString();
    File.WriteAllText("/usr/src/app/files/logoutput.txt", $"{timestamp}: {randomString}");
}