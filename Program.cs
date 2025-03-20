using DotnetLLMRag;

const string uri = "z";
const string embeddingModel = "";
const string respondingModel = "";
const string txtFilePath = "";
const string vectorStoreFilePath = "";

var utils = new Utils(uri, embeddingModel, respondingModel, vectorStoreFilePath);

var documents = File.ReadAllLines(txtFilePath);

Console.WriteLine("Generating ... (this my take a whileee)"); //depends on file size and embedding model

var vectorStore = utils.LoadVectorStore();

Console.WriteLine("Generating embeddings ...");
foreach (var doc in documents)
{
    var embedding = await utils.GenerateEmbeddingAsync(doc);
    vectorStore[embedding] = doc;
}

utils.SaveVectorStore(vectorStore);

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