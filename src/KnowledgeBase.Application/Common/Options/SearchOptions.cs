namespace KnowledgeBase.Application.Common.Options;

public class SearchOptions
{
    public const string SectionName = "Search";

    /// <summary>
    /// Default number of chunks returned by a semantic search.
    /// </summary>
    public int DefaultTopK { get; set; } = 5;

    /// <summary>
    /// Hard upper bound for the number of chunks a single request may return.
    /// </summary>
    public int MaxTopK { get; set; } = 20;
}
