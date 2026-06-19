using KnowledgeBase.Auth;
using KnowledgeBase.Search.Api.Contracts;
using KnowledgeBase.Search.Application;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace KnowledgeBase.Search.Api.Controllers;

[ApiController]
[Route("api/search")]
[Authorize(Policy = AuthPolicies.Member)]
public sealed class SearchController : ControllerBase
{
    private readonly SemanticSearchService searchService;
    private readonly SearchExplorerService explorerService;

    public SearchController(
        SemanticSearchService searchService,
        SearchExplorerService explorerService)
    {
        this.searchService = searchService;
        this.explorerService = explorerService;
    }

    [HttpPost]
    [ProducesResponseType(typeof(IReadOnlyList<SearchResult>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<SearchResult>>> Search(
        [FromBody] SearchRequest request,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Query))
        {
            return BadRequest("Query cannot be empty.");
        }

        var results = await searchService.SearchAsync(request.Query, cancellationToken);
        return Ok(results);
    }

    [HttpGet("explorer")]
    [ProducesResponseType(typeof(SearchExplorerResult), StatusCodes.Status200OK)]
    public async Task<ActionResult<SearchExplorerResult>> Explorer(CancellationToken cancellationToken)
    {
        var explorer = await explorerService.GetExplorerAsync(cancellationToken);
        return Ok(explorer);
    }

    [HttpPost("trace")]
    [ProducesResponseType(typeof(SearchTraceResult), StatusCodes.Status200OK)]
    public async Task<ActionResult<SearchTraceResult>> Trace(
        [FromBody] SearchRequest request,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Query))
        {
            return BadRequest("Query cannot be empty.");
        }

        var trace = await searchService.SearchWithTraceAsync(request.Query, cancellationToken);
        return Ok(trace);
    }
}
