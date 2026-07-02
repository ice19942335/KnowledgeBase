namespace KnowledgeBase.Contracts;

/// <summary>
/// Published by the Document service after a file is uploaded and stored.
/// Consumed by the Ingestion worker.
/// </summary>
public sealed record DocumentUploaded(
    Guid DocumentId,
    Guid TenantId,
    string DocumentName,
    string FileName,
    string ContentType,
    string StoragePath);

/// <summary>
/// A single generated chunk with its embedding vector.
/// </summary>
public sealed record GeneratedChunk(
    int Index,
    string Content,
    float[] Embedding,
    int EmbeddingTokenCount);

/// <summary>
/// Published by the Ingestion worker after extraction, chunking, and embedding.
/// Consumed by the Search service which owns the searchable store.
/// </summary>
public sealed record ChunksGenerated(
    Guid DocumentId,
    Guid TenantId,
    string DocumentName,
    string FileName,
    IReadOnlyList<GeneratedChunk> Chunks);

/// <summary>
/// Published when a document has been fully processed; consumed by the Document
/// service to update its status.
/// </summary>
public sealed record DocumentProcessingCompleted(
    Guid DocumentId,
    Guid TenantId,
    int ChunkCount);

/// <summary>
/// Published when processing fails; consumed by the Document service.
/// </summary>
public sealed record DocumentProcessingFailed(
    Guid DocumentId,
    Guid TenantId,
    string Reason);

/// <summary>
/// Published by the Document service when a document is deleted, so dependent
/// services can remove their derived data.
/// </summary>
public sealed record DocumentDeleted(
    Guid DocumentId,
    Guid TenantId);
