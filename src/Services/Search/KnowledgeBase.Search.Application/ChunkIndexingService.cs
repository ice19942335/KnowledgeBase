using KnowledgeBase.Contracts;
using KnowledgeBase.Search.Domain;
using Pgvector;

namespace KnowledgeBase.Search.Application;

/// <summary>
/// Indexes chunks received from the Ingestion worker into the search store.
/// </summary>
public sealed class ChunkIndexingService
{
    private readonly IChunkRepository chunkRepository;

    public ChunkIndexingService(IChunkRepository chunkRepository)
    {
        this.chunkRepository = chunkRepository;
    }

    public async Task IndexAsync(ChunksGenerated message, CancellationToken cancellationToken)
    {
        await chunkRepository.RemoveByDocumentAsync(message.TenantId, message.DocumentId, cancellationToken);

        var entities = message.Chunks.Select(chunk => new SearchableChunk(
            message.DocumentId,
            message.TenantId,
            message.DocumentName,
            chunk.Index,
            chunk.Content,
            new Vector(chunk.Embedding),
            chunk.EmbeddingTokenCount)).ToList();

        await chunkRepository.AddRangeAsync(entities, cancellationToken);
        await chunkRepository.SaveChangesAsync(cancellationToken);
    }

    public async Task RemoveAsync(Guid tenantId, Guid documentId, CancellationToken cancellationToken)
    {
        await chunkRepository.RemoveByDocumentAsync(tenantId, documentId, cancellationToken);
        await chunkRepository.SaveChangesAsync(cancellationToken);
    }
}
