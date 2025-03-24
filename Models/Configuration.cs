using System.Text.Json.Serialization;

namespace DotnetLLMRAG.Models;

public class Configuration
{
    [JsonPropertyName("uri")]
    public string Uri { get; set; }
    [JsonPropertyName("embeddingModel")]
    public string EmbeddingModel { get; set; }
    [JsonPropertyName("respondingModel")]
    public string RespondingModel { get; set; }
    [JsonPropertyName("txtFilePath")]
    public string TxtFilePath { get; set; }
    [JsonPropertyName("vectorStoreFilePath")]
    public string VectorStoreFilePath { get; set; }
}
