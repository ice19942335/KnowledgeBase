using KnowledgeBase.Chat.Domain;

namespace KnowledgeBase.Chat.Application;

public interface IConversationRepository
{
    Task AddAsync(Conversation conversation, CancellationToken cancellationToken);

    Task<Conversation?> GetAsync(Guid tenantId, Guid conversationId, CancellationToken cancellationToken);

    Task<IReadOnlyList<Conversation>> ListAsync(Guid tenantId, CancellationToken cancellationToken);

    Task RemoveAsync(Conversation conversation, CancellationToken cancellationToken);

    Task SaveChangesAsync(CancellationToken cancellationToken);
}
