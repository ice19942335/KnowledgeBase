using KnowledgeBase.Contracts;
using KnowledgeBase.Document.Application;
using KnowledgeBase.Document.Domain;
using KnowledgeBase.SharedKernel.Messaging;
using KnowledgeBase.SharedKernel.Storage;
using KnowledgeBase.Tenancy;
using NSubstitute;

namespace KnowledgeBase.UnitTests.Documents;

public sealed class DocumentAppServiceRetryTests
{
    private readonly IDocumentRepository repository = Substitute.For<IDocumentRepository>();
    private readonly IFileStorage fileStorage = Substitute.For<IFileStorage>();
    private readonly IEventPublisher eventPublisher = Substitute.For<IEventPublisher>();
    private readonly ITenantContext tenantContext = Substitute.For<ITenantContext>();
    private readonly Guid tenantId = Guid.NewGuid();

    public DocumentAppServiceRetryTests()
    {
        tenantContext.RequireTenant().Returns(tenantId);
    }

    private DocumentAppService CreateSut() => new(
        repository,
        fileStorage,
        eventPublisher,
        tenantContext);

    [Fact]
    public async Task RetryProcessingAsync_WhenDocumentNotFound_ReturnsNotFound()
    {
        var documentId = Guid.NewGuid();
        repository.GetAsync(tenantId, documentId, Arg.Any<CancellationToken>())
            .Returns((StoredDocument?)null);

        var sut = CreateSut();

        var result = await sut.RetryProcessingAsync(documentId, CancellationToken.None);

        Assert.Equal(DocumentRetryStatus.NotFound, result.Status);
        Assert.Null(result.Document);
        await eventPublisher.DidNotReceive().PublishAsync(Arg.Any<DocumentUploaded>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task RetryProcessingAsync_WhenDocumentIsNotFailed_ReturnsNotRetryable()
    {
        var document = new StoredDocument(tenantId, "Policy", "policy.md", "text/markdown", "blobs/policy.md");
        document.MarkProcessed(3);

        repository.GetAsync(tenantId, document.Id, Arg.Any<CancellationToken>())
            .Returns(document);

        var sut = CreateSut();

        var result = await sut.RetryProcessingAsync(document.Id, CancellationToken.None);

        Assert.Equal(DocumentRetryStatus.NotRetryable, result.Status);
        Assert.Null(result.Document);
        await eventPublisher.DidNotReceive().PublishAsync(Arg.Any<DocumentUploaded>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task RetryProcessingAsync_WhenDocumentFailed_RepublishesEventAndMarksProcessing()
    {
        var document = new StoredDocument(tenantId, "Policy", "policy.md", "text/markdown", "blobs/policy.md");
        document.MarkFailed("Embedding failed");

        repository.GetAsync(tenantId, document.Id, Arg.Any<CancellationToken>())
            .Returns(document);

        var sut = CreateSut();

        var result = await sut.RetryProcessingAsync(document.Id, CancellationToken.None);

        Assert.Equal(DocumentRetryStatus.Success, result.Status);
        Assert.NotNull(result.Document);
        Assert.Equal(DocumentStatus.Processing, result.Document.Status);
        Assert.Equal(DocumentStatus.Processing, document.Status);
        Assert.Null(document.Error);

        await repository.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
        await eventPublisher.Received(1).PublishAsync(
            Arg.Is<DocumentUploaded>(evt =>
                evt.DocumentId == document.Id &&
                evt.TenantId == tenantId &&
                evt.StoragePath == "blobs/policy.md"),
            Arg.Any<CancellationToken>());
    }
}
