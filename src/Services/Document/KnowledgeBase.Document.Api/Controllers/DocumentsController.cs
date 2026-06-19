using KnowledgeBase.Auth;
using KnowledgeBase.Document.Application;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace KnowledgeBase.Document.Api.Controllers;

[ApiController]
[Route("api/documents")]
[Authorize]
public sealed class DocumentsController : ControllerBase
{
    private readonly DocumentAppService documentService;

    public DocumentsController(DocumentAppService documentService)
    {
        this.documentService = documentService;
    }

    [HttpPost]
    [Authorize(Policy = AuthPolicies.ContentManagement)]
    [RequestSizeLimit(50_000_000)]
    [ProducesResponseType(typeof(DocumentDto), StatusCodes.Status201Created)]
    public async Task<ActionResult<DocumentDto>> Upload(
        IFormFile file,
        [FromForm] string? name,
        CancellationToken cancellationToken)
    {
        if (file is null || file.Length == 0)
        {
            return BadRequest("A non-empty file is required.");
        }

        await using var stream = file.OpenReadStream();

        var document = await documentService.UploadAsync(
            stream,
            string.IsNullOrWhiteSpace(name) ? file.FileName : name,
            file.FileName,
            file.ContentType,
            cancellationToken);

        return CreatedAtAction(nameof(GetById), new { id = document.Id }, document);
    }

    [HttpGet]
    [Authorize(Policy = AuthPolicies.Member)]
    [ProducesResponseType(typeof(IReadOnlyList<DocumentDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<DocumentDto>>> List(CancellationToken cancellationToken)
    {
        var documents = await documentService.ListAsync(cancellationToken);
        return Ok(documents);
    }

    [HttpGet("{id:guid}")]
    [Authorize(Policy = AuthPolicies.Member)]
    [ProducesResponseType(typeof(DocumentDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<DocumentDto>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var document = await documentService.GetAsync(id, cancellationToken);
        return document is null ? NotFound() : Ok(document);
    }

    [HttpGet("{id:guid}/content")]
    [Authorize(Policy = AuthPolicies.Member)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetContent(
        Guid id,
        [FromQuery] bool download,
        CancellationToken cancellationToken)
    {
        var content = await documentService.GetContentAsync(id, cancellationToken);
        if (content is null)
        {
            return NotFound();
        }

        if (download)
        {
            return File(content.Content, content.ContentType, content.FileName);
        }

        return File(content.Content, content.ContentType);
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Policy = AuthPolicies.ContentManagement)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var deleted = await documentService.DeleteAsync(id, cancellationToken);
        return deleted ? NoContent() : NotFound();
    }
}
