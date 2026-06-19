using KnowledgeBase.Auth;
using KnowledgeBase.Chat.Api.Contracts;
using KnowledgeBase.Chat.Application;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace KnowledgeBase.Chat.Api.Controllers;

[ApiController]
[Route("api/chat")]
[Authorize(Policy = AuthPolicies.Member)]
public sealed class ChatController : ControllerBase
{
    private readonly ChatAppService chatService;

    public ChatController(ChatAppService chatService)
    {
        this.chatService = chatService;
    }

    [HttpPost]
    [ProducesResponseType(typeof(ChatAnswerDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<ChatAnswerDto>> Ask(
        [FromBody] ChatRequest request,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Question))
        {
            return BadRequest("Question cannot be empty.");
        }

        var conversationId = request.ConversationId ?? Guid.NewGuid();

        var answer = await chatService.AskAsync(conversationId, request.Question, cancellationToken);
        return Ok(answer);
    }

    [HttpPost("trace")]
    [ProducesResponseType(typeof(ChatTraceAnswerDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<ChatTraceAnswerDto>> AskWithTrace(
        [FromBody] ChatRequest request,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Question))
        {
            return BadRequest("Question cannot be empty.");
        }

        var conversationId = request.ConversationId ?? Guid.NewGuid();
        var trace = await chatService.AskWithTraceAsync(conversationId, request.Question, cancellationToken);
        return Ok(trace);
    }

    [HttpGet("conversations")]
    [ProducesResponseType(typeof(IReadOnlyList<ConversationDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<ConversationDto>>> ListConversations(
        CancellationToken cancellationToken)
    {
        var conversations = await chatService.ListConversationsAsync(cancellationToken);
        return Ok(conversations);
    }

    [HttpGet("conversations/{id:guid}")]
    [ProducesResponseType(typeof(ConversationDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ConversationDetailDto>> GetConversation(
        Guid id,
        CancellationToken cancellationToken)
    {
        var conversation = await chatService.GetConversationAsync(id, cancellationToken);
        return conversation is null ? NotFound() : Ok(conversation);
    }
}
