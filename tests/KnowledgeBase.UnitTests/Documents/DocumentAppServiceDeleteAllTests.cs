using KnowledgeBase.Contracts;
using KnowledgeBase.Document.Application;
using KnowledgeBase.Document.Domain;
using KnowledgeBase.SharedKernel.Messaging;
using KnowledgeBase.SharedKernel.Storage;
using KnowledgeBase.Tenancy;
using NSubstitute;

namespace KnowledgeBase.UnitTests.Documents;

public sealed class DocumentAppServiceDeleteAllTests
{
    private readonly IDocumentRepository repository = Substitute.For<IDocumentRepository>();
    private readonly IFileStorage fileStorage = Substitute.For<IFileStorage>();
    private readonly IEventPublisher eventPublisher = Substitute.For<IEventPublisher>();
    private readonly ITenantContext tenantContext = Substitute.For<ITenantContext>();
    private readonly Guid tenantId = Guid.NewGuid();

    public DocumentAppServiceDeleteAllTests()
    {
        tenantContext.RequireTenant().Returns(tenantId);
    }

    private DocumentAppService CreateSut() => new(
        repository,
        fileStorage,
        eventPublisher,
        tenantContext);

    [Fact]
    public async Task DeleteAllAsync_WhenNoDocuments_ReturnsZero()
    {
        repository.ListAsync(tenantId, Arg.Any<CancellationToken>())
            .Returns(Array.Empty<StoredDocument>());

        var sut = CreateSut();

        var result = await sut.DeleteAllAsync(CancellationToken.None);

        Assert.Equal(0, result.DeletedCount);
        await repository.DidNotReceive().RemoveAsync(Arg.Any<StoredDocument>(), Arg.Any<CancellationToken>());
        await eventPublisher.DidNotReceive().PublishAsync(Arg.Any<DocumentDeleted>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task DeleteAllAsync_RemovesEveryDocumentAndPublishesEvents()
    {
        var documents = new[]
        {
            new StoredDocument(tenantId, "Policy A", "policy-a.md", "text/markdown", "blobs/policy-a.md"),
            new StoredDocument(tenantId, "Policy B", "policy-b.md", "text/markdown", "blobs/policy-b.md"),
        };

        repository.ListAsync(tenantId, Arg.Any<CancellationToken>())
            .Returns(documents);

        var sut = CreateSut();

        var result = await sut.DeleteAllAsync(CancellationToken.None);

        Assert.Equal(2, result.DeletedCount);
        await repository.Received(2).RemoveAsync(Arg.Any<StoredDocument>(), Arg.Any<CancellationToken>());
        await repository.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
        await fileStorage.Received(1).DeleteAsync("blobs/policy-a.md", Arg.Any<CancellationToken>());
        await fileStorage.Received(1).DeleteAsync("blobs/policy-b.md", Arg.Any<CancellationToken>());
        await eventPublisher.Received(2).PublishAsync(
            Arg.Is<DocumentDeleted>(evt => evt.TenantId == tenantId),
            Arg.Any<CancellationToken>());
    }
}
