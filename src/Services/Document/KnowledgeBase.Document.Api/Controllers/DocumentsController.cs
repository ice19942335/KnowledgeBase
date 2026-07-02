using KnowledgeBase.Auth;
using KnowledgeBase.Document.Application;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace KnowledgeBase.Document.Api.Controllers;

[ApiController]
[Route("api/documents")]
[Authorize]
public sealed class DocumentsController : ControllerBase
{
    private readonly DocumentAppService documentService;
    private readonly DocumentOptions documentOptions;

    public DocumentsController(DocumentAppService documentService, IOptions<DocumentOptions> documentOptions)
    {
        this.documentService = documentService;
        this.documentOptions = documentOptions.Value;
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

    [HttpPost("batch")]
    [Authorize(Policy = AuthPolicies.ContentManagement)]
    [RequestSizeLimit(50_000_000)]
    [ProducesResponseType(typeof(BatchUploadResultDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<BatchUploadResultDto>> UploadBatch(
        IList<IFormFile> files,
        CancellationToken cancellationToken)
    {
        if (files is null || files.Count == 0)
        {
            return BadRequest("At least one file is required.");
        }

        if (files.Count > documentOptions.MaxBatchFileCount)
        {
            return BadRequest($"A maximum of {documentOptions.MaxBatchFileCount} files can be uploaded at once.");
        }

        var requests = new List<UploadFileRequest>(files.Count);

        foreach (var file in files)
        {
            if (file is null)
            {
                continue;
            }

            var stream = file.OpenReadStream();
            requests.Add(new UploadFileRequest(
                stream,
                file.FileName,
                file.FileName,
                file.ContentType,
                file.Length));
        }

        if (requests.Count == 0)
        {
            return BadRequest("At least one non-empty file is required.");
        }

        try
        {
            var result = await documentService.UploadBatchAsync(requests, cancellationToken);
            return Created(string.Empty, result);
        }
        finally
        {
            foreach (var request in requests)
            {
                await request.Content.DisposeAsync();
            }
        }
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

    [HttpDelete]
    [Authorize(Policy = AuthPolicies.ContentManagement)]
    [ProducesResponseType(typeof(DeleteAllResultDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<DeleteAllResultDto>> DeleteAll(CancellationToken cancellationToken)
    {
        var result = await documentService.DeleteAllAsync(cancellationToken);
        return Ok(result);
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

    [HttpPost("{id:guid}/retry")]
    [Authorize(Policy = AuthPolicies.ContentManagement)]
    [ProducesResponseType(typeof(DocumentDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<DocumentDto>> Retry(Guid id, CancellationToken cancellationToken)
    {
        var result = await documentService.RetryProcessingAsync(id, cancellationToken);

        return result.Status switch
        {
            DocumentRetryStatus.NotFound => NotFound(),
            DocumentRetryStatus.NotRetryable => BadRequest("Only failed documents can be retried."),
            _ => Ok(result.Document),
        };
    }
}
