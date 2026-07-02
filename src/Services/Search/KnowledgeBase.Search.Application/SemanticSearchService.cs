using System.Diagnostics;
using KnowledgeBase.Ai;
using KnowledgeBase.SharedKernel.Diagnostics;
using KnowledgeBase.SharedKernel.Retrieval;
using KnowledgeBase.Tenancy;
using Microsoft.Extensions.Options;
using Pgvector;

namespace KnowledgeBase.Search.Application;

public sealed class SemanticSearchService
{
    private readonly IChunkRepository chunkRepository;
    private readonly IEmbeddingGenerator embeddingGenerator;
    private readonly IChunkReranker chunkReranker;
    private readonly ITenantContext tenantContext;
    private readonly IAiAvailabilityState aiAvailabilityState;
    private readonly SearchOptions options;

    public SemanticSearchService(
        IChunkRepository chunkRepository,
        IEmbeddingGenerator embeddingGenerator,
        IChunkReranker chunkReranker,
        ITenantContext tenantContext,
        IAiAvailabilityState aiAvailabilityState,
        IOptions<SearchOptions> options)
    {
        this.chunkRepository = chunkRepository;
        this.embeddingGenerator = embeddingGenerator;
        this.chunkReranker = chunkReranker;
        this.tenantContext = tenantContext;
        this.aiAvailabilityState = aiAvailabilityState;
        this.options = options.Value;
    }

    public async Task<SearchQueryResult> SearchAsync(
        string query,
        CancellationToken cancellationToken)
    {
        var tenantId = tenantContext.RequireTenant();
        var execution = await SearchInternalAsync(tenantId, query, includeTrace: false, cancellationToken);

        return new SearchQueryResult(
            execution.Results,
            CreateTokenUsageSummary(execution.RequestTokens, execution.Results));
    }

    public Task<SearchTraceResult> SearchWithTraceAsync(
        string query,
        CancellationToken cancellationToken)
    {
        return SearchWithTraceInternalAsync(query, cancellationToken);
    }

    internal async Task<SearchTraceResult> SearchWithTraceInternalAsync(
        string query,
        CancellationToken cancellationToken)
    {
        var tenantId = tenantContext.RequireTenant();
        var steps = new List<PipelineTraceStep>();
        var totalStopwatch = Stopwatch.StartNew();

        var execution = await SearchInternalAsync(
            tenantId,
            query,
            includeTrace: true,
            cancellationToken,
            steps);

        totalStopwatch.Stop();

        return new SearchTraceResult(
            query,
            execution.Results,
            steps,
            totalStopwatch.ElapsedMilliseconds,
            CreateTokenUsageSummary(execution.RequestTokens, execution.Results));
    }

    private async Task<SearchExecutionResult> SearchInternalAsync(
        Guid tenantId,
        string query,
        bool includeTrace,
        CancellationToken cancellationToken,
        List<PipelineTraceStep>? steps = null)
    {
        if (!aiAvailabilityState.IsConfigured)
        {
            throw new AiNotConfiguredException();
        }

        var embedStopwatch = Stopwatch.StartNew();
        var embeddingResult = await embeddingGenerator.GenerateAsync(query, cancellationToken);
        embedStopwatch.Stop();
        var requestTokens = embeddingResult.TokenCount;

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
                    Dimensions = embeddingResult.Values.Length,
                    TokenCount = embeddingResult.TokenCount,
                    VectorPreview = embeddingResult.Values.Take(8).Select(value => Math.Round(value, 6)).ToArray()
                }));
        }

        var vector = new Vector(embeddingResult.Values);
        var retrievalTopK = options.RetrievalTopK;
        var finalTopK = options.FinalTopK;

        var searchStopwatch = Stopwatch.StartNew();
        var vectorResults = await chunkRepository.SearchVectorAsync(
            tenantId,
            vector,
            retrievalTopK,
            cancellationToken);

        var filteredVectorResults = vectorResults
            .Where(result => result.Score >= options.MinSimilarityScore)
            .ToList();

        IReadOnlyList<SearchResult> keywordResults = Array.Empty<SearchResult>();
        if (options.HybridSearchEnabled)
        {
            keywordResults = await chunkRepository.SearchKeywordAsync(
                tenantId,
                query,
                retrievalTopK,
                cancellationToken);
        }

        searchStopwatch.Stop();

        if (includeTrace && steps is not null)
        {
            steps.Add(new PipelineTraceStep(
                Order: 2,
                Name: "search.vector",
                Description: "Find the closest document chunks in pgvector for the tenant.",
                DurationMs: searchStopwatch.ElapsedMilliseconds,
                Input: new
                {
                    TenantId = tenantId,
                    RetrievalTopK = retrievalTopK,
                    MinSimilarityScore = options.MinSimilarityScore
                },
                Output: new
                {
                    RawResultCount = vectorResults.Count,
                    FilteredResultCount = filteredVectorResults.Count,
                    Results = vectorResults.Select(result => new
                    {
                        result.DocumentId,
                        result.DocumentName,
                        result.ChunkIndex,
                        Score = Math.Round(result.Score, 4),
                        result.EmbeddingTokenCount,
                        ContentPreview = Truncate(result.Content, 160)
                    }).ToList()
                }));
        }

        IReadOnlyList<RankedChunkHit> fusedResults;

        if (options.HybridSearchEnabled)
        {
            if (includeTrace && steps is not null)
            {
                steps.Add(new PipelineTraceStep(
                    Order: 3,
                    Name: "search.keyword",
                    Description: "Score chunks by keyword overlap for hybrid retrieval.",
                    DurationMs: 0,
                    Input: new { Query = query, RetrievalTopK = retrievalTopK },
                    Output: keywordResults.Select(result => new
                    {
                        result.DocumentId,
                        result.DocumentName,
                        result.ChunkIndex,
                        Score = Math.Round(result.Score, 4),
                        result.EmbeddingTokenCount,
                        ContentPreview = Truncate(result.Content, 160)
                    }).ToList()));
            }

            fusedResults = HybridSearchMerger.Merge(
                ToRankedHits(filteredVectorResults),
                ToRankedHits(keywordResults),
                options.RrfK);
        }
        else if (filteredVectorResults.Count > 0)
        {
            fusedResults = ToRankedHits(filteredVectorResults);
        }
        else
        {
            fusedResults = ToRankedHits(
                await chunkRepository.SearchKeywordAsync(tenantId, query, retrievalTopK, cancellationToken));
        }

        if (includeTrace && steps is not null)
        {
            steps.Add(new PipelineTraceStep(
                Order: 4,
                Name: "search.hybrid_merge",
                Description: "Merge vector and keyword rankings with reciprocal rank fusion.",
                DurationMs: 0,
                Input: new { options.HybridSearchEnabled, options.RrfK },
                Output: fusedResults.Take(retrievalTopK).Select(result => new
                {
                    result.DocumentId,
                    result.DocumentName,
                    result.ChunkIndex,
                    Score = Math.Round(result.Score, 4),
                    result.EmbeddingTokenCount,
                    ContentPreview = Truncate(result.Content, 160)
                }).ToList()));
        }

        var expansionStopwatch = Stopwatch.StartNew();
        var expandedResults = await ExpandNeighborsAsync(tenantId, fusedResults, cancellationToken);
        expansionStopwatch.Stop();

        if (includeTrace && steps is not null)
        {
            steps.Add(new PipelineTraceStep(
                Order: 5,
                Name: "search.expand_neighbors",
                Description: "Include adjacent chunks around high-ranking matches.",
                DurationMs: expansionStopwatch.ElapsedMilliseconds,
                Input: new { options.NeighborExpansionRadius },
                Output: expandedResults.Take(retrievalTopK).Select(result => new
                {
                    result.DocumentId,
                    result.DocumentName,
                    result.ChunkIndex,
                    Score = Math.Round(result.Score, 4),
                    result.EmbeddingTokenCount,
                    ContentPreview = Truncate(result.Content, 160)
                }).ToList()));
        }

        var rerankStopwatch = Stopwatch.StartNew();
        var rerankCandidates = expandedResults
            .Take(options.RerankingCandidateLimit)
            .Select(hit => new RankedChunkCandidate(
                hit.DocumentId,
                hit.DocumentName,
                hit.FileName,
                hit.ChunkIndex,
                hit.Content,
                hit.Score,
                hit.EmbeddingTokenCount))
            .ToList();

        var rerankResult = await chunkReranker.RerankAsync(
            query,
            rerankCandidates,
            finalTopK,
            cancellationToken);
        rerankStopwatch.Stop();
        requestTokens += rerankResult.TokenUsage.TotalTokens;

        var rerankedHits = rerankResult.Candidates
            .Select(candidate => new RankedChunkHit(
                candidate.DocumentId,
                candidate.DocumentName,
                candidate.FileName,
                candidate.ChunkIndex,
                candidate.Content,
                candidate.Score,
                candidate.EmbeddingTokenCount))
            .ToList();

        var finalHits = options.ContiguousGapFillEnabled
            ? ChunkExpansionHelper.FillContiguousGaps(
                rerankedHits,
                expandedResults,
                finalTopK + options.ContiguousGapFillMaxExtra)
            : rerankedHits;

        if (includeTrace && steps is not null)
        {
            steps.Add(new PipelineTraceStep(
                Order: 6,
                Name: "search.rerank",
                Description: "Re-score candidate chunks with the chat model before returning final context.",
                DurationMs: rerankStopwatch.ElapsedMilliseconds,
                Input: new
                {
                    options.RerankingEnabled,
                    options.RerankingCandidateLimit,
                    FinalTopK = finalTopK,
                    options.ContiguousGapFillEnabled,
                    options.ContiguousGapFillMaxExtra
                },
                Output: new
                {
                    TokenUsage = rerankResult.TokenUsage,
                    Results = finalHits.Select(result => new
                    {
                        result.DocumentId,
                        result.DocumentName,
                        result.ChunkIndex,
                        Score = Math.Round(result.Score, 4),
                        result.EmbeddingTokenCount,
                        ContentPreview = Truncate(result.Content, 160)
                    }).ToList()
                }));
        }

        var results = finalHits
            .Select(candidate => new SearchResult(
                candidate.DocumentId,
                candidate.DocumentName,
                candidate.ChunkIndex,
                candidate.Content,
                candidate.Score,
                candidate.EmbeddingTokenCount))
            .ToList();

        return new SearchExecutionResult(results, requestTokens);
    }

    private async Task<IReadOnlyList<RankedChunkHit>> ExpandNeighborsAsync(
        Guid tenantId,
        IReadOnlyList<RankedChunkHit> seeds,
        CancellationToken cancellationToken)
    {
        if (options.NeighborExpansionRadius <= 0 || seeds.Count == 0)
        {
            return seeds;
        }

        var seedLocators = seeds
            .Select(hit => new ChunkLocator(hit.DocumentId, hit.ChunkIndex))
            .ToList();

        var neighborLocators = ChunkExpansionHelper.CollectNeighborLocators(
            seedLocators,
            options.NeighborExpansionRadius);

        var missingLocators = neighborLocators
            .Where(locator => seeds.All(seed =>
                seed.DocumentId != locator.DocumentId || seed.ChunkIndex != locator.ChunkIndex))
            .ToList();

        if (missingLocators.Count == 0)
        {
            return seeds;
        }

        var neighborChunks = await chunkRepository.GetChunksByLocatorsAsync(
            tenantId,
            missingLocators,
            cancellationToken);

        return ChunkExpansionHelper.Expand(
            seeds,
            ToRankedHits(neighborChunks),
            options.NeighborExpansionRadius);
    }

    private static IReadOnlyList<RankedChunkHit> ToRankedHits(IReadOnlyList<SearchResult> results)
    {
        return results
            .Select(result => new RankedChunkHit(
                result.DocumentId,
                result.DocumentName,
                null,
                result.ChunkIndex,
                result.Content,
                result.Score,
                result.EmbeddingTokenCount))
            .ToList();
    }

    private static TokenUsageSummary CreateTokenUsageSummary(
        int requestTokens,
        IReadOnlyList<SearchResult> results)
    {
        return new TokenUsageSummary(requestTokens, SumIndexedTokens(results));
    }

    private static int SumIndexedTokens(IReadOnlyList<SearchResult> results)
    {
        return results
            .DistinctBy(result => (result.DocumentId, result.ChunkIndex))
            .Sum(result => result.EmbeddingTokenCount);
    }

    private static string Truncate(string value, int maxLength)
    {
        if (value.Length <= maxLength)
        {
            return value;
        }

        return value[..maxLength] + "...";
    }

    private sealed record SearchExecutionResult(
        IReadOnlyList<SearchResult> Results,
        int RequestTokens);
}
