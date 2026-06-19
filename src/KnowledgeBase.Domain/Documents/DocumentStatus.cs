namespace KnowledgeBase.Domain.Documents;

/// <summary>
/// Lifecycle of a document as it moves through the ingestion pipeline.
/// </summary>
public enum DocumentStatus
{
    Uploaded = 0,
    Processing = 1,
    Processed = 2,
    Failed = 3
}
