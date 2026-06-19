namespace KnowledgeBase.Domain.Documents;

/// <summary>
/// Aggregate root representing an uploaded document and its derived content.
/// </summary>
public class Document
{
    private readonly List<DocumentChunk> chunks = new();

    private Document()
    {
        Name = string.Empty;
        FileName = string.Empty;
        ContentType = string.Empty;
    }

    public Document(string name, string fileName, string contentType)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Document name is required.", nameof(name));
        }

        if (string.IsNullOrWhiteSpace(fileName))
        {
            throw new ArgumentException("File name is required.", nameof(fileName));
        }

        Id = Guid.NewGuid();
        Name = name;
        FileName = fileName;
        ContentType = string.IsNullOrWhiteSpace(contentType) ? "application/octet-stream" : contentType;
        UploadedAtUtc = DateTime.UtcNow;
        Status = DocumentStatus.Uploaded;
    }

    public Guid Id { get; private set; }

    public string Name { get; private set; }

    public string FileName { get; private set; }

    public string ContentType { get; private set; }

    public DateTime UploadedAtUtc { get; private set; }

    public DocumentStatus Status { get; private set; }

    public string? ExtractedText { get; private set; }

    public string? FailureReason { get; private set; }

    public IReadOnlyCollection<DocumentChunk> Chunks => chunks.AsReadOnly();

    public void MarkProcessing()
    {
        Status = DocumentStatus.Processing;
        FailureReason = null;
    }

    public void SetExtractedText(string text)
    {
        ExtractedText = text ?? string.Empty;
    }

    public void ReplaceChunks(IEnumerable<DocumentChunk> newChunks)
    {
        ArgumentNullException.ThrowIfNull(newChunks);

        chunks.Clear();
        chunks.AddRange(newChunks);
    }

    public void MarkProcessed()
    {
        Status = DocumentStatus.Processed;
        FailureReason = null;
    }

    public void MarkFailed(string reason)
    {
        Status = DocumentStatus.Failed;
        FailureReason = reason;
    }
}
