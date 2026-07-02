using Google.GenAI.Types;

namespace KnowledgeBase.Ai;

public static class GeminiEmbeddingTokenCounts
{
    public static int ReadStatisticsTokenCount(ContentEmbedding? embedding)
    {
        return (int)(embedding?.Statistics?.TokenCount ?? 0);
    }
}
