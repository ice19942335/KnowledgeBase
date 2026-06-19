namespace KnowledgeBase.Application.Documents;

/// <summary>
/// Request to ingest a single uploaded file into the knowledge base.
/// </summary>
public sealed class UploadDocumentCommand
{
    public required Stream Content { get; init; }

    public required string FileName { get; init; }

    public required string ContentType { get; init; }

    /// <summary>
    /// Optional human-friendly name. Falls back to the file name when omitted.
    /// </summary>
    public string? DisplayName { get; init; }
}
