using KnowledgeBase.Contracts;
using KnowledgeBase.Document.Domain;
using KnowledgeBase.SharedKernel.Messaging;
using KnowledgeBase.SharedKernel.Storage;
using KnowledgeBase.Tenancy;

namespace KnowledgeBase.Document.Application;

public sealed class DocumentAppService
{
    private readonly IDocumentRepository repository;
    private readonly IFileStorage fileStorage;
    private readonly IEventPublisher eventPublisher;
    private readonly ITenantContext tenantContext;

    public DocumentAppService(
        IDocumentRepository repository,
        IFileStorage fileStorage,
        IEventPublisher eventPublisher,
        ITenantContext tenantContext)
    {
        this.repository = repository;
        this.fileStorage = fileStorage;
        this.eventPublisher = eventPublisher;
        this.tenantContext = tenantContext;
    }

    public async Task<DocumentDto> UploadAsync(
        Stream content,
        string name,
        string fileName,
        string contentType,
        CancellationToken cancellationToken)
    {
        var tenantId = tenantContext.RequireTenant();

        var storagePath = await fileStorage.SaveAsync(content, fileName, cancellationToken);

        var document = new StoredDocument(tenantId, name, fileName, contentType, storagePath);

        await repository.AddAsync(document, cancellationToken);
        await repository.SaveChangesAsync(cancellationToken);

        document.MarkProcessing();
        await repository.SaveChangesAsync(cancellationToken);

        await eventPublisher.PublishAsync(
            new DocumentUploaded(
                document.Id,
                tenantId,
                document.Name,
                document.FileName,
                document.ContentType,
                storagePath),
            cancellationToken);

        return DocumentDto.From(document);
    }

    public async Task<IReadOnlyList<DocumentDto>> ListAsync(CancellationToken cancellationToken)
    {
        var tenantId = tenantContext.RequireTenant();
        var documents = await repository.ListAsync(tenantId, cancellationToken);
        return documents.Select(DocumentDto.From).ToList();
    }

    public async Task<DocumentDto?> GetAsync(Guid documentId, CancellationToken cancellationToken)
    {
        var tenantId = tenantContext.RequireTenant();
        var document = await repository.GetAsync(tenantId, documentId, cancellationToken);
        return document is null ? null : DocumentDto.From(document);
    }

    public async Task<DocumentContent?> GetContentAsync(Guid documentId, CancellationToken cancellationToken)
    {
        var tenantId = tenantContext.RequireTenant();
        var document = await repository.GetAsync(tenantId, documentId, cancellationToken);

        if (document is null)
        {
            return null;
        }

        var content = await fileStorage.OpenAsync(document.StoragePath, cancellationToken);

        return new DocumentContent(document.FileName, document.ContentType, content);
    }

    public async Task<bool> DeleteAsync(Guid documentId, CancellationToken cancellationToken)
    {
        var tenantId = tenantContext.RequireTenant();

        var document = await repository.GetAsync(tenantId, documentId, cancellationToken);
        if (document is null)
        {
            return false;
        }

        await repository.RemoveAsync(document, cancellationToken);
        await repository.SaveChangesAsync(cancellationToken);
        await fileStorage.DeleteAsync(document.StoragePath, cancellationToken);

        await eventPublisher.PublishAsync(
            new DocumentDeleted(document.Id, tenantId),
            cancellationToken);

        return true;
    }

    public async Task ApplyProcessingResultAsync(
        Guid tenantId,
        Guid documentId,
        int chunkCount,
        CancellationToken cancellationToken)
    {
        var document = await repository.GetAsync(tenantId, documentId, cancellationToken);
        if (document is null)
        {
            return;
        }

        document.MarkProcessed(chunkCount);
        await repository.SaveChangesAsync(cancellationToken);
    }

    public async Task ApplyProcessingFailureAsync(
        Guid tenantId,
        Guid documentId,
        string reason,
        CancellationToken cancellationToken)
    {
        var document = await repository.GetAsync(tenantId, documentId, cancellationToken);
        if (document is null)
        {
            return;
        }

        document.MarkFailed(reason);
        await repository.SaveChangesAsync(cancellationToken);
    }
}
