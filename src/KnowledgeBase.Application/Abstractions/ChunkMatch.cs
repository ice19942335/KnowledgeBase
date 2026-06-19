namespace KnowledgeBase.Application.Abstractions;

/// <summary>
/// Read model returned by a vector search: a matched chunk plus its similarity score.
/// </summary>
public sealed record ChunkMatch(
    Guid DocumentId,
    string DocumentName,
    string FileName,
    int ChunkIndex,
    string Content,
    double Score);
