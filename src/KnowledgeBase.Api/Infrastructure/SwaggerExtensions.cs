using Microsoft.OpenApi;

namespace KnowledgeBase.Api.Infrastructure;

public static class SwaggerExtensions
{
    public static IServiceCollection AddKnowledgeBaseSwagger(this IServiceCollection services)
    {
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "AI Knowledge Base API",
                Version = "v1",
                Description = "MVP API: document ingestion, semantic search, and RAG chat."
            });
        });

        return services;
    }
}
