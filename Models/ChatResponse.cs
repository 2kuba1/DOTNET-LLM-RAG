using System.Text.Json.Serialization;

namespace DotnetLLMRag.Models;

public class ChatResponse
{
    [JsonPropertyName("id")]
    public string Id { get; set; }
    [JsonPropertyName("object")]
    public string Object { get; set; }
    [JsonPropertyName("created")]
    public int Created { get; set; }
    [JsonPropertyName("model")]
    public string Model { get; set; }
    [JsonPropertyName("choices")]
    public Choices[] Choices { get; set; }
    [JsonPropertyName("usage")]
    public Usage Usage { get; set; }

    public string SystemFingerPrint { get; set; }
    public XGroq XGroq { get; set; }
}

public class Choices
{
    [JsonPropertyName("index")]
    public int Index { get; set; }
    [JsonPropertyName("message")]
    public Message Message { get; set; }
    [JsonPropertyName("logprobs")]
    public string? Logprobs { get; set; }
    [JsonPropertyName("finish_reason")]
    public string FinishReason { get; set; }
}

public class Message
{
    [JsonPropertyName("role")]
    public string Role { get; set; }
    [JsonPropertyName("content")]
    public string Content { get; set; }
}

public class Usage
{
    public double QueueTime { get; set; }
    public int PromptTokens { get; set; }
    public double PromptTime { get; set; }
    public int CompletionTime { get; set; }
    public int TotalTokens { get; set; }
    public double TotalTime { get; set; }
}

public class XGroq
{
    public string Id { get; set; }
}