using Azure;
using Azure.Search.Documents;

public class DocumentIngester
{
    private readonly SearchClient _searchClient;
    private readonly EmbeddingService _embeddingService;
    private const int ChunkSize = 512;
    private const int ChunkOverlap = 50;

    public DocumentIngester(string searchEndpoint, string searchKey, string indexName, EmbeddingService embeddingService)
    {
        _searchClient = new SearchClient(new Uri(searchEndpoint), indexName, new AzureKeyCredential(searchKey));
        _embeddingService = embeddingService;
    }

    public async Task IngestTextAsync(string text, string source)
    {
        var chunks = ChunkText(text, source);

        foreach (var chunk in chunks)
        {
            chunk.Embedding = await _embeddingService.GetEmbeddingAsync(chunk.Content);
        }

        await _searchClient.UploadDocumentsAsync(chunks);
        Console.WriteLine($"Ingested {chunks.Count} chunks from '{source}'");
    }

    private List<DocumentChunk> ChunkText(string text, string source)
    {
        var words = text.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        var chunks = new List<DocumentChunk>();
        int index = 0;
        int chunkIndex = 0;

        while (index < words.Length)
        {
            var chunkWords = words.Skip(index).Take(ChunkSize).ToArray();
            chunks.Add(new DocumentChunk
            {
                Id = $"{source}-{chunkIndex}",
                Content = string.Join(" ", chunkWords),
                Source = source,
                ChunkIndex = chunkIndex
            });

            index += ChunkSize - ChunkOverlap;
            chunkIndex++;
        }

        return chunks;
    }
}