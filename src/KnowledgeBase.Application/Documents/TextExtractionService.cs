using KnowledgeBase.Application.Abstractions;

namespace KnowledgeBase.Application.Documents;

public sealed class TextExtractionService : ITextExtractionService
{
    private readonly IEnumerable<ITextExtractor> extractors;

    public TextExtractionService(IEnumerable<ITextExtractor> extractors)
    {
        this.extractors = extractors;
    }

    public Task<string> ExtractAsync(
        Stream content,
        string fileName,
        string contentType,
        CancellationToken cancellationToken)
    {
        var extractor = extractors.FirstOrDefault(candidate => candidate.CanExtract(fileName, contentType));

        if (extractor is null)
        {
            throw new NotSupportedException($"No text extractor is registered for file '{fileName}' ({contentType}).");
        }

        return extractor.ExtractAsync(content, fileName, cancellationToken);
    }
}
