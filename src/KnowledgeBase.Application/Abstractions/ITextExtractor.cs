namespace KnowledgeBase.Application.Abstractions;

/// <summary>
/// Extracts plain text from an uploaded file of a specific type.
/// Implementations are selected by <see cref="CanExtract"/>.
/// </summary>
public interface ITextExtractor
{
    bool CanExtract(string fileName, string contentType);

    Task<string> ExtractAsync(Stream content, string fileName, CancellationToken cancellationToken);
}
