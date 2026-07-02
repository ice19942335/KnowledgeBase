namespace KnowledgeBase.Document.Application;

public enum DocumentRetryStatus
{
    Success,
    NotFound,
    NotRetryable,
}

public sealed record DocumentRetryResult(DocumentRetryStatus Status, DocumentDto? Document)
{
    public static DocumentRetryResult Success(DocumentDto document) =>
        new(DocumentRetryStatus.Success, document);

    public static DocumentRetryResult NotFound() =>
        new(DocumentRetryStatus.NotFound, null);

    public static DocumentRetryResult NotRetryable() =>
        new(DocumentRetryStatus.NotRetryable, null);
}
