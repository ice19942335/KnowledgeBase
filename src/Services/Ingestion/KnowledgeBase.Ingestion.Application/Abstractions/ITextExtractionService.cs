namespace KnowledgeBase.Ingestion.Application.Abstractions;

/// <summary>
/// Resolves the appropriate <see cref="ITextExtractor"/> for a file and extracts its text.
/// </summary>
public interface ITextExtractionService
{
    Task<string> ExtractAsync(Stream content, string fileName, string contentType, CancellationToken cancellationToken);
}
