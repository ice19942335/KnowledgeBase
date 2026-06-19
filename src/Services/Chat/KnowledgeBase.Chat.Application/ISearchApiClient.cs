namespace KnowledgeBase.Chat.Application;

/// <summary>
/// Calls the Search service API to retrieve context for RAG.
/// </summary>
public interface ISearchApiClient
{
    Task<IReadOnlyList<SearchContextChunk>> SearchAsync(
        Guid tenantId,
        string query,
        CancellationToken cancellationToken);

    Task<SearchTraceResponse> SearchWithTraceAsync(
        Guid tenantId,
        string query,
        CancellationToken cancellationToken);
}

public sealed record SearchContextChunk(
    Guid DocumentId,
    string DocumentName,
    int ChunkIndex,
    string Content,
    double Score);
