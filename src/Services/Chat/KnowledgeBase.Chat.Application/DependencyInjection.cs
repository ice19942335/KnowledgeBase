using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace KnowledgeBase.Chat.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddChatApplication(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddOptions<RagOptions>()
            .Bind(configuration.GetSection(RagOptions.SectionName));

        services.AddOptions<ChatOptions>()
            .Bind(configuration.GetSection(ChatOptions.SectionName))
            .Validate(
                options => !string.IsNullOrWhiteSpace(options.SearchServiceBaseUrl),
                "Chat:SearchServiceBaseUrl must be configured.")
            .ValidateOnStart();

        services.AddScoped<ChatAppService>();

        return services;
    }
}
