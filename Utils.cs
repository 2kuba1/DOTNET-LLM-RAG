using System.Text.Json;
using System.Text;
using DotnetLLMRag.Models;

namespace DotnetLLMRag;

public class Utils
{
    private string UriText { get; }
    private string EmbeddingModel { get; }
    private string RespondingModel { get; }
    private string VectorStoreFilePath { get; }

    private readonly HttpClient _httpClient;

    public Utils(string uriText, string embeddingModel, string respondingModel, string vectorStoreFilePath)
    {
        if (string.IsNullOrEmpty(uriText))
            throw new ArgumentNullException(nameof(uriText), "URI cannot be null or empty");

        if (string.IsNullOrEmpty(embeddingModel))
            throw new ArgumentNullException(nameof(embeddingModel), "Embedding model cannot be null or empty");

        if (string.IsNullOrEmpty(respondingModel))
            throw new ArgumentNullException(nameof(respondingModel), "Responding model cannot be null or empty");

        if (string.IsNullOrEmpty(vectorStoreFilePath))
            throw new ArgumentNullException(nameof(vectorStoreFilePath), "Vector store file path cannot be null or empty");

        _httpClient = new HttpClient
        {
            BaseAddress = new Uri(uriText),
            Timeout = TimeSpan.FromMinutes(5)
        };

        VectorStoreFilePath = vectorStoreFilePath;
        UriText = uriText;
        EmbeddingModel = embeddingModel;
        RespondingModel = respondingModel;
    }

    public void SaveVectorStore(Dictionary<string, string> vectorStore)
    {
        if (vectorStore == null || vectorStore.Count == 0)
        {
            Console.WriteLine("Warning: Attempted to save an empty vector store.");
        }

        try
        {
            var json = JsonSerializer.Serialize(vectorStore, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(VectorStoreFilePath, json);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error writing vector store JSON: {ex.Message}");
        }
    }


    public Dictionary<string, string> LoadVectorStore()
    {
        if (!File.Exists(VectorStoreFilePath))
        {
            Console.WriteLine("Vector store file not found. Returning an empty dictionary.");
            return new Dictionary<string, string>();
        }

        var json = File.ReadAllText(VectorStoreFilePath);

        if (string.IsNullOrWhiteSpace(json))
        {
            Console.WriteLine("Vector store file is empty. Returning an empty dictionary.");
            return new Dictionary<string, string>();
        }

        try
        {
            return JsonSerializer.Deserialize<Dictionary<string, string>>(json) ?? new Dictionary<string, string>();
        }
        catch (JsonException ex)
        {
            Console.WriteLine($"Error deserializing vector store JSON: {ex.Message}");
            return new Dictionary<string, string>();
        }
    }
    public async Task<float[]> GenerateEmbeddingAsync(string text)
    {
        try
        {
            var requestBody = JsonSerializer.Serialize(new { model = EmbeddingModel, prompt = text });
            var response = await _httpClient.PostAsync("api/embeddings", new StringContent(requestBody, Encoding.UTF8, "application/json"));

            response.EnsureSuccessStatusCode();

            var responseBody = await response.Content.ReadAsStringAsync();
            var json = JsonSerializer.Deserialize<EmbeddingResponse>(responseBody);

            return json?.Embedding ?? Array.Empty<float>();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error generating embedding: {ex.Message}");
            return Array.Empty<float>();
        }
    }

    public string RetrieveRelevantText(float[] queryVector, Dictionary<string, string> vectorStore)
    {
        double highestSimilarity = -1;
        var bestMatch = "I couldn't find relevant information in my knowledge base.";

        foreach (var entry in vectorStore)
        {
            var vector = JsonSerializer.Deserialize<float[]>(entry.Key);
            if (vector == null || vector.Length != queryVector.Length) continue;

            var similarity = CosineSimilarity(queryVector, vector);
            if (similarity > highestSimilarity)
            {
                highestSimilarity = similarity;
                bestMatch = entry.Value;
            }
        }

        return bestMatch;
    }

    private double CosineSimilarity(float[] vec1, float[] vec2)
    {
        if (vec1.Length != vec2.Length) return -1;

        double dotProduct = 0, mag1 = 0, mag2 = 0;

        for (int i = 0; i < vec1.Length; i++)
        {
            dotProduct += vec1[i] * vec2[i];
            mag1 += vec1[i] * vec1[i];
            mag2 += vec2[i] * vec2[i];
        }

        return dotProduct / (Math.Sqrt(mag1) * Math.Sqrt(mag2) + 1e-8);
    }

    public async Task GenerateResponseAsync(string prompt)
    {
        var requestBody = JsonSerializer.Serialize(new { model = RespondingModel, prompt });

        try
        {
            var response = await _httpClient.PostAsync($"{UriText}api/generate", new StringContent(requestBody, Encoding.UTF8, "application/json"));

            if (!response.IsSuccessStatusCode)
            {
                Console.WriteLine($"Error: {response.StatusCode}");
                return;
            }

            var stream = await response.Content.ReadAsStreamAsync();
            var sb = new StringBuilder();

            using var reader = new StreamReader(stream);

            while (await reader.ReadLineAsync() is { } line)
            {
                if (string.IsNullOrWhiteSpace(line)) continue;

                try
                {
                    var chunk = JsonSerializer.Deserialize<ChatResponse>(line);
                    if (chunk != null)
                    {
                        sb.Append(chunk.Response);
                        if (chunk.Done) break;
                    }
                }
                catch (JsonException ex)
                {
                    Console.WriteLine($"JSON Error: {ex.Message}");
                }
            }

            Console.WriteLine(sb.ToString());
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error generating response: {ex.Message}");
        }
    }
}
