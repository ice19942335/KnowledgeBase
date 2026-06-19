using KnowledgeBase.Domain.Documents;

namespace KnowledgeBase.Application.Documents;

public sealed record DocumentDto(
    Guid Id,
    string Name,
    string FileName,
    string ContentType,
    DateTime UploadedAtUtc,
    DocumentStatus Status,
    int ChunkCount)
{
    public static DocumentDto FromDomain(Document document) => new(
        document.Id,
        document.Name,
        document.FileName,
        document.ContentType,
        document.UploadedAtUtc,
        document.Status,
        document.Chunks.Count);
}
