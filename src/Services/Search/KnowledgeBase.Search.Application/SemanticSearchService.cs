using System.Diagnostics;
using KnowledgeBase.Ai;
using KnowledgeBase.SharedKernel.Diagnostics;
using KnowledgeBase.Tenancy;
using Microsoft.Extensions.Options;
using Pgvector;

namespace KnowledgeBase.Search.Application;

public sealed class SemanticSearchService
{
    private readonly IChunkRepository chunkRepository;
    private readonly IEmbeddingGenerator embeddingGenerator;
    private readonly ITenantContext tenantContext;
    private readonly IAiAvailabilityState aiAvailabilityState;
    private readonly int topK;
    private readonly double minSimilarityScore;

    public SemanticSearchService(
        IChunkRepository chunkRepository,
        IEmbeddingGenerator embeddingGenerator,
        ITenantContext tenantContext,
        IAiAvailabilityState aiAvailabilityState,
        IOptions<SearchOptions> options)
    {
        this.chunkRepository = chunkRepository;
        this.embeddingGenerator = embeddingGenerator;
        this.tenantContext = tenantContext;
        this.aiAvailabilityState = aiAvailabilityState;
        topK = options.Value.TopK;
        minSimilarityScore = options.Value.MinSimilarityScore;
    }

    public async Task<IReadOnlyList<SearchResult>> SearchAsync(
        string query,
        CancellationToken cancellationToken)
    {
        var tenantId = tenantContext.RequireTenant();
        return await SearchInternalAsync(tenantId, query, topK, includeTrace: false, cancellationToken);
    }

    public Task<SearchTraceResult> SearchWithTraceAsync(
        string query,
        CancellationToken cancellationToken)
    {
        return SearchWithTraceInternalAsync(query, topK, cancellationToken);
    }

    internal async Task<SearchTraceResult> SearchWithTraceInternalAsync(
        string query,
        int requestedTopK,
        CancellationToken cancellationToken)
    {
        var tenantId = tenantContext.RequireTenant();
        var steps = new List<PipelineTraceStep>();
        var totalStopwatch = Stopwatch.StartNew();

        var results = await SearchInternalAsync(
            tenantId,
            query,
            requestedTopK,
            includeTrace: true,
            cancellationToken,
            steps);

        totalStopwatch.Stop();
        return new SearchTraceResult(query, results, steps, totalStopwatch.ElapsedMilliseconds);
    }

    private async Task<IReadOnlyList<SearchResult>> SearchInternalAsync(
        Guid tenantId,
        string query,
        int requestedTopK,
        bool includeTrace,
        CancellationToken cancellationToken,
        List<PipelineTraceStep>? steps = null)
    {
        if (!aiAvailabilityState.IsConfigured)
        {
            throw new AiNotConfiguredException();
        }

        var embedStopwatch = Stopwatch.StartNew();
        var embedding = await embeddingGenerator.GenerateAsync(query, cancellationToken);
        embedStopwatch.Stop();

        if (includeTrace && steps is not null)
        {
            steps.Add(new PipelineTraceStep(
                Order: 1,
                Name: "embedding.generate",
                Description: "Convert the search query into a vector using the embedding model.",
                DurationMs: embedStopwatch.ElapsedMilliseconds,
                Input: new { Query = query },
                Output: new
                {
                    Dimensions = embedding.Length,
                    VectorPreview = embedding.Take(8).Select(value => Math.Round(value, 6)).ToArray()
                }));
        }

        var vector = new Vector(embedding);

        var searchStopwatch = Stopwatch.StartNew();
        var vectorResults = await chunkRepository.SearchVectorAsync(
            tenantId,
            vector,
            requestedTopK,
            cancellationToken);
        searchStopwatch.Stop();

        var filteredResults = vectorResults
            .Where(result => result.Score >= minSimilarityScore)
            .ToList();

        if (includeTrace && steps is not null)
        {
            steps.Add(new PipelineTraceStep(
                Order: 2,
                Name: "search.vector",
                Description: "Find the closest document chunks in pgvector for the tenant.",
                DurationMs: searchStopwatch.ElapsedMilliseconds,
                Input: new { TenantId = tenantId, TopK = requestedTopK, MinSimilarityScore = minSimilarityScore },
                Output: new
                {
                    RawResultCount = vectorResults.Count,
                    FilteredResultCount = filteredResults.Count,
                    Results = vectorResults.Select(result => new
                    {
                        result.DocumentId,
                        result.DocumentName,
                        result.ChunkIndex,
                        Score = Math.Round(result.Score, 4),
                        ContentPreview = Truncate(result.Content, 160)
                    }).ToList()
                }));
        }

        if (filteredResults.Count > 0)
        {
            return filteredResults;
        }

        var fallbackStopwatch = Stopwatch.StartNew();
        var keywordResults = await chunkRepository.SearchKeywordAsync(
            tenantId,
            query,
            requestedTopK,
            cancellationToken);
        fallbackStopwatch.Stop();

        if (includeTrace && steps is not null)
        {
            steps.Add(new PipelineTraceStep(
                Order: 3,
                Name: "search.keyword_fallback",
                Description: "Vector matches were missing or below the similarity threshold, so keyword search was used.",
                DurationMs: fallbackStopwatch.ElapsedMilliseconds,
                Input: new { Query = query, TopK = requestedTopK, MinSimilarityScore = minSimilarityScore },
                Output: keywordResults.Select(result => new
                {
                    result.DocumentId,
                    result.DocumentName,
                    result.ChunkIndex,
                    Score = Math.Round(result.Score, 4),
                    ContentPreview = Truncate(result.Content, 160)
                }).ToList()));
        }

        return keywordResults;
    }

    private static string Truncate(string value, int maxLength)
    {
        if (value.Length <= maxLength)
        {
            return value;
        }

        return value[..maxLength] + "...";
    }
}
