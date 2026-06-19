namespace KnowledgeBase.SharedKernel.Diagnostics;

public sealed record PipelineTraceStep(
    int Order,
    string Name,
    string Description,
    long DurationMs,
    object? Input,
    object? Output);
