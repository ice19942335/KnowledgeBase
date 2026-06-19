using KnowledgeBase.Domain.Documents;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Pgvector;

namespace KnowledgeBase.Infrastructure.Persistence.Configurations;

public sealed class DocumentChunkConfiguration : IEntityTypeConfiguration<DocumentChunk>
{
    private const int EmbeddingDimensions = 1536;

    public void Configure(EntityTypeBuilder<DocumentChunk> builder)
    {
        builder.ToTable("document_chunks");

        builder.HasKey(chunk => chunk.Id);

        builder.Property(chunk => chunk.DocumentId)
            .IsRequired();

        builder.Property(chunk => chunk.ChunkIndex)
            .IsRequired();

        builder.Property(chunk => chunk.Content)
            .IsRequired();

        builder.Property(chunk => chunk.Embedding)
            .HasColumnType($"vector({EmbeddingDimensions})")
            .HasConversion(
                value => new Vector(value!),
                value => value.ToArray());

        builder.HasIndex(chunk => chunk.DocumentId);
    }
}
