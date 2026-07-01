using KnowledgeBase.SharedKernel.Retrieval;

namespace KnowledgeBase.Application.Abstractions;

/// <summary>
/// Vector similarity search over document chunks.
/// </summary>
public interface IChunkSearchRepository
{
    Task<IReadOnlyList<ChunkMatch>> SearchVectorAsync(
        float[] queryEmbedding,
        int topK,
        CancellationToken cancellationToken);

    Task<IReadOnlyList<ChunkMatch>> SearchKeywordAsync(
        string query,
        int topK,
        CancellationToken cancellationToken);

    Task<IReadOnlyList<ChunkMatch>> GetChunksByLocatorsAsync(
        IReadOnlyList<ChunkLocator> locators,
        CancellationToken cancellationToken);
}
