namespace KnowledgeBase.Application.Abstractions;

/// <summary>
/// Vector similarity search over document chunks.
/// </summary>
public interface IChunkSearchRepository
{
    Task<IReadOnlyList<ChunkMatch>> SearchAsync(
        float[] queryEmbedding,
        int topK,
        CancellationToken cancellationToken);
}
