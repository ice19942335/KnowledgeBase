using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace KnowledgeBase.Document.Infrastructure.Persistence;

/// <summary>
/// Design-time factory so EF Core tooling can create migrations without the full
/// application host (Aspire) being available.
/// </summary>
public sealed class DocumentDbContextFactory : IDesignTimeDbContextFactory<DocumentDbContext>
{
    public DocumentDbContext CreateDbContext(string[] args)
    {
        var options = new DbContextOptionsBuilder<DocumentDbContext>()
            .UseNpgsql("Host=localhost;Database=documentdb;Username=postgres;Password=postgres")
            .Options;

        return new DocumentDbContext(options);
    }
}
