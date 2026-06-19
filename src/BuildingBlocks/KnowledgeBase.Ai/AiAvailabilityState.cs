using Microsoft.Extensions.Options;

namespace KnowledgeBase.Ai;

public sealed class AiAvailabilityState : IAiAvailabilityState
{
    public const string MissingApiKeyMessage =
        "Gemini API key is not configured. Set Gemini:ApiKey in AppHost user-secrets (see README).";

    public AiAvailabilityState(IOptions<GeminiOptions> options)
    {
        IsConfigured = !string.IsNullOrWhiteSpace(options.Value.ApiKey);
        StatusMessage = IsConfigured
            ? "Gemini API key is configured."
            : MissingApiKeyMessage;
    }

    public bool IsConfigured { get; }

    public string StatusMessage { get; }
}
