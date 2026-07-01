namespace KnowledgeBase.SharedKernel.Retrieval;

public sealed record RankedChunkHit(
    Guid DocumentId,
    string DocumentName,
    string? FileName,
    int ChunkIndex,
    string Content,
    double Score);
