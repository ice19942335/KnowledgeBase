using KnowledgeBase.Document.Domain;

namespace KnowledgeBase.Document.Application;

public interface IDocumentRepository
{
    Task AddAsync(StoredDocument document, CancellationToken cancellationToken);

    Task<StoredDocument?> GetAsync(Guid tenantId, Guid documentId, CancellationToken cancellationToken);

    Task<IReadOnlyList<StoredDocument>> ListAsync(Guid tenantId, CancellationToken cancellationToken);

    Task RemoveAsync(StoredDocument document, CancellationToken cancellationToken);

    Task SaveChangesAsync(CancellationToken cancellationToken);
}
