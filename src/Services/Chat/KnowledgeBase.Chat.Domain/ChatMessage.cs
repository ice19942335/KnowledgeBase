namespace KnowledgeBase.Chat.Domain;

public sealed class ChatMessage
{
    private ChatMessage()
    {
    }

    public ChatMessage(Guid conversationId, MessageRole role, string content)
    {
        Id = Guid.NewGuid();
        ConversationId = conversationId;
        Role = role;
        Content = content;
        CreatedAt = DateTime.UtcNow;
    }

    public Guid Id { get; private set; }

    public Guid ConversationId { get; private set; }

    public MessageRole Role { get; private set; }

    public string Content { get; private set; } = string.Empty;

    /// <summary>
    /// Serialized source references (document names, chunk indices) when <see cref="Role"/> is Assistant.
    /// </summary>
    public string? SourceReferences { get; set; }

    public DateTime CreatedAt { get; private set; }
}
