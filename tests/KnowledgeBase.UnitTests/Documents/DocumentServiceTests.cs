using KnowledgeBase.Ai;
using KnowledgeBase.Application.Abstractions;
using KnowledgeBase.Application.Common.Options;
using KnowledgeBase.Application.Documents;
using KnowledgeBase.Domain.Documents;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;

namespace KnowledgeBase.UnitTests.Documents;

public sealed class DocumentServiceTests
{
    private readonly IDocumentRepository repository = Substitute.For<IDocumentRepository>();
    private readonly ITextExtractionService extractionService = Substitute.For<ITextExtractionService>();
    private readonly IEmbeddingGenerator embeddingGenerator = Substitute.For<IEmbeddingGenerator>();
    private readonly IDocumentSummaryGenerator documentSummaryGenerator = Substitute.For<IDocumentSummaryGenerator>();
    private readonly IContextualEmbeddingFormatter contextualEmbeddingFormatter = Substitute.For<IContextualEmbeddingFormatter>();
    private readonly ITextChunker chunker;

    public DocumentServiceTests()
    {
        chunker = new TextChunker(Options.Create(new ChunkingOptions
        {
            MaxChunkSize = 40,
            OverlapSize = 10
        }));

        documentSummaryGenerator
            .GenerateAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns("Summary text");

        contextualEmbeddingFormatter
            .Format(Arg.Any<ContextualEmbeddingRequest>())
            .Returns(call => call.Arg<ContextualEmbeddingRequest>().Content);
    }

    private DocumentService CreateSut() => new(
        repository,
        extractionService,
        chunker,
        embeddingGenerator,
        documentSummaryGenerator,
        contextualEmbeddingFormatter,
        NullLogger<DocumentService>.Instance);

    private static UploadDocumentCommand CreateCommand() => new()
    {
        Content = new MemoryStream([1, 2, 3]),
        FileName = "policy.txt",
        ContentType = "text/plain",
        DisplayName = "HR Policy"
    };

    [Fact]
    public async Task UploadAsync_ProcessesDocumentAndEmbedsEveryChunk()
    {
        extractionService
            .ExtractAsync(Arg.Any<Stream>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns("Employees receive twenty five vacation days per year according to the policy document.");

        embeddingGenerator
            .GenerateAsync(Arg.Any<IReadOnlyList<string>>(), Arg.Any<CancellationToken>())
            .Returns(call =>
            {
                var inputs = call.Arg<IReadOnlyList<string>>();
                return inputs.Select(_ => new[] { 0.1f, 0.2f }).ToList();
            });

        var sut = CreateSut();

        var result = await sut.UploadAsync(CreateCommand(), CancellationToken.None);

        Assert.Equal(DocumentStatus.Processed, result.Status);
        Assert.Equal("HR Policy", result.Name);
        Assert.True(result.ChunkCount > 0);

        await embeddingGenerator.Received(1)
            .GenerateAsync(Arg.Is<IReadOnlyList<string>>(list => list.Count == result.ChunkCount), Arg.Any<CancellationToken>());
        await documentSummaryGenerator.Received(1)
            .GenerateAsync("HR Policy", Arg.Any<string>(), Arg.Any<CancellationToken>());
        await repository.Received(1).AddAsync(Arg.Any<Document>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task UploadAsync_WhenExtractionFails_MarksFailedAndRethrows()
    {
        extractionService
            .ExtractAsync(Arg.Any<Stream>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Throws(new InvalidOperationException("corrupted file"));

        var sut = CreateSut();

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => sut.UploadAsync(CreateCommand(), CancellationToken.None));

        await repository.Received().SaveChangesAsync(Arg.Any<CancellationToken>());
        await embeddingGenerator.DidNotReceive()
            .GenerateAsync(Arg.Any<IReadOnlyList<string>>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task DeleteAsync_WhenDocumentMissing_ReturnsFalse()
    {
        repository
            .GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns((Document?)null);

        var sut = CreateSut();

        var deleted = await sut.DeleteAsync(Guid.NewGuid(), CancellationToken.None);

        Assert.False(deleted);
        repository.DidNotReceive().Remove(Arg.Any<Document>());
    }
}
