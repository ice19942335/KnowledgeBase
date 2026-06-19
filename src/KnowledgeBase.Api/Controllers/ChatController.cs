using KnowledgeBase.Api.Contracts;
using KnowledgeBase.Application.Chat;
using Microsoft.AspNetCore.Mvc;

namespace KnowledgeBase.Api.Controllers;

[ApiController]
[Route("api/chat")]
public sealed class ChatController : ControllerBase
{
    private readonly IRagChatService chatService;

    public ChatController(IRagChatService chatService)
    {
        this.chatService = chatService;
    }

    [HttpPost]
    [ProducesResponseType(typeof(ChatAnswerDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ChatAnswerDto>> Ask(
        [FromBody] ChatRequest request,
        CancellationToken cancellationToken)
    {
        var answer = await chatService.AskAsync(request.Question, cancellationToken);
        return Ok(answer);
    }
}
