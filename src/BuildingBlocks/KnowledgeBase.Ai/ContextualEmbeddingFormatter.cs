using Microsoft.Extensions.Options;

namespace KnowledgeBase.Ai;

public sealed class ContextualEmbeddingFormatter : IContextualEmbeddingFormatter
{
    private readonly ContextualEmbeddingOptions options;

    public ContextualEmbeddingFormatter(IOptions<ContextualEmbeddingOptions> options)
    {
        this.options = options.Value;
    }

    public string Format(ContextualEmbeddingRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (!options.Enabled)
        {
            return request.Content;
        }

        var section = string.IsNullOrWhiteSpace(request.SectionTitle)
            ? "General"
            : request.SectionTitle.Trim();

        var summary = string.IsNullOrWhiteSpace(request.DocumentSummary)
            ? "Not available."
            : request.DocumentSummary.Trim();

        return $"""
            Document: {request.DocumentName.Trim()}
            Summary: {summary}
            Section: {section}
            Part {request.ChunkIndex + 1} of {request.TotalChunks}

            {request.Content}
            """;
    }
}
