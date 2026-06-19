using KnowledgeBase.Application.Abstractions;
using KnowledgeBase.Infrastructure.Persistence.ReadModels;
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

    public async Task<IReadOnlyList<ChunkMatch>> SearchAsync(
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
}
