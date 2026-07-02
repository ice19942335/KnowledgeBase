namespace KnowledgeBase.Application.Common.Options;

public class SearchOptions
{
    public const string SectionName = "Search";

    public int RetrievalTopK { get; set; } = 20;

    public int FinalTopK { get; set; } = 5;

    public int DefaultTopK { get; set; } = 5;

    public int MaxTopK { get; set; } = 20;

    public double MinSimilarityScore { get; set; } = 0.35;

    public bool HybridSearchEnabled { get; set; } = true;

    public int RrfK { get; set; } = 60;

    public int NeighborExpansionRadius { get; set; } = 1;

    public bool RerankingEnabled { get; set; } = true;

    public int RerankingCandidateLimit { get; set; } = 20;

    public bool ContiguousGapFillEnabled { get; set; } = true;

    public int ContiguousGapFillMaxExtra { get; set; } = 3;
}
