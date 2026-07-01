using KnowledgeBase.Search.Domain;
using KnowledgeBase.SharedKernel.Retrieval;
using Pgvector;

namespace KnowledgeBase.Search.Application;

public interface IChunkRepository
{
    Task AddRangeAsync(IEnumerable<SearchableChunk> chunks, CancellationToken cancellationToken);

    Task RemoveByDocumentAsync(Guid tenantId, Guid documentId, CancellationToken cancellationToken);

    Task<IReadOnlyList<SearchResult>> SearchVectorAsync(
        Guid tenantId,
        Vector queryEmbedding,
        int topK,
        CancellationToken cancellationToken);

    Task<IReadOnlyList<SearchResult>> SearchKeywordAsync(
        Guid tenantId,
        string query,
        int topK,
        CancellationToken cancellationToken);

    Task<IReadOnlyList<SearchResult>> GetChunksByLocatorsAsync(
        Guid tenantId,
        IReadOnlyList<ChunkLocator> locators,
        CancellationToken cancellationToken);

    Task<IReadOnlyList<ChunkDetailDto>> ListAsync(
        Guid tenantId,
        Guid? documentId,
        CancellationToken cancellationToken);

    Task SaveChangesAsync(CancellationToken cancellationToken);
}
