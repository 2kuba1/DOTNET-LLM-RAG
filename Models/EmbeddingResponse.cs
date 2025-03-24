using System.Text.Json.Serialization;

namespace DotnetLLMRag.Models;

public class EmbeddingResponse
{
    [JsonPropertyName("embedding")]
    public float[] Embedding { get; set; }
}