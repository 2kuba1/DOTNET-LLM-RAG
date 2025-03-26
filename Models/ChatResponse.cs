using System.Text.Json.Serialization;

namespace DotnetLLMRag.Models;

public class ChatResponse
{
    public string Id { get; set; }
    public string Object { get; set; }
    public int Created { get; set; }
    public string Model { get; set; }
    public Choices Choices { get; set; }
    public Usage Usage { get; set; }
    public string SystemFingerPrint { get; set; }
    public XGroq XGroq { get; set; }
}

public class Choices
{
    public int Index { get; set; }
    public Message Message { get; set; }
    public string? Logprobs { get; set; }
    public string FinishReason { get; set; }
}

public class Message
{
    public string Role { get; set; }
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