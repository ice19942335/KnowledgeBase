namespace KnowledgeBase.Document.Application;

public sealed record BatchUploadItemResult(
    string FileName,
    DocumentDto? Document,
    string? Error);

public sealed record BatchUploadResultDto(IReadOnlyList<BatchUploadItemResult> Results)
{
    public int SucceededCount => Results.Count(item => item.Document is not null);

    public int FailedCount => Results.Count(item => item.Document is null);
}
