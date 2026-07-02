namespace KnowledgeBase.Search.Application;

public class SearchOptions
{
    public const string SectionName = "Search";

    public int RetrievalTopK { get; set; } = 20;

    public int FinalTopK { get; set; } = 5;

    /// <summary>
    /// Minimum cosine similarity (0-1) for vector matches included in hybrid fusion.
    /// </summary>
    public double MinSimilarityScore { get; set; } = 0.35;

    public bool HybridSearchEnabled { get; set; } = true;

    public int RrfK { get; set; } = 60;

    public int NeighborExpansionRadius { get; set; } = 1;

    public bool RerankingEnabled { get; set; } = true;

    public int RerankingCandidateLimit { get; set; } = 20;

    public bool ContiguousGapFillEnabled { get; set; } = true;

    public int ContiguousGapFillMaxExtra { get; set; } = 3;
}
