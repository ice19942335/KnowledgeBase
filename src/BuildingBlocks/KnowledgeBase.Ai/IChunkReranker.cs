namespace KnowledgeBase.Ai;

public interface IChunkReranker
{
    Task<IReadOnlyList<RankedChunkCandidate>> RerankAsync(
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
    double Score);
