using DotnetLLMRag;
using DotnetLLMRAG.Models;
using System.Text.Json;

class Program
{
    static async Task Main()
    {
        const string configurationFilePath = "appsettings.json";
        using FileStream stream = File.OpenRead(configurationFilePath);
        var configuration = await JsonSerializer.DeserializeAsync<Configuration>(stream) ?? throw new Exception("Configuration file is null");
        var utils = new Utils(configuration);

        var words = File.ReadAllText(configuration.TxtFilePath).Split(' ');

        const int chunkSize = 700;
        const int overlap = 300;

        var documents = new List<string>();

        for (int i = 0; i < words.Length; i += chunkSize - overlap)
        {
            var chunk = string.Join(" ", words.Skip(i).Take(chunkSize));
            documents.Add(chunk);
        }

        Console.WriteLine("Generating ... (this may take a while)");

        var vectorStore = utils.LoadVectorStore();

        foreach (var doc in documents)
        {
            var embedding = await utils.GenerateEmbeddingAsync(doc);
            if (embedding.Length > 0)
            {
                vectorStore[JsonSerializer.Serialize(embedding)] = doc;
            }
        }

        utils.SaveVectorStore(vectorStore);
        Console.WriteLine("AI is ready!");

        var previousQuery = "";
        bool firstQuery = false;

        while (true)
        {
            Console.Write("\nYou: ");
            var userQuery = Console.ReadLine();

            if (string.IsNullOrEmpty(userQuery))
            {
                Console.WriteLine("Prompt cannot be null or empty!\n");
                continue;
            }

            if (userQuery == configuration.CorrectionPrefix && !firstQuery && !string.IsNullOrEmpty(previousQuery))
            {
                Console.Write("Give me correct answer: ");
                var answerToSave = $"This is the correct answer to '{previousQuery}': " + Console.ReadLine();

                vectorStore = await utils.FeedByUser(answerToSave, vectorStore, chunkSize, overlap);
                continue;
            }

            var queryEmbedding = await utils.GenerateEmbeddingAsync(userQuery);
            if (queryEmbedding.Length == 0)
            {
                Console.WriteLine("Error generating query embedding!");
                continue;
            }

            var retrievedContext = utils.RetrieveRelevantText(queryEmbedding, vectorStore);
            var finalPrompt = $"Context: {retrievedContext}\n\nQuestion: {userQuery}";

            Console.Write("AI: ");
            await utils.GenerateResponseAsync(finalPrompt);

            previousQuery = userQuery;
        }

    }
}