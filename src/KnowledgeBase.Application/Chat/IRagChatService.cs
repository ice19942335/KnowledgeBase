namespace KnowledgeBase.Application.Chat;

public interface IRagChatService
{
    Task<ChatAnswerDto> AskAsync(string question, CancellationToken cancellationToken);
}
