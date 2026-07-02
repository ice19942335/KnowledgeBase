namespace KnowledgeBase.Ai;

public interface IChunkReranker
{
    Task<RerankResult> RerankAsync(
        string query,
        IReadOnlyList<RankedChunkCandidate> candidates,
        int finalTopK,
        CancellationToken cancellationToken);
}

public sealed record RankedChunkCandidate(
    Guid DocumentId,
    string DocumentName,
    string? FileName,
    int ChunkIndex,
    string Content,
    double Score,
    int EmbeddingTokenCount = 0);
