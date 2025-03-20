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
        if(string.IsNullOrEmpty(uriText))
            throw new ArgumentNullException("Uri can not be null or empty");

        if(string.IsNullOrEmpty(embeddingModel))
            throw new ArgumentNullException("embeddingModel can not be null or empty");
        
        if(string.IsNullOrEmpty(respondingModel))
            throw new ArgumentNullException("respondingModel can not be null or empty");
        
        if(string.IsNullOrEmpty(vectorStoreFilePath))
            throw new ArgumentNullException("vectorStoreFilePath can not be null or empty");
        
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

    public void SaveVectorStore(Dictionary<float[], string> vectorStore)
    {
        var serializedData = vectorStore.ToDictionary(
            kvp => JsonSerializer.Serialize(kvp.Key),
            kvp => kvp.Value
        );
        File.WriteAllText(VectorStoreFilePath, JsonSerializer.Serialize(serializedData));
    }
    
    public Dictionary<float[], string> LoadVectorStore()
    {
        if (!File.Exists(VectorStoreFilePath)) return new Dictionary<float[], string>();

        var serializedData = JsonSerializer.Deserialize<Dictionary<string, string>>(File.ReadAllText(VectorStoreFilePath));

        return serializedData.ToDictionary(
            kvp => JsonSerializer.Deserialize<float[]>(kvp.Key),
            kvp => kvp.Value
        );
    }
    
    public async Task<float[]> GenerateEmbeddingAsync(string text)
    {
        try
        {
            var requestBody = JsonSerializer.Serialize(new { model = EmbeddingModel, prompt = text });
            var response = await _httpClient.PostAsync("api/embeddings", new StringContent(requestBody, Encoding.UTF8, "application/json"));
            var responseBody = await response.Content.ReadAsStringAsync();

            var json = JsonSerializer.Deserialize<EmbeddingResponse>(responseBody);
            return json?.Embedding ?? [];
        }
        catch (HttpRequestException ex)
        {
            await Console.Out.WriteLineAsync(ex.Message);
            throw new HttpRequestException(ex.Message);
        }
    }

    public string RetrieveRelevantText(float[] queryVector, Dictionary<float[], string> vectorStore)
    {
        double highestSimilarity = -1;
        var bestMatch = "I couldn't find relevant information in my knowledge base.";

        foreach (var entry in vectorStore)
        {
            var similarity = CosineSimilarity(queryVector, entry.Key);
            if (!(similarity > highestSimilarity)) continue;
            
            highestSimilarity = similarity;
            bestMatch = entry.Value;
        }

        return bestMatch;
    }

    private double CosineSimilarity(float[] vec1, float[] vec2)
    {
        double dotProduct = 0, mag1 = 0, mag2 = 0;

        for (int i = 0; i < vec1.Length; i++)
        {
            dotProduct += vec1[i] * vec2[i];
            mag1 += Math.Pow(vec1[i], 2);
            mag2 += Math.Pow(vec2[i], 2);
        }

        return dotProduct / (Math.Sqrt(mag1) * Math.Sqrt(mag2) + 1e-8);
    }

    public async Task GenerateResponseAsync(string prompt)
    {
        var requestBody = JsonSerializer.Serialize(new { model = RespondingModel, prompt });
        HttpResponseMessage response;

        try
        {
            response = await _httpClient.PostAsync($"{UriText}api/generate", new StringContent(requestBody, Encoding.UTF8, "application/json"));
        }
        catch (HttpRequestException ex)
        {
            await Console.Out.WriteLineAsync(ex.Message);
            throw new HttpRequestException(ex.Message);
        }

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

                if (chunk is not null)
                {
                    sb.Append(chunk.Response);

                    if (chunk.Done)
                    {
                        break;
                    }
                }
                else
                {
                    await Console.Out.WriteLineAsync("Error: Could not deserialize chunk.");
                }
            }
            catch (JsonException ex)
            {
                await Console.Out.WriteLineAsync($"Error deserializing chunk: {ex.Message}");
            }
        }

        await Console.Out.WriteLineAsync(sb.ToString());
    }
}
