using KnowledgeBase.Chat.Domain;
using Microsoft.EntityFrameworkCore;

namespace KnowledgeBase.Chat.Infrastructure.Persistence;

public sealed class ChatDbContext : DbContext
{
    public ChatDbContext(DbContextOptions<ChatDbContext> options)
        : base(options)
    {
    }

    public DbSet<Conversation> Conversations => Set<Conversation>();
    public DbSet<ChatMessage> Messages => Set<ChatMessage>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Conversation>(entity =>
        {
            entity.ToTable("conversations");
            entity.HasKey(c => c.Id);
            entity.Property(c => c.Title).HasMaxLength(256);
            entity.HasIndex(c => new { c.TenantId, c.CreatedAt });

            entity.HasMany(c => c.Messages)
                .WithOne()
                .HasForeignKey(m => m.ConversationId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<ChatMessage>(entity =>
        {
            entity.ToTable("chat_messages");
            entity.HasKey(m => m.Id);
            entity.Property(m => m.Role).HasConversion<int>();
            entity.Property(m => m.Content).IsRequired();
            entity.Property(m => m.SourceReferences).HasMaxLength(8000);
        });
    }
}
