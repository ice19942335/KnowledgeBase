namespace KnowledgeBase.Ai;

public sealed record AiTokenUsage(int PromptTokens, int CompletionTokens)
{
    public static AiTokenUsage Empty { get; } = new(0, 0);

    public int TotalTokens => PromptTokens + CompletionTokens;

    public AiTokenUsage Add(AiTokenUsage other)
    {
        return new AiTokenUsage(
            PromptTokens + other.PromptTokens,
            CompletionTokens + other.CompletionTokens);
    }
}

public sealed record EmbeddingResult(float[] Values, int TokenCount);

public sealed record ChatCompletionResult(string Text, AiTokenUsage Usage);

public sealed record RerankResult(
    IReadOnlyList<RankedChunkCandidate> Candidates,
    AiTokenUsage TokenUsage);
