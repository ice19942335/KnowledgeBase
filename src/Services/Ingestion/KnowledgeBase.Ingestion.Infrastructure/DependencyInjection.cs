using KnowledgeBase.Ai;
using KnowledgeBase.Ingestion.Application.Abstractions;
using KnowledgeBase.Ingestion.Infrastructure.TextExtraction;
using KnowledgeBase.SharedKernel.Storage;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace KnowledgeBase.Ingestion.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddIngestionInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddSingleton<ITextExtractor, PdfTextExtractor>();
        services.AddSingleton<ITextExtractor, PlainTextExtractor>();

        services.AddKnowledgeBaseAi(configuration);
        services.AddLocalFileStorage(configuration);

        return services;
    }
}
