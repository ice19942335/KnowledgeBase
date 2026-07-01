using KnowledgeBase.Application.Abstractions;
using KnowledgeBase.Application.Search;
using KnowledgeBase.Infrastructure.Persistence.ReadModels;
using KnowledgeBase.SharedKernel.Retrieval;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using Pgvector;

namespace KnowledgeBase.Infrastructure.Persistence;

public sealed class ChunkSearchRepository : IChunkSearchRepository
{
    private const string SearchSql =
        """
        SELECT
            c."DocumentId" AS "DocumentId",
            d."Name" AS "DocumentName",
            d."FileName" AS "FileName",
            c."ChunkIndex" AS "ChunkIndex",
            c."Content" AS "Content",
            (1 - (c."Embedding" <=> @query))::double precision AS "Score"
        FROM "document_chunks" AS c
        INNER JOIN "documents" AS d ON d."Id" = c."DocumentId"
        WHERE c."Embedding" IS NOT NULL
        ORDER BY c."Embedding" <=> @query
        LIMIT @topK
        """;

    private readonly KnowledgeBaseDbContext context;

    public ChunkSearchRepository(KnowledgeBaseDbContext context)
    {
        this.context = context;
    }

    public async Task<IReadOnlyList<ChunkMatch>> SearchVectorAsync(
        float[] queryEmbedding,
        int topK,
        CancellationToken cancellationToken)
    {
        var queryParameter = new NpgsqlParameter("query", new Vector(queryEmbedding));
        var topKParameter = new NpgsqlParameter("topK", topK);

        var rows = await context.Set<ChunkSearchRow>()
            .FromSqlRaw(SearchSql, queryParameter, topKParameter)
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        return rows
            .Select(row => new ChunkMatch(
                row.DocumentId,
                row.DocumentName,
                row.FileName,
                row.ChunkIndex,
                row.Content,
                row.Score))
            .ToList();
    }

    public async Task<IReadOnlyList<ChunkMatch>> SearchKeywordAsync(
        string query,
        int topK,
        CancellationToken cancellationToken)
    {
        var terms = KeywordSearchScorer.Tokenize(query);
        var rows = await context.DocumentChunks
            .AsNoTracking()
            .Join(
                context.Documents.AsNoTracking(),
                chunk => chunk.DocumentId,
                document => document.Id,
                (chunk, document) => new { chunk, document })
            .ToListAsync(cancellationToken);

        return rows
            .Select(row => new ChunkMatch(
                row.chunk.DocumentId,
                row.document.Name,
                row.document.FileName,
                row.chunk.ChunkIndex,
                row.chunk.Content,
                KeywordSearchScorer.Score(row.chunk.Content, query, terms)))
            .Where(match => match.Score > 0)
            .OrderByDescending(match => match.Score)
            .ThenBy(match => match.ChunkIndex)
            .Take(topK)
            .ToList();
    }

    public async Task<IReadOnlyList<ChunkMatch>> GetChunksByLocatorsAsync(
        IReadOnlyList<ChunkLocator> locators,
        CancellationToken cancellationToken)
    {
        if (locators.Count == 0)
        {
            return Array.Empty<ChunkMatch>();
        }

        var documentIds = locators.Select(locator => locator.DocumentId).Distinct().ToList();
        var locatorSet = locators
            .Select(locator => (locator.DocumentId, locator.ChunkIndex))
            .ToHashSet();

        var rows = await context.DocumentChunks
            .AsNoTracking()
            .Join(
                context.Documents.AsNoTracking(),
                chunk => chunk.DocumentId,
                document => document.Id,
                (chunk, document) => new { chunk, document })
            .Where(row => documentIds.Contains(row.chunk.DocumentId))
            .ToListAsync(cancellationToken);

        return rows
            .Where(row => locatorSet.Contains((row.chunk.DocumentId, row.chunk.ChunkIndex)))
            .Select(row => new ChunkMatch(
                row.chunk.DocumentId,
                row.document.Name,
                row.document.FileName,
                row.chunk.ChunkIndex,
                row.chunk.Content,
                0))
            .ToList();
    }
}
