using Azure;
using Azure.AI.OpenAI;
using OpenAI.Chat;

namespace RagDemo.Core;

public class ChatService
{
    private readonly ChatClient _client;

    public ChatService(string endpoint, string key, string deployment)
    {
        var azureClient = new AzureOpenAIClient(new Uri(endpoint), new AzureKeyCredential(key));
        _client = azureClient.GetChatClient(deployment);
    }

    public async Task<string> AskAsync(string question, List<DocumentChunk> context)
    {
        var contextText = string.Join("\n\n", context.Select(c => c.Content));

        var messages = new List<ChatMessage>
        {
            new SystemChatMessage($"""
                                   You are a financial research assistant. Answer questions using only the context provided.
                                   If the answer is not in the context, say so.

                                   Context:
                                   {contextText}
                                   """),
            new UserChatMessage(question)
        };

        var response = await _client.CompleteChatAsync(messages);
        return response.Value.Content[0].Text;
    }
}