using KnowledgeBase.SharedKernel.Retrieval;
using Xunit;

namespace KnowledgeBase.UnitTests.Retrieval;

public sealed class HybridSearchMergerTests
{
    [Fact]
    public void Merge_CombinesVectorAndKeywordRankings()
    {
        var documentId = Guid.NewGuid();
        var vectorResults = new[]
        {
            new RankedChunkHit(documentId, "HR Policy", null, 0, "vector chunk", 0.9)
        };
        var keywordResults = new[]
        {
            new RankedChunkHit(documentId, "HR Policy", null, 1, "keyword chunk", 1.0)
        };

        var merged = HybridSearchMerger.Merge(vectorResults, keywordResults, rrfK: 60);

        Assert.Equal(2, merged.Count);
        Assert.Contains(merged, hit => hit.ChunkIndex == 0);
        Assert.Contains(merged, hit => hit.ChunkIndex == 1);
    }

    [Fact]
    public void Merge_DeduplicatesSameChunk()
    {
        var documentId = Guid.NewGuid();
        var hit = new RankedChunkHit(documentId, "HR Policy", null, 2, "same chunk", 0.8);

        var merged = HybridSearchMerger.Merge(new[] { hit }, new[] { hit }, rrfK: 60);

        Assert.Single(merged);
        Assert.True(merged[0].Score > 0);
    }
}
