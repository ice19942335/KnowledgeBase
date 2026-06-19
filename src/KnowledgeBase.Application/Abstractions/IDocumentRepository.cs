using KnowledgeBase.Domain.Documents;

namespace KnowledgeBase.Application.Abstractions;

public interface IDocumentRepository
{
    Task AddAsync(Document document, CancellationToken cancellationToken);

    Task<Document?> GetByIdAsync(Guid id, CancellationToken cancellationToken);

    Task<IReadOnlyList<Document>> GetAllAsync(CancellationToken cancellationToken);

    void Remove(Document document);

    Task SaveChangesAsync(CancellationToken cancellationToken);
}
