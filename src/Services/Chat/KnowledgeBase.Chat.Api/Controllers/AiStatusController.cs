using KnowledgeBase.Ai;
using KnowledgeBase.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace KnowledgeBase.Chat.Api.Controllers;

[ApiController]
[Route("api/chat/ai")]
[Authorize(Policy = AuthPolicies.Member)]
public sealed class AiStatusController : ControllerBase
{
    [HttpGet("status")]
    [ProducesResponseType(typeof(AiStatusDto), StatusCodes.Status200OK)]
    public ActionResult<AiStatusDto> GetStatus([FromServices] IAiAvailabilityState availabilityState)
    {
        return Ok(new AiStatusDto(availabilityState.IsConfigured, availabilityState.StatusMessage));
    }
}
