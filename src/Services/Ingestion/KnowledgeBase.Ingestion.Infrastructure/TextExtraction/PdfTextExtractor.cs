using System.Text;
using KnowledgeBase.Ingestion.Application.Abstractions;
using UglyToad.PdfPig;

namespace KnowledgeBase.Ingestion.Infrastructure.TextExtraction;

public sealed class PdfTextExtractor : ITextExtractor
{
    public bool CanExtract(string fileName, string contentType)
    {
        return fileName.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase)
            || string.Equals(contentType, "application/pdf", StringComparison.OrdinalIgnoreCase);
    }

    public async Task<string> ExtractAsync(Stream content, string fileName, CancellationToken cancellationToken)
    {
        using var buffer = new MemoryStream();
        await content.CopyToAsync(buffer, cancellationToken);
        buffer.Position = 0;

        var builder = new StringBuilder();

        using var document = PdfDocument.Open(buffer);

        foreach (var page in document.GetPages())
        {
            cancellationToken.ThrowIfCancellationRequested();
            var words = page.GetWords().Select(word => word.Text);
            builder.AppendLine(string.Join(' ', words));
        }

        return builder.ToString();
    }
}
