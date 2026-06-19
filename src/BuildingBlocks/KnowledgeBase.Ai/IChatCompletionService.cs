namespace KnowledgeBase.Ai;

/// <summary>
/// Produces a chat completion from a system prompt and a user prompt.
/// </summary>
public interface IChatCompletionService
{
    Task<string> CompleteAsync(string systemPrompt, string userPrompt, CancellationToken cancellationToken);
}
