using DotnetLLMRag;
using System.Text.Json;

class Program
{
    static async Task Main()
    {
        const string uri = "http://127.0.0.1:11436/";
        const string embeddingModel = "avr/sfr-embedding-mistral:latest";
        const string respondingModel = "deepseek-r1:1.5b";
        const string txtFilePath = "base.txt";
        const string vectorStoreFilePath = "vector.json";

        var utils = new Utils(uri, embeddingModel, respondingModel, vectorStoreFilePath);

        var words = File.ReadAllText(txtFilePath).Split(' ');

        const int chunkSize = 500;
        const int overlap = 100;

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

        while (true)
        {
            Console.Write("\nYou: ");
            var userQuery = Console.ReadLine();

            if (string.IsNullOrEmpty(userQuery))
            {
                Console.WriteLine("Prompt cannot be null or empty!\n");
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
        }
    }
}