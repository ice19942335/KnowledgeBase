namespace KnowledgeBase.Infrastructure.Persistence.ReadModels;

/// <summary>
/// Keyless projection used to materialize raw vector-search query results.
/// </summary>
public sealed class ChunkSearchRow
{
    public Guid DocumentId { get; set; }

    public string DocumentName { get; set; } = string.Empty;

    public string FileName { get; set; } = string.Empty;

    public int ChunkIndex { get; set; }

    public string Content { get; set; } = string.Empty;

    public double Score { get; set; }
}
