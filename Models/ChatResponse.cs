using System.Text.Json.Serialization;

namespace DotnetLLMRag.Models;

public class ChatResponse
{
    [JsonPropertyName("model")]
    public string Model { get; set; }
    [JsonPropertyName("created_at")]
    public DateTime CreatedAt { get; set; }
    [JsonPropertyName("response")]
    public string Response { get; set; }
    [JsonPropertyName("done")]
    public bool Done { get; set; }
}