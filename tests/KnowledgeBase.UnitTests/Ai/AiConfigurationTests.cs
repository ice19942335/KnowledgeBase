using KnowledgeBase.Ai;
using Microsoft.Extensions.Configuration;

namespace KnowledgeBase.UnitTests.Ai;

public sealed class AiConfigurationTests
{
    [Fact]
    public void GetEmbeddingDimensions_UsesGeminiSection()
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                [$"{GeminiOptions.SectionName}:EmbeddingDimensions"] = "768"
            })
            .Build();

        var dimensions = AiConfiguration.GetEmbeddingDimensions(configuration);

        Assert.Equal(768, dimensions);
    }

    [Fact]
    public void GetEmbeddingDimensions_DefaultsTo1536()
    {
        var configuration = new ConfigurationBuilder().Build();

        var dimensions = AiConfiguration.GetEmbeddingDimensions(configuration);

        Assert.Equal(1536, dimensions);
    }
}
