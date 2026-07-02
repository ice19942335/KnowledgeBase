using KnowledgeBase.Search.Domain;
using Microsoft.EntityFrameworkCore;

namespace KnowledgeBase.Search.Infrastructure.Persistence;

public sealed class SearchDbContext : DbContext
{
    public SearchDbContext(DbContextOptions<SearchDbContext> options)
        : base(options)
    {
    }

    public DbSet<SearchableChunk> Chunks => Set<SearchableChunk>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.HasPostgresExtension("vector");

        modelBuilder.Entity<SearchableChunk>(entity =>
        {
            entity.ToTable("searchable_chunks");
            entity.HasKey(c => c.Id);

            entity.Property(c => c.DocumentName).HasMaxLength(512);
            entity.Property(c => c.Content).IsRequired();
            entity.Property(c => c.Embedding).HasColumnType("vector(1536)");
            entity.Property(c => c.EmbeddingTokenCount).HasDefaultValue(0);

            entity.HasIndex(c => c.Embedding)
                .HasMethod("hnsw")
                .HasOperators("vector_cosine_ops");

            entity.HasIndex(c => new { c.TenantId, c.DocumentId });
        });
    }
}
