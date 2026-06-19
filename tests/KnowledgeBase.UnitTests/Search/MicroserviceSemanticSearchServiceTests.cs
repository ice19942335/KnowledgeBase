using KnowledgeBase.Ai;
using KnowledgeBase.Search.Application;
using KnowledgeBase.Tenancy;
using Microsoft.Extensions.Options;
using NSubstitute;
using Pgvector;

namespace KnowledgeBase.UnitTests.Search;

public sealed class MicroserviceSemanticSearchServiceTests
{
    private readonly IChunkRepository chunkRepository = Substitute.For<IChunkRepository>();
    private readonly IEmbeddingGenerator embeddingGenerator = Substitute.For<IEmbeddingGenerator>();
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
    public async Task SearchAsync_WhenVectorMatchesBelowThreshold_FallsBackToKeywordSearch()
    {
        var tenantId = Guid.NewGuid();
        tenantContext.RequireTenant().Returns(tenantId);
        aiAvailabilityState.IsConfigured.Returns(true);

        embeddingGenerator.GenerateAsync("annual leave", Arg.Any<CancellationToken>())
            .Returns(new float[] { 0.1f, 0.2f, 0.3f });

        chunkRepository.SearchVectorAsync(
                tenantId,
                Arg.Any<Vector>(),
                5,
                Arg.Any<CancellationToken>())
            .Returns(new[]
            {
                new SearchResult(Guid.NewGuid(), "Irrelevant", 0, "Unrelated content", 0.05)
            });

        chunkRepository.SearchKeywordAsync(tenantId, "annual leave", 5, Arg.Any<CancellationToken>())
            .Returns(new[]
            {
                new SearchResult(Guid.NewGuid(), "HR Policy", 1, "28 calendar days of annual leave", 1.0)
            });

        var sut = CreateSut();

        var results = await sut.SearchAsync("annual leave", CancellationToken.None);

        var result = Assert.Single(results);
        Assert.Equal("HR Policy", result.DocumentName);
        await chunkRepository.Received(1).SearchKeywordAsync(
            tenantId,
            "annual leave",
            5,
            Arg.Any<CancellationToken>());
    }

    private SemanticSearchService CreateSut()
    {
        return new SemanticSearchService(
            chunkRepository,
            embeddingGenerator,
            tenantContext,
            aiAvailabilityState,
            Options.Create(new SearchOptions { TopK = 5, MinSimilarityScore = 0.35 }));
    }
}
