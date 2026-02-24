using Microsoft.Extensions.Configuration;
using RagDemo.Core;

var config = new ConfigurationBuilder()
    .AddJsonFile("appsettings.json")
    .Build();

var openAiEndpoint = config["AzureOpenAI:Endpoint"]!;
var openAiKey = config["AzureOpenAI:Key"]!;
var embeddingDeployment = config["AzureOpenAI:EmbeddingDeployment"]!;
var chatDeployment = config["AzureOpenAI:ChatDeployment"]!;
var searchEndpoint = config["AzureSearch:Endpoint"]!;
var searchKey = config["AzureSearch:Key"]!;
var indexName = config["AzureSearch:IndexName"]!;

var embeddingService = new EmbeddingService(openAiEndpoint, openAiKey, embeddingDeployment);
var ingester = new DocumentIngester(searchEndpoint, searchKey, indexName, embeddingService);
var pdfIngester = new PdfIngester(ingester);

// drop PDFs in a folder and point here
var pdfFolder = @"C:\RagDocs";
await pdfIngester.IngestFolderAsync(pdfFolder);

// then run the same Q&A as before
var retrievalService = new RetrievalService(searchEndpoint, searchKey, indexName, embeddingService);
var chatService = new ChatService(openAiEndpoint, openAiKey, chatDeployment);

Console.WriteLine("\nReady to answer questions. Type 'quit' to exit.\n");
while (true)
{
    Console.Write("Question: ");
    var question = Console.ReadLine();
    if (question?.ToLower() == "quit") break;

    var chunks = await retrievalService.RetrieveAsync(question!);
    var answer = await chatService.AskAsync(question!, chunks);
    Console.WriteLine($"\nAnswer: {answer}\n");
}