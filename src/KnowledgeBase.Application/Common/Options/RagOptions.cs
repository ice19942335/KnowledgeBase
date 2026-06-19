namespace KnowledgeBase.Application.Common.Options;

public class RagOptions
{
    public const string SectionName = "Rag";

    /// <summary>
    /// Number of chunks retrieved as grounding context for an answer.
    /// </summary>
    public int ContextChunkCount { get; set; } = 5;

    /// <summary>
    /// Answer returned when no relevant context is available.
    /// </summary>
    public string NoAnswerResponse { get; set; } = "I don't know.";
}
