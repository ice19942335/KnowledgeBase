using KnowledgeBase.Ai;

namespace KnowledgeBase.UnitTests;

internal static class AiTestData
{
    internal static EmbeddingResult Embedding(float value = 0.1f, int tokenCount = 0)
    {
        return new EmbeddingResult([value], tokenCount);
    }

    internal static IReadOnlyList<EmbeddingResult> Embeddings(int count, int tokenCount = 0)
    {
        return Enumerable.Range(0, count)
            .Select(_ => Embedding(0.1f, tokenCount))
            .ToList();
    }

    internal static ChatCompletionResult Completion(string text, int promptTokens = 0, int completionTokens = 0)
    {
        return new ChatCompletionResult(text, new AiTokenUsage(promptTokens, completionTokens));
    }

    internal static RerankResult Rerank(IReadOnlyList<RankedChunkCandidate> candidates, int promptTokens = 0, int completionTokens = 0)
    {
        return new RerankResult(candidates, new AiTokenUsage(promptTokens, completionTokens));
    }
}
