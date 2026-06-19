namespace KnowledgeBase.Search.Application;

public class SearchOptions
{
    public const string SectionName = "Search";

    public int TopK { get; set; } = 5;

    /// <summary>
    /// Minimum cosine similarity (0-1) for vector matches. Lower matches trigger keyword fallback.
    /// </summary>
    public double MinSimilarityScore { get; set; } = 0.35;
}
