using Microsoft.Extensions.Configuration;

namespace KnowledgeBase.Ai;

public static class AiConfiguration
{
    public static int GetEmbeddingDimensions(IConfiguration configuration)
    {
        return configuration.GetSection(GeminiOptions.SectionName).Get<GeminiOptions>()?.EmbeddingDimensions
            ?? 1536;
    }
}
