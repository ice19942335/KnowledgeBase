using KnowledgeBase.Contracts;
using KnowledgeBase.Document.Application;
using KnowledgeBase.Document.Domain;
using KnowledgeBase.SharedKernel.Messaging;
using KnowledgeBase.SharedKernel.Storage;
using KnowledgeBase.Tenancy;
using NSubstitute;
using NSubstitute.ExceptionExtensions;

namespace KnowledgeBase.UnitTests.Documents;

public sealed class DocumentAppServiceBatchTests
{
    private readonly IDocumentRepository repository = Substitute.For<IDocumentRepository>();
    private readonly IFileStorage fileStorage = Substitute.For<IFileStorage>();
    private readonly IEventPublisher eventPublisher = Substitute.For<IEventPublisher>();
    private readonly ITenantContext tenantContext = Substitute.For<ITenantContext>();
    private readonly Guid tenantId = Guid.NewGuid();

    public DocumentAppServiceBatchTests()
    {
        tenantContext.RequireTenant().Returns(tenantId);
        fileStorage.SaveAsync(Arg.Any<Stream>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(call => $"blobs/{call.Arg<string>()}");
    }

    private DocumentAppService CreateSut() => new(
        repository,
        fileStorage,
        eventPublisher,
        tenantContext);

    [Fact]
    public async Task UploadBatchAsync_UploadsEveryNonEmptyFile()
    {
        var sut = CreateSut();
        var requests = new[]
        {
            CreateRequest("policy-a.md", "Policy A"),
            CreateRequest("policy-b.md", "Policy B"),
        };

        var result = await sut.UploadBatchAsync(requests, CancellationToken.None);

        Assert.Equal(2, result.SucceededCount);
        Assert.Equal(0, result.FailedCount);
        await repository.Received(2).AddAsync(Arg.Any<StoredDocument>(), Arg.Any<CancellationToken>());
        await eventPublisher.Received(2).PublishAsync(
            Arg.Any<DocumentUploaded>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task UploadBatchAsync_SkipsEmptyFilesAndContinuesWithOthers()
    {
        var sut = CreateSut();
        var requests = new[]
        {
            CreateRequest("empty.md", "Empty", contentLength: 0),
            CreateRequest("valid.md", "Valid"),
        };

        var result = await sut.UploadBatchAsync(requests, CancellationToken.None);

        Assert.Equal(1, result.SucceededCount);
        Assert.Equal(1, result.FailedCount);
        Assert.Equal("empty.md", result.Results[0].FileName);
        Assert.Null(result.Results[0].Document);
        Assert.NotNull(result.Results[0].Error);
        Assert.NotNull(result.Results[1].Document);
    }

    [Fact]
    public async Task UploadBatchAsync_RecordsFailureWhenSingleUploadThrows()
    {
        fileStorage.SaveAsync(Arg.Any<Stream>(), "broken.md", Arg.Any<CancellationToken>())
            .ThrowsAsync(new InvalidOperationException("Storage unavailable"));

        var sut = CreateSut();
        var requests = new[]
        {
            CreateRequest("broken.md", "Broken"),
            CreateRequest("valid.md", "Valid"),
        };

        var result = await sut.UploadBatchAsync(requests, CancellationToken.None);

        Assert.Equal(1, result.SucceededCount);
        Assert.Equal(1, result.FailedCount);
        Assert.Equal("Storage unavailable", result.Results[0].Error);
        Assert.NotNull(result.Results[1].Document);
    }

    private static UploadFileRequest CreateRequest(
        string fileName,
        string name,
        long contentLength = 3) =>
        new(new MemoryStream([1, 2, 3]), name, fileName, "text/markdown", contentLength);
}
