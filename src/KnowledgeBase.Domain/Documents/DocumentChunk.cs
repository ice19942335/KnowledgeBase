namespace KnowledgeBase.Domain.Documents;

/// <summary>
/// A contiguous slice of a document's text together with its vector embedding.
/// Chunks are the unit of retrieval for semantic search and RAG.
/// </summary>
public class DocumentChunk
{
    private DocumentChunk()
    {
        Content = string.Empty;
    }

    public DocumentChunk(Guid documentId, int chunkIndex, string content)
    {
        if (documentId == Guid.Empty)
        {
            throw new ArgumentException("Document id is required.", nameof(documentId));
        }

        if (chunkIndex < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(chunkIndex), "Chunk index cannot be negative.");
        }

        if (string.IsNullOrWhiteSpace(content))
        {
            throw new ArgumentException("Chunk content cannot be empty.", nameof(content));
        }

        Id = Guid.NewGuid();
        DocumentId = documentId;
        ChunkIndex = chunkIndex;
        Content = content;
    }

    public Guid Id { get; private set; }

    public Guid DocumentId { get; private set; }

    public int ChunkIndex { get; private set; }

    public string Content { get; private set; }

    /// <summary>
    /// Embedding vector for the chunk content. Null until embeddings are generated.
    /// </summary>
    public float[]? Embedding { get; private set; }

    public void SetEmbedding(float[] embedding)
    {
        ArgumentNullException.ThrowIfNull(embedding);

        if (embedding.Length == 0)
        {
            throw new ArgumentException("Embedding cannot be empty.", nameof(embedding));
        }

        Embedding = embedding;
    }
}
