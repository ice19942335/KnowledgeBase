namespace KnowledgeBase.Search.Application;

public sealed record ChunkDetailDto(
    Guid Id,
    Guid DocumentId,
    string DocumentName,
    int ChunkIndex,
    string Content,
    DateTime IndexedAt);

public sealed record DocumentChunksGroupDto(
    Guid DocumentId,
    string DocumentName,
    IReadOnlyList<ChunkDetailDto> Chunks);

public sealed record SearchExplorerResult(
    IReadOnlyList<DocumentChunksGroupDto> Documents,
    int TotalChunks);

public sealed record SearchTraceResult(
    string Query,
    IReadOnlyList<SearchResult> Results,
    IReadOnlyList<SharedKernel.Diagnostics.PipelineTraceStep> Steps,
    long TotalDurationMs);
