using KnowledgeBase.Search.Application;
using KnowledgeBase.Search.Domain;
using KnowledgeBase.SharedKernel.Retrieval;
using Microsoft.EntityFrameworkCore;
using Pgvector;
using Pgvector.EntityFrameworkCore;

namespace KnowledgeBase.Search.Infrastructure.Persistence;

public sealed class ChunkRepository : IChunkRepository
{
    private readonly SearchDbContext dbContext;

    public ChunkRepository(SearchDbContext dbContext)
    {
        this.dbContext = dbContext;
    }

    public async Task AddRangeAsync(IEnumerable<SearchableChunk> chunks, CancellationToken cancellationToken)
    {
        await dbContext.Chunks.AddRangeAsync(chunks, cancellationToken);
    }

    public async Task RemoveByDocumentAsync(Guid tenantId, Guid documentId, CancellationToken cancellationToken)
    {
        await dbContext.Chunks
            .Where(c => c.TenantId == tenantId && c.DocumentId == documentId)
            .ExecuteDeleteAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<SearchResult>> SearchKeywordAsync(
        Guid tenantId,
        string query,
        int topK,
        CancellationToken cancellationToken)
    {
        var terms = KeywordSearchScorer.Tokenize(query);
        var chunks = await dbContext.Chunks
            .Where(chunk => chunk.TenantId == tenantId)
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        return chunks
            .Select(chunk => new SearchResult(
                chunk.DocumentId,
                chunk.DocumentName,
                chunk.ChunkIndex,
                chunk.Content,
                KeywordSearchScorer.Score(chunk.Content, query, terms),
                chunk.EmbeddingTokenCount))
            .Where(result => result.Score > 0)
            .OrderByDescending(result => result.Score)
            .ThenBy(result => result.ChunkIndex)
            .Take(topK)
            .ToList();
    }

    public async Task<IReadOnlyList<SearchResult>> SearchVectorAsync(
        Guid tenantId,
        Vector queryEmbedding,
        int topK,
        CancellationToken cancellationToken)
    {
        return await dbContext.Chunks
            .Where(c => c.TenantId == tenantId)
            .OrderBy(c => c.Embedding.CosineDistance(queryEmbedding))
            .Take(topK)
            .Select(c => new SearchResult(
                c.DocumentId,
                c.DocumentName,
                c.ChunkIndex,
                c.Content,
                1.0 - c.Embedding.CosineDistance(queryEmbedding),
                c.EmbeddingTokenCount))
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<SearchResult>> GetChunksByLocatorsAsync(
        Guid tenantId,
        IReadOnlyList<ChunkLocator> locators,
        CancellationToken cancellationToken)
    {
        if (locators.Count == 0)
        {
            return Array.Empty<SearchResult>();
        }

        var documentIds = locators.Select(locator => locator.DocumentId).Distinct().ToList();
        var locatorSet = locators
            .Select(locator => (locator.DocumentId, locator.ChunkIndex))
            .ToHashSet();

        var chunks = await dbContext.Chunks
            .Where(chunk => chunk.TenantId == tenantId && documentIds.Contains(chunk.DocumentId))
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        return chunks
            .Where(chunk => locatorSet.Contains((chunk.DocumentId, chunk.ChunkIndex)))
            .Select(chunk => new SearchResult(
                chunk.DocumentId,
                chunk.DocumentName,
                chunk.ChunkIndex,
                chunk.Content,
                0,
                chunk.EmbeddingTokenCount))
            .ToList();
    }

    public async Task<IReadOnlyList<ChunkDetailDto>> ListAsync(
        Guid tenantId,
        IReadOnlyList<Guid>? documentIds,
        CancellationToken cancellationToken)
    {
        var query = dbContext.Chunks.Where(chunk => chunk.TenantId == tenantId);

        if (documentIds is not null)
        {
            query = query.Where(chunk => documentIds.Contains(chunk.DocumentId));
        }

        return await query
            .OrderBy(chunk => chunk.DocumentName)
            .ThenBy(chunk => chunk.ChunkIndex)
            .Select(chunk => new ChunkDetailDto(
                chunk.Id,
                chunk.DocumentId,
                chunk.DocumentName,
                chunk.ChunkIndex,
                chunk.Content,
                chunk.IndexedAt,
                chunk.EmbeddingTokenCount))
            .ToListAsync(cancellationToken);
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken)
    {
        return dbContext.SaveChangesAsync(cancellationToken);
    }
}
