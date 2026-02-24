using Azure;
using Azure.Search.Documents.Indexes;
using Azure.Search.Documents.Indexes.Models;

namespace RagDemo.Core;

public class SearchIndexService
{
    private readonly SearchIndexClient _indexClient;
    private readonly string _indexName;

    public SearchIndexService(string endpoint, string key, string indexName)
    {
        _indexClient = new SearchIndexClient(new Uri(endpoint), new AzureKeyCredential(key));
        _indexName = indexName;
    }

    public async Task EnsureIndexExistsAsync()
    {
        var fields = new FieldBuilder().Build(typeof(DocumentChunk));

        // Override embedding field to be a vector field
        var index = new SearchIndex(_indexName)
        {
            Fields = fields,
            VectorSearch = new VectorSearch()
        };

        index.VectorSearch.Profiles.Add(new VectorSearchProfile("default-profile", "default-hnsw"));
        index.VectorSearch.Algorithms.Add(new HnswAlgorithmConfiguration("default-hnsw"));

        // Mark the Embedding field as a vector field
        var embeddingField = index.Fields.First(f => f.Name == "Embedding");
        // Replace with SearchField explicitly
        index.Fields.Remove(embeddingField);
        index.Fields.Add(new VectorSearchField("Embedding", 1536, "default-profile"));

        await _indexClient.CreateOrUpdateIndexAsync(index);
        Console.WriteLine($"Index '{_indexName}' ready.");
    }
}