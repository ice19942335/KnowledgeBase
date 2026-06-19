namespace KnowledgeBase.Search.Application;

public sealed record SearchResult(
    Guid DocumentId,
    string DocumentName,
    int ChunkIndex,
    string Content,
    double Score);
