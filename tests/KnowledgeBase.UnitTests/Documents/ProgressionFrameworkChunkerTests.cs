using KnowledgeBase.Application.Common.Options;
using KnowledgeBase.Application.Documents;
using Microsoft.Extensions.Options;
using Xunit;

namespace KnowledgeBase.UnitTests.Documents;

public sealed class ProgressionFrameworkChunkerTests
{
  [Fact]
  public void Split_WithBa3Section_KeepsAllSkillsInSingleChunk()
  {
    var chunker = CreateChunker(maxChunkSize: 1000, maxSectionChunkSize: 8000);
    var text = """
      ### BA3 — Senior Business Analyst
      * Addresses any challenge or opportunity, regardless of level of complexity,
      * Finds a way to deliver business value for any change challenge,

      IIBA equivalent is L3 Skilled

      | Knowledge ID | Knowledge description | Mastery criteria | Tools |
      |--------------|------------------------|------------------|-------|
      | BA3-001 | First skill | Mastery one | Tool one |
      | BA3-002 | Second skill | Mastery two | Tool two |
      | BA3-003 | Third skill | Mastery three | Tool three |
      | BA3-014 | Fourteenth skill | Mastery fourteen | Tool fourteen |
      | BA3-015 | Fifteenth skill | Mastery fifteen | Tool fifteen |
      """;

    var result = chunker.Split(text);

    Assert.Single(result);
    Assert.Equal("BA3 — Senior Business Analyst", result[0].SectionTitle);
    Assert.Contains("BA3-001", result[0].Content);
    Assert.Contains("BA3-015", result[0].Content);
  }

  [Fact]
  public void Split_WithOversizedSection_FallsBackToSizeBasedChunks()
  {
    var chunker = CreateChunker(maxChunkSize: 200, maxSectionChunkSize: 100);
    var text = "### Large Section\n" + string.Join(' ', Enumerable.Repeat("token", 200));

    var result = chunker.Split(text);

    Assert.True(result.Count > 1);
    Assert.All(result, chunk => Assert.Equal("Large Section", chunk.SectionTitle));
  }

  private static TextChunker CreateChunker(int maxChunkSize, int maxSectionChunkSize)
  {
    return new TextChunker(Options.Create(new ChunkingOptions
    {
      MaxChunkSize = maxChunkSize,
      OverlapSize = 20,
      MaxSectionChunkSize = maxSectionChunkSize
    }));
  }
}
