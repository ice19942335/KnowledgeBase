using Google.GenAI.Types;
using KnowledgeBase.Ai;
using Xunit;

namespace KnowledgeBase.UnitTests.Ai;

public sealed class GeminiEmbeddingTokenCountsTests
{
    [Fact]
    public void ReadStatisticsTokenCount_ReturnsZero_WhenStatisticsMissing()
    {
        var embedding = new ContentEmbedding
        {
            Values = [0.1, 0.2],
        };

        Assert.Equal(0, GeminiEmbeddingTokenCounts.ReadStatisticsTokenCount(embedding));
    }

    [Fact]
    public void ReadStatisticsTokenCount_ReturnsValue_WhenStatisticsPresent()
    {
        var embedding = new ContentEmbedding
        {
            Values = [0.1, 0.2],
            Statistics = new ContentEmbeddingStatistics
            {
                TokenCount = 128,
            },
        };

        Assert.Equal(128, GeminiEmbeddingTokenCounts.ReadStatisticsTokenCount(embedding));
    }
}
