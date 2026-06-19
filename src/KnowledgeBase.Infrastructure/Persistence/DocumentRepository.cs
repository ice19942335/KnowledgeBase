using KnowledgeBase.Application.Abstractions;
using KnowledgeBase.Domain.Documents;
using Microsoft.EntityFrameworkCore;

namespace KnowledgeBase.Infrastructure.Persistence;

public sealed class DocumentRepository : IDocumentRepository
{
    private readonly KnowledgeBaseDbContext context;

    public DocumentRepository(KnowledgeBaseDbContext context)
    {
        this.context = context;
    }

    public async Task AddAsync(Document document, CancellationToken cancellationToken)
    {
        await context.Documents.AddAsync(document, cancellationToken);
    }

    public async Task<Document?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        return await context.Documents
            .Include(document => document.Chunks)
            .FirstOrDefaultAsync(document => document.Id == id, cancellationToken);
    }

    public async Task<IReadOnlyList<Document>> GetAllAsync(CancellationToken cancellationToken)
    {
        return await context.Documents
            .Include(document => document.Chunks)
            .OrderByDescending(document => document.UploadedAtUtc)
            .ToListAsync(cancellationToken);
    }

    public void Remove(Document document)
    {
        context.Documents.Remove(document);
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken)
    {
        return context.SaveChangesAsync(cancellationToken);
    }
}
