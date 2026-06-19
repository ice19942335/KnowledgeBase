using Pgvector;

namespace KnowledgeBase.Search.Domain;

/// <summary>
/// Indexed chunk owned by the Search service. Received via the ChunksGenerated
/// integration event from the Ingestion worker.
/// </summary>
public sealed class SearchableChunk
{
    private SearchableChunk()
    {
    }

    public SearchableChunk(
        Guid documentId,
        Guid tenantId,
        string documentName,
        int chunkIndex,
        string content,
        Vector embedding)
    {
        Id = Guid.NewGuid();
        DocumentId = documentId;
        TenantId = tenantId;
        DocumentName = documentName;
        ChunkIndex = chunkIndex;
        Content = content;
        Embedding = embedding;
        IndexedAt = DateTime.UtcNow;
    }

    public Guid Id { get; private set; }

    public Guid DocumentId { get; private set; }

    public Guid TenantId { get; private set; }

    public string DocumentName { get; private set; } = string.Empty;

    public int ChunkIndex { get; private set; }

    public string Content { get; private set; } = string.Empty;

    public Vector Embedding { get; private set; } = null!;

    public DateTime IndexedAt { get; private set; }
}
