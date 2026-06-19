namespace KnowledgeBase.Chat.Domain;

public sealed class Conversation
{
    private readonly List<ChatMessage> messages = [];

    private Conversation()
    {
    }

    public Conversation(Guid tenantId, string title)
    {
        Id = Guid.NewGuid();
        TenantId = tenantId;
        Title = title;
        CreatedAt = DateTime.UtcNow;
    }

    public Guid Id { get; private set; }

    public Guid TenantId { get; private set; }

    public string Title { get; private set; } = string.Empty;

    public DateTime CreatedAt { get; private set; }

    public IReadOnlyList<ChatMessage> Messages => messages.AsReadOnly();

    public ChatMessage AddMessage(MessageRole role, string content, string? sources = null)
    {
        var message = new ChatMessage(Id, role, content)
        {
            SourceReferences = sources
        };

        messages.Add(message);
        return message;
    }
}
