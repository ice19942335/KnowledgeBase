using KnowledgeBase.Tenancy;

namespace KnowledgeBase.Search.Application;

public sealed class SearchExplorerService
{
    private readonly IChunkRepository chunkRepository;
    private readonly ITenantContext tenantContext;

    public SearchExplorerService(IChunkRepository chunkRepository, ITenantContext tenantContext)
    {
        this.chunkRepository = chunkRepository;
        this.tenantContext = tenantContext;
    }

    public async Task<SearchExplorerResult> GetExplorerAsync(
        IReadOnlyList<Guid>? documentIds,
        CancellationToken cancellationToken)
    {
        var tenantId = tenantContext.RequireTenant();
        var chunks = await chunkRepository.ListAsync(tenantId, documentIds, cancellationToken);

        var documents = chunks
            .GroupBy(chunk => chunk.DocumentId)
            .Select(group => new DocumentChunksGroupDto(
                group.Key,
                group.First().DocumentName,
                group.OrderBy(chunk => chunk.ChunkIndex).ToList()))
            .OrderBy(group => group.DocumentName)
            .ToList();

        return new SearchExplorerResult(documents, chunks.Count);
    }
}
