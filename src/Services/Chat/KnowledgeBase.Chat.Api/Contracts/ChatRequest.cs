namespace KnowledgeBase.Chat.Api.Contracts;

public sealed record ChatRequest(Guid? ConversationId, string Question);
