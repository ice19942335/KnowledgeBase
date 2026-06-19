using KnowledgeBase.Ingestion.Application.Abstractions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace KnowledgeBase.Ingestion.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddIngestionApplication(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddOptions<ChunkingOptions>()
            .Bind(configuration.GetSection(ChunkingOptions.SectionName));

        services.AddSingleton<ITextChunker, TextChunker>();
        services.AddSingleton<ITextExtractionService, TextExtractionService>();
        services.AddScoped<IngestionService>();

        return services;
    }
}
