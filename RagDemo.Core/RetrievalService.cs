using Azure;
using Azure.Search.Documents;
using Azure.Search.Documents.Models;

namespace RagDemo.Core;

public class RetrievalService
{
    private readonly SearchClient _searchClient;
    private readonly EmbeddingService _embeddingService;

    public RetrievalService(string searchEndpoint, string searchKey, string indexName, EmbeddingService embeddingService)
    {
        _searchClient = new SearchClient(new Uri(searchEndpoint), indexName, new AzureKeyCredential(searchKey));
        _embeddingService = embeddingService;
    }

    public async Task<List<DocumentChunk>> RetrieveAsync(string query, int topK = 3)
    {
        var queryEmbedding = await _embeddingService.GetEmbeddingAsync(query);

        var searchOptions = new SearchOptions
        {
            VectorSearch = new VectorSearchOptions
            {
                Queries = { new VectorizedQuery(queryEmbedding) { KNearestNeighborsCount = topK, Fields = { "Embedding" } } }
            },
            Size = topK
        };

        var response = await _searchClient.SearchAsync<DocumentChunk>(null, searchOptions);
        var results = new List<DocumentChunk>();

        await foreach (var result in response.Value.GetResultsAsync())
        {
            results.Add(result.Document);
        }

        return results;
    }
}