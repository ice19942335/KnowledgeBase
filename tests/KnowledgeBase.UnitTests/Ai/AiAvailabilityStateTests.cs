using KnowledgeBase.Ai;
using Microsoft.Extensions.Options;

namespace KnowledgeBase.UnitTests.Ai;

public sealed class AiAvailabilityStateTests
{
    [Fact]
    public void IsConfigured_ReturnsFalseWhenApiKeyMissing()
    {
        var state = CreateState(apiKey: string.Empty);

        Assert.False(state.IsConfigured);
        Assert.Equal(AiAvailabilityState.MissingApiKeyMessage, state.StatusMessage);
    }

    [Fact]
    public void IsConfigured_ReturnsTrueWhenApiKeyPresent()
    {
        var state = CreateState(apiKey: "test-key");

        Assert.True(state.IsConfigured);
        Assert.Contains("configured", state.StatusMessage, StringComparison.OrdinalIgnoreCase);
    }

    private static AiAvailabilityState CreateState(string apiKey)
    {
        var options = Options.Create(new GeminiOptions { ApiKey = apiKey });
        return new AiAvailabilityState(options);
    }
}
