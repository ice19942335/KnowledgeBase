using KnowledgeBase.Domain.Documents;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace KnowledgeBase.Infrastructure.Persistence.Configurations;

public sealed class DocumentConfiguration : IEntityTypeConfiguration<Document>
{
    public void Configure(EntityTypeBuilder<Document> builder)
    {
        builder.ToTable("documents");

        builder.HasKey(document => document.Id);

        builder.Property(document => document.Name)
            .IsRequired()
            .HasMaxLength(512);

        builder.Property(document => document.FileName)
            .IsRequired()
            .HasMaxLength(512);

        builder.Property(document => document.ContentType)
            .IsRequired()
            .HasMaxLength(256);

        builder.Property(document => document.UploadedAtUtc)
            .IsRequired();

        builder.Property(document => document.Status)
            .HasConversion<int>()
            .IsRequired();

        builder.Property(document => document.ExtractedText);

        builder.Property(document => document.FailureReason)
            .HasMaxLength(2048);

        builder.HasMany(document => document.Chunks)
            .WithOne()
            .HasForeignKey(chunk => chunk.DocumentId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Metadata
            .FindNavigation(nameof(Document.Chunks))!
            .SetPropertyAccessMode(PropertyAccessMode.Field);
    }
}
