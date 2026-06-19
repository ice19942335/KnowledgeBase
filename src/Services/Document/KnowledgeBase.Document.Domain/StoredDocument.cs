namespace KnowledgeBase.Document.Domain;

/// <summary>
/// Aggregate root owned by the Document service. It tracks file metadata and the
/// processing lifecycle. The extracted chunks/embeddings are owned by the Search
/// service (database-per-service).
/// </summary>
public sealed class StoredDocument
{
    private StoredDocument()
    {
    }

    public StoredDocument(Guid tenantId, string name, string fileName, string contentType, string storagePath)
    {
        if (tenantId == Guid.Empty)
        {
            throw new ArgumentException("Tenant id is required.", nameof(tenantId));
        }

        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Name is required.", nameof(name));
        }

        if (string.IsNullOrWhiteSpace(storagePath))
        {
            throw new ArgumentException("Storage path is required.", nameof(storagePath));
        }

        Id = Guid.NewGuid();
        TenantId = tenantId;
        Name = name;
        FileName = fileName;
        ContentType = contentType;
        StoragePath = storagePath;
        Status = DocumentStatus.Uploaded;
        CreatedAt = DateTime.UtcNow;
    }

    public Guid Id { get; private set; }

    public Guid TenantId { get; private set; }

    public string Name { get; private set; } = string.Empty;

    public string FileName { get; private set; } = string.Empty;

    public string ContentType { get; private set; } = string.Empty;

    public string StoragePath { get; private set; } = string.Empty;

    public DocumentStatus Status { get; private set; }

    public int ChunkCount { get; private set; }

    public string? Error { get; private set; }

    public DateTime CreatedAt { get; private set; }

    public DateTime? ProcessedAt { get; private set; }

    public void MarkProcessing()
    {
        Status = DocumentStatus.Processing;
        Error = null;
    }

    public void MarkProcessed(int chunkCount)
    {
        Status = DocumentStatus.Processed;
        ChunkCount = chunkCount;
        ProcessedAt = DateTime.UtcNow;
        Error = null;
    }

    public void MarkFailed(string error)
    {
        Status = DocumentStatus.Failed;
        Error = error;
        ProcessedAt = DateTime.UtcNow;
    }
}
