using KnowledgeBase.Ai;
using KnowledgeBase.Search.Application;
using KnowledgeBase.SharedKernel.Retrieval;
using KnowledgeBase.Tenancy;
using Microsoft.Extensions.Options;
using NSubstitute;
using Pgvector;

namespace KnowledgeBase.UnitTests.Search;

public sealed class MicroserviceSemanticSearchServiceTests
{
    private readonly IChunkRepository chunkRepository = Substitute.For<IChunkRepository>();
    private readonly IEmbeddingGenerator embeddingGenerator = Substitute.For<IEmbeddingGenerator>();
    private readonly IChunkReranker chunkReranker = Substitute.For<IChunkReranker>();
    private readonly ITenantContext tenantContext = Substitute.For<ITenantContext>();
    private readonly IAiAvailabilityState aiAvailabilityState = Substitute.For<IAiAvailabilityState>();

    [Fact]
    public async Task SearchAsync_WithoutApiKey_ThrowsAiNotConfiguredException()
    {
        var tenantId = Guid.NewGuid();
        tenantContext.RequireTenant().Returns(tenantId);
        aiAvailabilityState.IsConfigured.Returns(false);

        var sut = CreateSut();

        await Assert.ThrowsAsync<AiNotConfiguredException>(
            () => sut.SearchAsync("annual leave", CancellationToken.None));
    }

    [Fact]
    public async Task SearchAsync_WithHybridSearch_MergesKeywordResults()
    {
        var tenantId = Guid.NewGuid();
        tenantContext.RequireTenant().Returns(tenantId);
        aiAvailabilityState.IsConfigured.Returns(true);

        embeddingGenerator.GenerateAsync("annual leave", Arg.Any<CancellationToken>())
            .Returns(new float[] { 0.1f, 0.2f, 0.3f });

        chunkRepository.SearchVectorAsync(
                tenantId,
                Arg.Any<Vector>(),
                20,
                Arg.Any<CancellationToken>())
            .Returns(new[]
            {
                new SearchResult(Guid.NewGuid(), "Irrelevant", 0, "Unrelated content", 0.05)
            });

        chunkRepository.SearchKeywordAsync(tenantId, "annual leave", 20, Arg.Any<CancellationToken>())
            .Returns(new[]
            {
                new SearchResult(Guid.NewGuid(), "HR Policy", 1, "28 calendar days of annual leave", 1.0)
            });

        chunkRepository.GetChunksByLocatorsAsync(
                tenantId,
                Arg.Any<IReadOnlyList<ChunkLocator>>(),
                Arg.Any<CancellationToken>())
            .Returns(Array.Empty<SearchResult>());

        chunkReranker.RerankAsync(
                Arg.Any<string>(),
                Arg.Any<IReadOnlyList<RankedChunkCandidate>>(),
                5,
                Arg.Any<CancellationToken>())
            .Returns(call =>
            {
                var candidates = call.Arg<IReadOnlyList<RankedChunkCandidate>>();
                return candidates.OrderByDescending(candidate => candidate.Score).Take(5).ToList();
            });

        var sut = CreateSut();

        var results = await sut.SearchAsync("annual leave", CancellationToken.None);

        var result = Assert.Single(results);
        Assert.Equal("HR Policy", result.DocumentName);
        await chunkRepository.Received(1).SearchKeywordAsync(
            tenantId,
            "annual leave",
            20,
            Arg.Any<CancellationToken>());
    }

    private SemanticSearchService CreateSut()
    {
        return new SemanticSearchService(
            chunkRepository,
            embeddingGenerator,
            chunkReranker,
            tenantContext,
            aiAvailabilityState,
            Options.Create(new SearchOptions
            {
                RetrievalTopK = 20,
                FinalTopK = 5,
                MinSimilarityScore = 0.35,
                HybridSearchEnabled = true,
                NeighborExpansionRadius = 1,
                RerankingEnabled = true
            }));
    }
}
