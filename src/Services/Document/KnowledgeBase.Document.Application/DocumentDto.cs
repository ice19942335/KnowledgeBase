using KnowledgeBase.Document.Domain;

namespace KnowledgeBase.Document.Application;

public sealed record DocumentDto(
    Guid Id,
    string Name,
    string FileName,
    string ContentType,
    DocumentStatus Status,
    int ChunkCount,
    string? Error,
    DateTime CreatedAt,
    DateTime? ProcessedAt)
{
    public static DocumentDto From(StoredDocument document) => new(
        document.Id,
        document.Name,
        document.FileName,
        document.ContentType,
        document.Status,
        document.ChunkCount,
        document.Error,
        document.CreatedAt,
        document.ProcessedAt);
}
