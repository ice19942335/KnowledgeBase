using KnowledgeBase.Chat.Application;
using KnowledgeBase.Chat.Domain;
using Microsoft.EntityFrameworkCore;

namespace KnowledgeBase.Chat.Infrastructure.Persistence;

public sealed class ConversationRepository : IConversationRepository
{
    private readonly ChatDbContext dbContext;

    public ConversationRepository(ChatDbContext dbContext)
    {
        this.dbContext = dbContext;
    }

    public async Task AddAsync(Conversation conversation, CancellationToken cancellationToken)
    {
        await dbContext.Conversations.AddAsync(conversation, cancellationToken);
    }

    public Task<Conversation?> GetAsync(Guid tenantId, Guid conversationId, CancellationToken cancellationToken)
    {
        return dbContext.Conversations
            .Include(c => c.Messages.OrderBy(m => m.CreatedAt))
            .FirstOrDefaultAsync(c => c.TenantId == tenantId && c.Id == conversationId, cancellationToken);
    }

    public async Task<IReadOnlyList<Conversation>> ListAsync(Guid tenantId, CancellationToken cancellationToken)
    {
        return await dbContext.Conversations
            .Where(c => c.TenantId == tenantId)
            .OrderByDescending(c => c.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public Task RemoveAsync(Conversation conversation, CancellationToken cancellationToken)
    {
        dbContext.Conversations.Remove(conversation);
        return Task.CompletedTask;
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken)
    {
        return dbContext.SaveChangesAsync(cancellationToken);
    }
}
