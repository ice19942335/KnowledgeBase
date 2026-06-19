using System.Text;
using KnowledgeBase.Ingestion.Infrastructure.TextExtraction;
using Xunit;

namespace KnowledgeBase.UnitTests.Ingestion;

public sealed class PlainTextExtractorTests
{
    private readonly PlainTextExtractor extractor = new();

    [Theory]
    [InlineData("policy.md", "application/octet-stream")]
    [InlineData("policy.MD", "application/octet-stream")]
    [InlineData("notes.txt", "text/plain")]
    [InlineData("readme.text", "application/octet-stream")]
    [InlineData("unknown.bin", "text/markdown")]
    public void CanExtract_AcceptsMarkdownAndPlainTextFiles(string fileName, string contentType)
    {
        Assert.True(extractor.CanExtract(fileName, contentType));
    }

    [Fact]
    public async Task ExtractAsync_ReadsMarkdownContent()
    {
        const string markdown = "# HR Policy\n\nAnnual leave: **28 days**.";
        await using var stream = new MemoryStream(Encoding.UTF8.GetBytes(markdown));

        var text = await extractor.ExtractAsync(stream, "HR_Policy.md", CancellationToken.None);

        Assert.Contains("Annual leave", text);
        Assert.Contains("28 days", text);
    }

}