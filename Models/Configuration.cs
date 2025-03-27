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
    [JsonPropertyName("groq_apikey")]
    public string GroqApiKey { get; set; }
    [JsonPropertyName("correctionPrefix")]
    public string CorrectionPrefix { get; set; }
    [JsonPropertyName("api-url")]
    public string ApiUrl { get; set; }
}
