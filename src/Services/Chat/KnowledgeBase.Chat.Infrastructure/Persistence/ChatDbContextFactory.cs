using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace KnowledgeBase.Chat.Infrastructure.Persistence;

public sealed class ChatDbContextFactory : IDesignTimeDbContextFactory<ChatDbContext>
{
    public ChatDbContext CreateDbContext(string[] args)
    {
        var options = new DbContextOptionsBuilder<ChatDbContext>()
            .UseNpgsql("Host=localhost;Database=chatdb;Username=postgres;Password=postgres")
            .Options;

        return new ChatDbContext(options);
    }
}
