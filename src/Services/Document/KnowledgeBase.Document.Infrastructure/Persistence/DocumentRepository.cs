using KnowledgeBase.Document.Application;
using KnowledgeBase.Document.Domain;
using Microsoft.EntityFrameworkCore;

namespace KnowledgeBase.Document.Infrastructure.Persistence;

public sealed class DocumentRepository : IDocumentRepository
{
    private readonly DocumentDbContext dbContext;

    public DocumentRepository(DocumentDbContext dbContext)
    {
        this.dbContext = dbContext;
    }

    public async Task AddAsync(StoredDocument document, CancellationToken cancellationToken)
    {
        await dbContext.Documents.AddAsync(document, cancellationToken);
    }

    public Task<StoredDocument?> GetAsync(Guid tenantId, Guid documentId, CancellationToken cancellationToken)
    {
        return dbContext.Documents
            .FirstOrDefaultAsync(d => d.TenantId == tenantId && d.Id == documentId, cancellationToken);
    }

    public async Task<IReadOnlyList<StoredDocument>> ListAsync(Guid tenantId, CancellationToken cancellationToken)
    {
        return await dbContext.Documents
            .Where(d => d.TenantId == tenantId)
            .OrderByDescending(d => d.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public Task RemoveAsync(StoredDocument document, CancellationToken cancellationToken)
    {
        dbContext.Documents.Remove(document);
        return Task.CompletedTask;
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken)
    {
        return dbContext.SaveChangesAsync(cancellationToken);
    }
}
