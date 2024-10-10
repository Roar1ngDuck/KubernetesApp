using System.Text.Json.Serialization;

namespace DummySiteController;

public class DummySiteSpec
{
    [JsonPropertyName("website_url")]
    public string WebsiteUrl { get; set; }
}
