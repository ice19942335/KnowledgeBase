using KnowledgeBase.Ai;
using Microsoft.Extensions.Options;
using Xunit;

namespace KnowledgeBase.UnitTests.Ai;

public sealed class ContextualEmbeddingFormatterTests
{
    [Fact]
    public void Format_WhenEnabled_IncludesDocumentMetadata()
    {
        var formatter = new ContextualEmbeddingFormatter(Options.Create(new ContextualEmbeddingOptions { Enabled = true }));
        var request = new ContextualEmbeddingRequest(
            "HR Policy",
            "hr.pdf",
            1,
            5,
            "Employees receive 28 days.",
            "Leave",
            "Company leave rules.");

        var result = formatter.Format(request);

        Assert.Contains("Document: HR Policy", result);
        Assert.Contains("Summary: Company leave rules.", result);
        Assert.Contains("Section: Leave", result);
        Assert.Contains("Part 2 of 5", result);
        Assert.Contains("Employees receive 28 days.", result);
    }

    [Fact]
    public void Format_WhenDisabled_ReturnsRawContent()
    {
        var formatter = new ContextualEmbeddingFormatter(Options.Create(new ContextualEmbeddingOptions { Enabled = false }));
        var request = new ContextualEmbeddingRequest(
            "HR Policy",
            null,
            0,
            1,
            "Raw chunk text",
            null,
            "ignored");

        var result = formatter.Format(request);

        Assert.Equal("Raw chunk text", result);
    }
}
