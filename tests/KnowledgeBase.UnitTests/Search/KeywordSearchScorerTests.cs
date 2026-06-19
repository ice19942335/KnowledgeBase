using KnowledgeBase.Search.Application;

namespace KnowledgeBase.UnitTests.Search;

public sealed class KeywordSearchScorerTests
{
    [Fact]
    public void Score_ReturnsOne_WhenContentContainsFullQuery()
    {
        var terms = KeywordSearchScorer.Tokenize("annual leave days");

        var score = KeywordSearchScorer.Score(
            "Employees receive 28 calendar days of annual leave.",
            "annual leave days",
            terms);

        Assert.Equal(1.0, score);
    }

    [Fact]
    public void Score_ReturnsPartialMatch_ForOverlappingTerms()
    {
        var terms = KeywordSearchScorer.Tokenize("probation period production");

        var score = KeywordSearchScorer.Score(
            "The probation period for production workers is 3 months.",
            "probation period production",
            terms);

        Assert.Equal(1.0, score);
    }

    [Fact]
    public void Tokenize_IgnoresShortWords()
    {
        var terms = KeywordSearchScorer.Tokenize("What is the notice period?");

        Assert.DoesNotContain("is", terms);
        Assert.Contains("notice", terms);
        Assert.Contains("period", terms);
    }
}
