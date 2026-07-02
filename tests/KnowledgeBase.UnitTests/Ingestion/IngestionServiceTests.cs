using KnowledgeBase.Ai;
using KnowledgeBase.Contracts;
using KnowledgeBase.Ingestion.Application;
using KnowledgeBase.Ingestion.Application.Abstractions;
using KnowledgeBase.SharedKernel.Storage;
using KnowledgeBase.SharedKernel.TextProcessing;
using NSubstitute;
using Xunit;

namespace KnowledgeBase.UnitTests.Ingestion;

public sealed class IngestionServiceTests
{
    private readonly IFileStorage fileStorage = Substitute.For<IFileStorage>();
    private readonly ITextExtractionService textExtractionService = Substitute.For<ITextExtractionService>();
    private readonly ITextChunker textChunker = Substitute.For<ITextChunker>();
    private readonly IEmbeddingGenerator embeddingGenerator = Substitute.For<IEmbeddingGenerator>();
    private readonly IDocumentSummaryGenerator documentSummaryGenerator = Substitute.For<IDocumentSummaryGenerator>();
    private readonly IContextualEmbeddingFormatter contextualEmbeddingFormatter = Substitute.For<IContextualEmbeddingFormatter>();
    private readonly IAiAvailabilityState aiAvailabilityState = Substitute.For<IAiAvailabilityState>();

    [Fact]
    public async Task IngestAsync_StoresEmbeddingTokenCountPerChunk()
    {
        aiAvailabilityState.IsConfigured.Returns(true);
        fileStorage.OpenAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(new MemoryStream("policy text"u8.ToArray()));
        textExtractionService.ExtractAsync(
                Arg.Any<Stream>(),
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<CancellationToken>())
            .Returns("Employees receive twenty five vacation days.");
        textChunker.Split(Arg.Any<string>())
            .Returns(
            [
                new TextChunk("chunk one", null),
                new TextChunk("chunk two", null)
            ]);
        documentSummaryGenerator.GenerateAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns("summary");
        contextualEmbeddingFormatter.Format(Arg.Any<ContextualEmbeddingRequest>())
            .Returns(call => call.Arg<ContextualEmbeddingRequest>().Content);
        embeddingGenerator.GenerateAsync(Arg.Any<IReadOnlyList<string>>(), Arg.Any<CancellationToken>())
            .Returns(
            [
                new EmbeddingResult([0.1f, 0.2f], 42),
                new EmbeddingResult([0.3f, 0.4f], 55)
            ]);

        var sut = new IngestionService(
            fileStorage,
            textExtractionService,
            textChunker,
            embeddingGenerator,
            documentSummaryGenerator,
            contextualEmbeddingFormatter,
            aiAvailabilityState);

        var message = new DocumentUploaded(
            Guid.NewGuid(),
            Guid.NewGuid(),
            "HR Policy",
            "policy.txt",
            "text/plain",
            "storage/path");

        var chunks = await sut.IngestAsync(message, CancellationToken.None);

        Assert.Equal(2, chunks.Count);
        Assert.Equal(42, chunks[0].EmbeddingTokenCount);
        Assert.Equal(55, chunks[1].EmbeddingTokenCount);
    }
}
