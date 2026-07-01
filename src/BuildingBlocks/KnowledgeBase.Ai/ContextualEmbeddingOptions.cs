namespace KnowledgeBase.Ai;

public class ContextualEmbeddingOptions
{
    public const string SectionName = "ContextualEmbedding";

    public bool Enabled { get; set; } = true;

    public int MaxSummaryInputCharacters { get; set; } = 12000;

    public int MaxSummaryWords { get; set; } = 120;

    public int ShortDocumentCharacterThreshold { get; set; } = 200;
}
