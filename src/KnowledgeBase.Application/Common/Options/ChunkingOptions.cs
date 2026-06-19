namespace KnowledgeBase.Application.Common.Options;

public class ChunkingOptions
{
    public const string SectionName = "Chunking";

    /// <summary>
    /// Maximum number of characters per chunk.
    /// </summary>
    public int MaxChunkSize { get; set; } = 1000;

    /// <summary>
    /// Number of characters shared between consecutive chunks to preserve context.
    /// </summary>
    public int OverlapSize { get; set; } = 150;
}
