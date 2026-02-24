using Microsoft.OpenApi;
using RagDemo.Core;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Financial RAG API",
        Version = "v1",
        Description = "Financial document Q&A using Azure OpenAI and Azure AI Search"
    });
});

// Wire up services
var config = builder.Configuration;
var openAiEndpoint = config["AzureOpenAI:Endpoint"]!;
var openAiKey = config["AzureOpenAI:Key"]!;
var embeddingDeployment = config["AzureOpenAI:EmbeddingDeployment"]!;
var chatDeployment = config["AzureOpenAI:ChatDeployment"]!;
var searchEndpoint = config["AzureSearch:Endpoint"]!;
var searchKey = config["AzureSearch:Key"]!;
var indexName = config["AzureSearch:IndexName"]!;

var embeddingService = new EmbeddingService(openAiEndpoint, openAiKey, embeddingDeployment);

builder.Services.AddSingleton(embeddingService);
builder.Services.AddSingleton(new RetrievalService(searchEndpoint, searchKey, indexName, embeddingService));
builder.Services.AddSingleton(new ChatService(openAiEndpoint, openAiKey, chatDeployment));

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();
app.MapControllers();

app.Run();