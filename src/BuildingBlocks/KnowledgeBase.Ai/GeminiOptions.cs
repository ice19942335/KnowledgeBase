using System.ComponentModel.DataAnnotations;

namespace KnowledgeBase.Ai;

public class GeminiOptions
{
    public const string SectionName = "Gemini";

    public string ApiKey { get; set; } = string.Empty;

    public string ChatModel { get; set; } = "gemini-3.5-flash";

    public string EmbeddingModel { get; set; } = "gemini-embedding-001";

    /// <summary>
    /// Must match the pgvector column size in the Search database.
    /// </summary>
    public int EmbeddingDimensions { get; set; } = 1536;
}
