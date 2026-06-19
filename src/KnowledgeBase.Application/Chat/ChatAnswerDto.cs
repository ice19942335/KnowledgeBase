namespace KnowledgeBase.Application.Chat;

public sealed record ChatAnswerDto(
    string Answer,
    IReadOnlyList<SourceReferenceDto> Sources);
