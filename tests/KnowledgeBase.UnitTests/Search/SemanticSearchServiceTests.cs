using KnowledgeBase.Ai;
using KnowledgeBase.Application.Abstractions;
using KnowledgeBase.Application.Common.Options;
using KnowledgeBase.Application.Search;
using Microsoft.Extensions.Options;
using NSubstitute;
using Xunit;

namespace KnowledgeBase.UnitTests.Search;

public sealed class SemanticSearchServiceTests
{
    private readonly IEmbeddingGenerator embeddingGenerator = Substitute.For<IEmbeddingGenerator>();
    private readonly IChunkSearchRepository searchRepository = Substitute.For<IChunkSearchRepository>();
    private readonly IChunkReranker chunkReranker = Substitute.For<IChunkReranker>();

    private SemanticSearchService CreateSut(SearchOptions? options = null)
    {
        var retrievalPipeline = new ChunkRetrievalPipeline(
            searchRepository,
            chunkReranker,
            Options.Create(options ?? new SearchOptions
            {
                RetrievalTopK = 20,
                FinalTopK = 5,
                DefaultTopK = 5,
                MaxTopK = 20,
                HybridSearchEnabled = false,
                NeighborExpansionRadius = 0,
                RerankingEnabled = false
            }));

        return new SemanticSearchService(embeddingGenerator, retrievalPipeline);
    }

    [Fact]
    public async Task SearchAsync_WithEmptyQuery_Throws()
    {
        var sut = CreateSut();

        await Assert.ThrowsAsync<ArgumentException>(
            () => sut.SearchAsync("  ", null, CancellationToken.None));
    }

    [Fact]
    public async Task SearchAsync_WhenTopKMissing_UsesDefaultFinalTopK()
    {
        embeddingGenerator.GenerateAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(new[] { 0.1f });
        searchRepository.SearchVectorAsync(Arg.Any<float[]>(), Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns(Array.Empty<ChunkMatch>());
        searchRepository.SearchKeywordAsync(Arg.Any<string>(), Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns(Array.Empty<ChunkMatch>());

        var sut = CreateSut(new SearchOptions
        {
            RetrievalTopK = 20,
            FinalTopK = 7,
            DefaultTopK = 7,
            MaxTopK = 20,
            HybridSearchEnabled = false,
            RerankingEnabled = false
        });

        await sut.SearchAsync("vacation", null, CancellationToken.None);

        await chunkReranker.Received(1).RerankAsync(
            "vacation",
            Arg.Any<IReadOnlyList<RankedChunkCandidate>>(),
            7,
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task SearchAsync_WhenTopKExceedsMax_IsClamped()
    {
        embeddingGenerator.GenerateAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(new[] { 0.1f });
        searchRepository.SearchVectorAsync(Arg.Any<float[]>(), Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns(Array.Empty<ChunkMatch>());
        searchRepository.SearchKeywordAsync(Arg.Any<string>(), Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns(Array.Empty<ChunkMatch>());

        var sut = CreateSut(new SearchOptions
        {
            RetrievalTopK = 20,
            FinalTopK = 5,
            DefaultTopK = 5,
            MaxTopK = 10,
            HybridSearchEnabled = false,
            RerankingEnabled = false
        });

        await sut.SearchAsync("vacation", 999, CancellationToken.None);

        await chunkReranker.Received(1).RerankAsync(
            "vacation",
            Arg.Any<IReadOnlyList<RankedChunkCandidate>>(),
            10,
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task SearchAsync_MapsMatchesToDtos()
    {
        embeddingGenerator.GenerateAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(new[] { 0.1f });

        var documentId = Guid.NewGuid();
        searchRepository.SearchVectorAsync(Arg.Any<float[]>(), Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns(new[]
            {
                new ChunkMatch(documentId, "HR Policy", "hr.pdf", 0, "25 vacation days", 0.92)
            });
        chunkReranker.RerankAsync(
                Arg.Any<string>(),
                Arg.Any<IReadOnlyList<RankedChunkCandidate>>(),
                Arg.Any<int>(),
                Arg.Any<CancellationToken>())
            .Returns(call =>
            {
                var candidates = call.Arg<IReadOnlyList<RankedChunkCandidate>>();
                return candidates.Select(candidate => candidate with { Score = 0.92 }).ToList();
            });

        var sut = CreateSut();

        var results = await sut.SearchAsync("vacation", null, CancellationToken.None);

        var result = Assert.Single(results);
        Assert.Equal(documentId, result.DocumentId);
        Assert.Equal("HR Policy", result.DocumentName);
        Assert.Equal("hr.pdf", result.FileName);
        Assert.Equal(0.92, result.Score);
    }
}
