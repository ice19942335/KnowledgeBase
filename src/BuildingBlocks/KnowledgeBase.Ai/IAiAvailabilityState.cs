namespace KnowledgeBase.Ai;

/// <summary>
/// Cached Gemini API key availability evaluated once at application startup.
/// </summary>
public interface IAiAvailabilityState
{
    bool IsConfigured { get; }

    string StatusMessage { get; }
}
