namespace KnowledgeBase.Ai;

/// <summary>
/// Generates vector embeddings for text using an embedding model.
/// </summary>
public interface IEmbeddingGenerator
{
    Task<IReadOnlyList<float[]>> GenerateAsync(IReadOnlyList<string> inputs, CancellationToken cancellationToken);

    Task<float[]> GenerateAsync(string input, CancellationToken cancellationToken);
}
