using KnowledgeBase.Ai;
using KnowledgeBase.Application.Abstractions;
using KnowledgeBase.Application.Common.Options;
using KnowledgeBase.SharedKernel.Retrieval;
using Microsoft.Extensions.Options;

namespace KnowledgeBase.Application.Search;

public sealed class ChunkRetrievalPipeline
{
    private readonly IChunkSearchRepository searchRepository;
    private readonly IChunkReranker chunkReranker;
    private readonly SearchOptions options;

    public ChunkRetrievalPipeline(
        IChunkSearchRepository searchRepository,
        IChunkReranker chunkReranker,
        IOptions<SearchOptions> options)
    {
        this.searchRepository = searchRepository;
        this.chunkReranker = chunkReranker;
        this.options = options.Value;
    }

    public async Task<IReadOnlyList<ChunkMatch>> RetrieveAsync(
        string query,
        float[] queryEmbedding,
        int? requestedTopK,
        CancellationToken cancellationToken)
    {
        var retrievalTopK = options.RetrievalTopK;
        var finalTopK = ResolveFinalTopK(requestedTopK);

        var vectorResults = await searchRepository.SearchVectorAsync(
            queryEmbedding,
            retrievalTopK,
            cancellationToken);

        var filteredVectorResults = vectorResults
            .Where(result => result.Score >= options.MinSimilarityScore)
            .ToList();

        IReadOnlyList<ChunkMatch> keywordResults = Array.Empty<ChunkMatch>();
        if (options.HybridSearchEnabled)
        {
            keywordResults = await searchRepository.SearchKeywordAsync(
                query,
                retrievalTopK,
                cancellationToken);
        }

        IReadOnlyList<RankedChunkHit> fusedResults;

        if (options.HybridSearchEnabled)
        {
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
                await searchRepository.SearchKeywordAsync(query, retrievalTopK, cancellationToken));
        }

        var expandedResults = await ExpandNeighborsAsync(fusedResults, cancellationToken);

        var reranked = await chunkReranker.RerankAsync(
            query,
            expandedResults
                .Take(options.RerankingCandidateLimit)
                .Select(hit => new RankedChunkCandidate(
                    hit.DocumentId,
                    hit.DocumentName,
                    hit.FileName,
                    hit.ChunkIndex,
                    hit.Content,
                    hit.Score))
                .ToList(),
            finalTopK,
            cancellationToken);

        return reranked
            .Select(candidate => new ChunkMatch(
                candidate.DocumentId,
                candidate.DocumentName,
                candidate.FileName ?? string.Empty,
                candidate.ChunkIndex,
                candidate.Content,
                candidate.Score))
            .ToList();
    }

    private async Task<IReadOnlyList<RankedChunkHit>> ExpandNeighborsAsync(
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

        var neighborChunks = await searchRepository.GetChunksByLocatorsAsync(
            missingLocators,
            cancellationToken);

        return ChunkExpansionHelper.Expand(
            seeds,
            ToRankedHits(neighborChunks),
            options.NeighborExpansionRadius);
    }

    private int ResolveFinalTopK(int? requested)
    {
        if (requested is null or <= 0)
        {
            return options.FinalTopK;
        }

        return Math.Min(requested.Value, options.MaxTopK);
    }

    private static IReadOnlyList<RankedChunkHit> ToRankedHits(IReadOnlyList<ChunkMatch> results)
    {
        return results
            .Select(result => new RankedChunkHit(
                result.DocumentId,
                result.DocumentName,
                result.FileName,
                result.ChunkIndex,
                result.Content,
                result.Score))
            .ToList();
    }
}
