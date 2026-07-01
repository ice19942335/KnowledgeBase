using KnowledgeBase.Application.Common.Options;
using KnowledgeBase.Application.Documents;
using Microsoft.Extensions.Options;
using Xunit;

namespace KnowledgeBase.UnitTests.Documents;

public sealed class TextChunkerTests
{
    private static TextChunker CreateChunker(int maxChunkSize, int overlapSize)
    {
        var options = Options.Create(new ChunkingOptions
        {
            MaxChunkSize = maxChunkSize,
            OverlapSize = overlapSize
        });

        return new TextChunker(options);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("\n\t  ")]
    public void Split_WithBlankText_ReturnsEmpty(string text)
    {
        var chunker = CreateChunker(100, 10);

        var result = chunker.Split(text);

        Assert.Empty(result);
    }

    [Fact]
    public void Split_WithShortText_ReturnsSingleNormalizedChunk()
    {
        var chunker = CreateChunker(100, 10);

        var result = chunker.Split("  hello   world \n test ");

        Assert.Single(result);
        Assert.Equal("hello world test", result[0].Content);
    }

    [Fact]
    public void Split_WithMarkdownHeading_AssignsSectionTitle()
    {
        var chunker = CreateChunker(200, 10);
        var text = "## Leave Policy\nEmployees receive twenty eight calendar days per year.";

        var result = chunker.Split(text);

        Assert.Single(result);
        Assert.Equal("Leave Policy", result[0].SectionTitle);
        Assert.Contains("twenty eight calendar days", result[0].Content);
    }

    [Fact]
    public void Split_WithLongText_ProducesMultipleChunksWithinSizeLimit()
    {
        const int maxChunkSize = 20;
        var chunker = CreateChunker(maxChunkSize, 5);
        var text = "one two three four five six seven eight nine ten eleven twelve";

        var result = chunker.Split(text);

        Assert.True(result.Count > 1);
        Assert.All(result, chunk => Assert.True(chunk.Content.Length <= maxChunkSize));
    }

    [Fact]
    public void Split_PreservesEveryOriginalWord()
    {
        var chunker = CreateChunker(25, 8);
        var words = new[] { "alpha", "beta", "gamma", "delta", "epsilon", "zeta", "eta", "theta" };
        var text = string.Join(' ', words);

        var result = chunker.Split(text);
        var combined = string.Join(' ', result.Select(chunk => chunk.Content));

        Assert.All(words, word => Assert.Contains(word, combined));
    }

    [Fact]
    public void Split_WithConsecutiveChunks_OverlapsContext()
    {
        var chunker = CreateChunker(30, 10);
        var text = "the quick brown fox jumps over the lazy dog near the river bank";

        var result = chunker.Split(text);

        Assert.True(result.Count >= 2);
        var tailOfFirst = result[0].Content.Split(' ')[^1];
        Assert.Contains(tailOfFirst, result[1].Content);
    }

    [Fact]
    public void Split_WithWordLongerThanChunk_HardSplitsTheWord()
    {
        const int maxChunkSize = 10;
        var chunker = CreateChunker(maxChunkSize, 0);
        var text = new string('a', 35);

        var result = chunker.Split(text);

        Assert.True(result.Count >= 4);
        Assert.All(result, chunk => Assert.True(chunk.Content.Length <= maxChunkSize));
    }
}
