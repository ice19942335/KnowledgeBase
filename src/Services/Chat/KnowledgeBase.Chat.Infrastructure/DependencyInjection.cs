using KnowledgeBase.Ai;
using KnowledgeBase.Chat.Application;
using KnowledgeBase.Chat.Infrastructure.Clients;
using KnowledgeBase.Chat.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace KnowledgeBase.Chat.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddChatInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("chatdb")
            ?? throw new InvalidOperationException("Connection string 'chatdb' is not configured.");

        services.AddDbContext<ChatDbContext>(options =>
            options.UseNpgsql(connectionString));

        services.AddScoped<IConversationRepository, ConversationRepository>();

        services.AddKnowledgeBaseAi(configuration);

        services.AddHttpClient<ISearchApiClient, HttpSearchApiClient>(client =>
        {
            client.BaseAddress = new Uri("https+http://search-api");
        });

        return services;
    }
}
