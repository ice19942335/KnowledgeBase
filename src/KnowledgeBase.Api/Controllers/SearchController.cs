using KnowledgeBase.Api.Contracts;
using KnowledgeBase.Application.Search;
using Microsoft.AspNetCore.Mvc;

namespace KnowledgeBase.Api.Controllers;

[ApiController]
[Route("api/search")]
public sealed class SearchController : ControllerBase
{
    private readonly ISemanticSearchService searchService;

    public SearchController(ISemanticSearchService searchService)
    {
        this.searchService = searchService;
    }

    [HttpPost]
    [ProducesResponseType(typeof(IReadOnlyList<SearchResultDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<IReadOnlyList<SearchResultDto>>> Search(
        [FromBody] SearchRequest request,
        CancellationToken cancellationToken)
    {
        var results = await searchService.SearchAsync(request.Query, request.TopK, cancellationToken);
        return Ok(results);
    }
}
