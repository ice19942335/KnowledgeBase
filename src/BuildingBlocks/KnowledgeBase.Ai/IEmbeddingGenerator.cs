namespace KnowledgeBase.Ai;

/// <summary>
/// Generates vector embeddings for text using an embedding model.
/// </summary>
public interface IEmbeddingGenerator
{
    Task<IReadOnlyList<EmbeddingResult>> GenerateAsync(
        IReadOnlyList<string> inputs,
        CancellationToken cancellationToken);

    Task<EmbeddingResult> GenerateAsync(string input, CancellationToken cancellationToken);
}
