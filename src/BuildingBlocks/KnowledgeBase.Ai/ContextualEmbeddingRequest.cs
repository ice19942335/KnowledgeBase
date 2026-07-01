namespace KnowledgeBase.Ai;

public sealed record ContextualEmbeddingRequest(
    string DocumentName,
    string? FileName,
    int ChunkIndex,
    int TotalChunks,
    string Content,
    string? SectionTitle,
    string DocumentSummary);
