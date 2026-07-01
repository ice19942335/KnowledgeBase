namespace KnowledgeBase.Ai;

public interface IDocumentSummaryGenerator
{
    Task<string> GenerateAsync(
        string documentName,
        string documentText,
        CancellationToken cancellationToken);
}
