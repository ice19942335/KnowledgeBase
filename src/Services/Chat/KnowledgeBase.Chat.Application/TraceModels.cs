using KnowledgeBase.SharedKernel.Diagnostics;

namespace KnowledgeBase.Chat.Application;

public sealed record SearchTraceResponse(
    string Query,
    IReadOnlyList<SearchContextChunk> Results,
    IReadOnlyList<PipelineTraceStep> Steps,
    long TotalDurationMs);

public sealed record ChatTraceAnswerDto(
    Guid ConversationId,
    string Answer,
    IReadOnlyList<SourceReference> Sources,
    IReadOnlyList<PipelineTraceStep> Steps,
    long TotalDurationMs);
