namespace KnowledgeBase.Ai;

public sealed class AiNotConfiguredException : InvalidOperationException
{
    public AiNotConfiguredException()
        : base(AiAvailabilityState.MissingApiKeyMessage)
    {
    }

    public AiNotConfiguredException(string message)
        : base(message)
    {
    }
}
