namespace KnowledgeBase.Ingestion.Application;

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

    /// <summary>
    /// When a heading section body fits within this limit, keep it as a single chunk
    /// instead of splitting by <see cref="MaxChunkSize"/>.
    /// </summary>
    public int MaxSectionChunkSize { get; set; } = 8000;
}
