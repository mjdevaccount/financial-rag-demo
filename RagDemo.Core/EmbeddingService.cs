using Azure;
using Azure.AI.OpenAI;
using OpenAI.Embeddings;

public class EmbeddingService
{
    private readonly EmbeddingClient _client;

    public EmbeddingService(string endpoint, string key, string deployment)
    {
        var azureClient = new AzureOpenAIClient(new Uri(endpoint), new AzureKeyCredential(key));
        _client = azureClient.GetEmbeddingClient(deployment);
    }

    public async Task<float[]> GetEmbeddingAsync(string text)
    {
        var result = await _client.GenerateEmbeddingAsync(text);
        return result.Value.ToFloats().ToArray();
    }
}