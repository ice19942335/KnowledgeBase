using KnowledgeBase.Document.Domain;
using Microsoft.EntityFrameworkCore;

namespace KnowledgeBase.Document.Infrastructure.Persistence;

public sealed class DocumentDbContext : DbContext
{
    public DocumentDbContext(DbContextOptions<DocumentDbContext> options)
        : base(options)
    {
    }

    public DbSet<StoredDocument> Documents => Set<StoredDocument>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<StoredDocument>(entity =>
        {
            entity.ToTable("documents");
            entity.HasKey(d => d.Id);

            entity.Property(d => d.Name).HasMaxLength(512).IsRequired();
            entity.Property(d => d.FileName).HasMaxLength(512).IsRequired();
            entity.Property(d => d.ContentType).HasMaxLength(256).IsRequired();
            entity.Property(d => d.StoragePath).HasMaxLength(1024).IsRequired();
            entity.Property(d => d.Status).HasConversion<int>();
            entity.Property(d => d.Error).HasMaxLength(2048);

            entity.HasIndex(d => new { d.TenantId, d.CreatedAt });
        });
    }
}
