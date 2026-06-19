using KnowledgeBase.Application.Abstractions;

namespace KnowledgeBase.Infrastructure.TextExtraction;

public sealed class PlainTextExtractor : ITextExtractor
{
    private static readonly string[] SupportedExtensions = [".txt", ".md", ".text"];

    public bool CanExtract(string fileName, string contentType)
    {
        return SupportedExtensions.Any(extension => fileName.EndsWith(extension, StringComparison.OrdinalIgnoreCase))
            || contentType.StartsWith("text/", StringComparison.OrdinalIgnoreCase);
    }

    public async Task<string> ExtractAsync(Stream content, string fileName, CancellationToken cancellationToken)
    {
        using var reader = new StreamReader(content, leaveOpen: true);
        return await reader.ReadToEndAsync(cancellationToken);
    }
}
