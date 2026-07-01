using KnowledgeBase.SharedKernel.TextProcessing;
using Xunit;

namespace KnowledgeBase.UnitTests.TextProcessing;

public sealed class HeadingParserTests
{
    [Fact]
    public void SplitIntoSections_WithMarkdownHeading_CreatesSection()
    {
        var text = "## Leave Policy\nEmployees receive 28 days.\n\n## Payroll\nPay on the 10th.";

        var sections = HeadingParser.SplitIntoSections(text);

        Assert.Equal(2, sections.Count);
        Assert.Equal("Leave Policy", sections[0].Title);
        Assert.Contains("28 days", sections[0].Body);
        Assert.Equal("Payroll", sections[1].Title);
    }

    [Fact]
    public void SplitIntoSections_WithNumberedHeading_CreatesSection()
    {
        var text = "1. General\nAll employees must comply.";

        var sections = HeadingParser.SplitIntoSections(text);

        Assert.Single(sections);
        Assert.Equal("1. General", sections[0].Title);
    }

    [Fact]
    public void SplitIntoSections_WithoutHeadings_ReturnsSingleSection()
    {
        var text = "Plain paragraph without headings.";

        var sections = HeadingParser.SplitIntoSections(text);

        Assert.Single(sections);
        Assert.Null(sections[0].Title);
        Assert.Equal(text, sections[0].Body);
    }
}
