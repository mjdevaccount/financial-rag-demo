using Microsoft.AspNetCore.Mvc;
using RagDemo.Core;

[ApiController]
[Route("api/[controller]")]
public class RagController : ControllerBase
{
    private readonly RetrievalService _retrievalService;
    private readonly ChatService _chatService;

    public RagController(RetrievalService retrievalService, ChatService chatService)
    {
        _retrievalService = retrievalService;
        _chatService = chatService;
    }

    [HttpPost("ask")]
    public async Task<ActionResult<RagResponse>> Ask([FromBody] RagRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Question))
            return BadRequest("Question is required.");

        var chunks = await _retrievalService.RetrieveAsync(request.Question);

        if (!chunks.Any())
            return Ok(new RagResponse { Answer = "No relevant documents found.", Sources = [] });

        var answer = await _chatService.AskAsync(request.Question, chunks);

        return Ok(new RagResponse
        {
            Answer = answer,
            Sources = chunks.Select(c => c.Source).Distinct().ToList()
        });
    }
}

public class RagRequest
{
    public string Question { get; set; } = string.Empty;
}

public class RagResponse
{
    public string Answer { get; set; } = string.Empty;
    public List<string> Sources { get; set; } = [];
}