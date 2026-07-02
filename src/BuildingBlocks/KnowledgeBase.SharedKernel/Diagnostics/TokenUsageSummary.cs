namespace KnowledgeBase.SharedKernel.Diagnostics;

public sealed record TokenUsageSummary(int RequestTokens, int IndexedTokens)
{
    public static TokenUsageSummary Empty { get; } = new(0, 0);

    public int TotalTokens => RequestTokens + IndexedTokens;
}
