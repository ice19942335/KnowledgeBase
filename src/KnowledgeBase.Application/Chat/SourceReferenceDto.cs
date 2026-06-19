namespace KnowledgeBase.Application.Chat;

public sealed record SourceReferenceDto(
    Guid DocumentId,
    string DocumentName,
    string FileName);
