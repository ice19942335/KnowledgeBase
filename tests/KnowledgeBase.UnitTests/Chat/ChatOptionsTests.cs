using KnowledgeBase.Chat.Application;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace KnowledgeBase.UnitTests.Chat;

public sealed class ChatOptionsTests
{
    [Fact]
    public void Bind_UsesExplicitSearchServiceBaseUrl_WhenConfigured()
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Chat:SearchServiceBaseUrl"] = "http://search-api:8080"
            })
            .Build();

        var services = new ServiceCollection();
        services.AddChatApplication(configuration);
        using var provider = services.BuildServiceProvider();

        var options = provider.GetRequiredService<IOptions<ChatOptions>>().Value;

        Assert.Equal("http://search-api:8080", options.SearchServiceBaseUrl);
    }

    [Fact]
    public void Bind_DefaultsToAspireServiceDiscoveryUri_WhenNotConfigured()
    {
        var configuration = new ConfigurationBuilder().Build();

        var services = new ServiceCollection();
        services.AddChatApplication(configuration);
        using var provider = services.BuildServiceProvider();

        var options = provider.GetRequiredService<IOptions<ChatOptions>>().Value;

        Assert.Equal("https+http://search-api", options.SearchServiceBaseUrl);
    }
}
