using KnowledgeBase.Ai;
using KnowledgeBase.Messaging;
using KnowledgeBase.Search.Application;
using KnowledgeBase.Search.Infrastructure.Messaging;
using KnowledgeBase.Search.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace KnowledgeBase.Search.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddSearchInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("searchdb")
            ?? throw new InvalidOperationException("Connection string 'searchdb' is not configured.");

        services.AddDbContext<SearchDbContext>(options =>
            options.UseNpgsql(connectionString, npgsql => npgsql.UseVector()));

        services.AddScoped<IChunkRepository, ChunkRepository>();

        services.AddKnowledgeBaseEmbeddings(configuration);

        services.AddKnowledgeBaseMessaging(configuration, bus =>
        {
            bus.AddConsumer<ChunksGeneratedConsumer>();
            bus.AddConsumer<DocumentDeletedConsumer>();
        });

        return services;
    }
}
