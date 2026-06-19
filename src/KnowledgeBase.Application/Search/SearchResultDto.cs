namespace KnowledgeBase.Application.Search;

public sealed record SearchResultDto(
    Guid DocumentId,
    string DocumentName,
    string FileName,
    int ChunkIndex,
    string Content,
    double Score);
