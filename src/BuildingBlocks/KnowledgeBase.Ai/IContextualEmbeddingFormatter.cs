namespace KnowledgeBase.Ai;

public interface IContextualEmbeddingFormatter
{
    string Format(ContextualEmbeddingRequest request);
}
