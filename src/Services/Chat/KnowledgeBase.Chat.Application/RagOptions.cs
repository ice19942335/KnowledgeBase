namespace KnowledgeBase.Chat.Application;

public class RagOptions
{
    public const string SectionName = "Rag";

    public string SystemPrompt { get; set; } =
        "You are a knowledge base assistant. Answer the user's question using only the provided context. " +
        "Do not invent information. If the answer is not contained in the context, reply exactly with \"I don't know.\". " +
        "Be concise and factual.";
}
