using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace KnowledgeBase.Search.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddSearchApplication(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddOptions<SearchOptions>()
            .Bind(configuration.GetSection(SearchOptions.SectionName));

        services.AddScoped<SemanticSearchService>();
        services.AddScoped<ChunkIndexingService>();
        services.AddScoped<SearchExplorerService>();

        return services;
    }
}
