using KnowledgeBase.SharedKernel.Retrieval;
using Xunit;

namespace KnowledgeBase.UnitTests.Search;

public sealed class ChunkExpansionHelperTests
{
  [Fact]
  public void FillContiguousGaps_IncludesMissingChunksBetweenSelectedOnes()
  {
    var documentId = Guid.Parse("11111111-1111-1111-1111-111111111111");

    var pool = new List<RankedChunkHit>
    {
      CreateHit(documentId, 14, 0.9),
      CreateHit(documentId, 15, 0.85),
      CreateHit(documentId, 16, 0.8),
      CreateHit(documentId, 17, 0.75),
      CreateHit(documentId, 18, 0.7),
      CreateHit(documentId, 19, 0.65)
    };

    var selected = new List<RankedChunkHit>
    {
      pool[0],
      pool[1],
      pool[2],
      pool[4]
    };

    var filled = ChunkExpansionHelper.FillContiguousGaps(selected, pool, maxTotal: 8);

    Assert.Contains(filled, hit => hit.ChunkIndex == 17);
    Assert.DoesNotContain(filled, hit => hit.ChunkIndex == 19);
  }

  private static RankedChunkHit CreateHit(Guid documentId, int chunkIndex, double score)
  {
    return new RankedChunkHit(
      documentId,
      "business-analysis-ba.md.txt",
      null,
      chunkIndex,
      $"chunk {chunkIndex}",
      score);
  }
}
