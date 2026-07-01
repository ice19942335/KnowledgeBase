using KnowledgeBase.Ai;
using KnowledgeBase.Application.Abstractions;
using KnowledgeBase.Application.Chat;
using KnowledgeBase.Application.Common.Options;
using KnowledgeBase.Application.Search;
using Microsoft.Extensions.Options;
using NSubstitute;
using Xunit;

namespace KnowledgeBase.UnitTests.Chat;

public sealed class RagChatServiceTests
{
    private readonly IEmbeddingGenerator embeddingGenerator = Substitute.For<IEmbeddingGenerator>();
    private readonly IChunkSearchRepository searchRepository = Substitute.For<IChunkSearchRepository>();
    private readonly IChunkReranker chunkReranker = Substitute.For<IChunkReranker>();
    private readonly IChatCompletionService chatCompletionService = Substitute.For<IChatCompletionService>();

    private RagChatService CreateSut(RagOptions? options = null)
    {
        var retrievalPipeline = new ChunkRetrievalPipeline(
            searchRepository,
            chunkReranker,
            Options.Create(new SearchOptions
            {
                RetrievalTopK = 20,
                FinalTopK = 5,
                HybridSearchEnabled = false,
                NeighborExpansionRadius = 0,
                RerankingEnabled = false
            }));

        return new RagChatService(
            embeddingGenerator,
            retrievalPipeline,
            chatCompletionService,
            Options.Create(options ?? new RagOptions { ContextChunkCount = 5, NoAnswerResponse = "I don't know." }));
    }

    [Fact]
    public async Task AskAsync_WhenNoContext_ReturnsNoAnswerAndSkipsLlm()
    {
        embeddingGenerator.GenerateAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(new[] { 0.1f });
        searchRepository.SearchVectorAsync(Arg.Any<float[]>(), Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns(Array.Empty<ChunkMatch>());
        searchRepository.SearchKeywordAsync(Arg.Any<string>(), Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns(Array.Empty<ChunkMatch>());

        var sut = CreateSut();

        var answer = await sut.AskAsync("How many vacation days?", CancellationToken.None);

        Assert.Equal("I don't know.", answer.Answer);
        Assert.Empty(answer.Sources);
        await chatCompletionService.DidNotReceive()
            .CompleteAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task AskAsync_WithContext_CallsLlmAndReturnsDistinctSources()
    {
        var firstDocument = Guid.NewGuid();
        var secondDocument = Guid.NewGuid();

        embeddingGenerator.GenerateAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(new[] { 0.1f });
        searchRepository.SearchVectorAsync(Arg.Any<float[]>(), Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns(new[]
            {
                new ChunkMatch(firstDocument, "HR Policy", "hr.pdf", 0, "25 vacation days", 0.95),
                new ChunkMatch(firstDocument, "HR Policy", "hr.pdf", 1, "annual leave", 0.90),
                new ChunkMatch(secondDocument, "Handbook", "handbook.pdf", 0, "leave rules", 0.80)
            });
        chunkReranker.RerankAsync(
                Arg.Any<string>(),
                Arg.Any<IReadOnlyList<RankedChunkCandidate>>(),
                Arg.Any<int>(),
                Arg.Any<CancellationToken>())
            .Returns(call => call.Arg<IReadOnlyList<RankedChunkCandidate>>());
        chatCompletionService.CompleteAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns("Employees receive 25 vacation days.");

        var sut = CreateSut();

        var answer = await sut.AskAsync("How many vacation days?", CancellationToken.None);

        Assert.Equal("Employees receive 25 vacation days.", answer.Answer);
        Assert.Equal(2, answer.Sources.Count);
        Assert.Contains(answer.Sources, source => source.DocumentId == firstDocument);
        Assert.Contains(answer.Sources, source => source.DocumentId == secondDocument);
        await chatCompletionService.Received(1)
            .CompleteAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task AskAsync_WithEmptyQuestion_Throws()
    {
        var sut = CreateSut();

        await Assert.ThrowsAsync<ArgumentException>(
            () => sut.AskAsync(" ", CancellationToken.None));
    }
}
