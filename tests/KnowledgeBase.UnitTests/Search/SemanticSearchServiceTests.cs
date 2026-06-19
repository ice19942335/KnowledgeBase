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

    private SemanticSearchService CreateSut(SearchOptions? options = null) => new(
        embeddingGenerator,
        searchRepository,
        Options.Create(options ?? new SearchOptions { DefaultTopK = 5, MaxTopK = 20 }));

    [Fact]
    public async Task SearchAsync_WithEmptyQuery_Throws()
    {
        var sut = CreateSut();

        await Assert.ThrowsAsync<ArgumentException>(
            () => sut.SearchAsync("  ", null, CancellationToken.None));
    }

    [Fact]
    public async Task SearchAsync_WhenTopKMissing_UsesDefault()
    {
        embeddingGenerator.GenerateAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(new[] { 0.1f });
        searchRepository.SearchAsync(Arg.Any<float[]>(), Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns(Array.Empty<ChunkMatch>());

        var sut = CreateSut(new SearchOptions { DefaultTopK = 7, MaxTopK = 20 });

        await sut.SearchAsync("vacation", null, CancellationToken.None);

        await searchRepository.Received(1).SearchAsync(Arg.Any<float[]>(), 7, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task SearchAsync_WhenTopKExceedsMax_IsClamped()
    {
        embeddingGenerator.GenerateAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(new[] { 0.1f });
        searchRepository.SearchAsync(Arg.Any<float[]>(), Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns(Array.Empty<ChunkMatch>());

        var sut = CreateSut(new SearchOptions { DefaultTopK = 5, MaxTopK = 10 });

        await sut.SearchAsync("vacation", 999, CancellationToken.None);

        await searchRepository.Received(1).SearchAsync(Arg.Any<float[]>(), 10, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task SearchAsync_MapsMatchesToDtos()
    {
        embeddingGenerator.GenerateAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(new[] { 0.1f });

        var documentId = Guid.NewGuid();
        searchRepository.SearchAsync(Arg.Any<float[]>(), Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns(new[]
            {
                new ChunkMatch(documentId, "HR Policy", "hr.pdf", 0, "25 vacation days", 0.92)
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
