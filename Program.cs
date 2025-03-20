using DotnetLLMRag;

const string uri = "http://127.0.0.1:11436/";
const string embeddingModel = "avr/sfr-embedding-mistral:latest";
const string respondingModel = "deepseek-r1:1.5b";
const string txtFilePath = "xd.txt";

var utils = new Utils(uri, embeddingModel, respondingModel);

var documents = File.ReadAllLines(txtFilePath);

var vectorStore = new Dictionary<float[], string>();

Console.WriteLine("Generating ... (this my take a whileee)"); //depends on file size and embedding model
foreach (var doc in documents)
{
    var embedding = await utils.GenerateEmbeddingAsync(doc);
    vectorStore[embedding] = doc;
}

Console.WriteLine("AI is ready!");

while (true)
{
    Console.Write("\nYou: ");
    var userQuery = Console.ReadLine();

    if (string.IsNullOrEmpty(userQuery))
    {
        Console.WriteLine("Prompt can not be null or empty!\n");
        continue;
    }

    var queryEmbedding = await utils.GenerateEmbeddingAsync(userQuery);

    var retrievedContext = utils.RetrieveRelevantText(queryEmbedding, vectorStore);

    var finalPrompt = $"Context: {retrievedContext}\n\nQuestion: {userQuery}";

    Console.Write("AI: ");
    await utils.GenerateResponseAsync(finalPrompt);
}