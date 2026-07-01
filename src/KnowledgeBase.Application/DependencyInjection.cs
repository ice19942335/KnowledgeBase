using KnowledgeBase.Application.Abstractions;
using KnowledgeBase.Application.Chat;
using KnowledgeBase.Application.Documents;
using KnowledgeBase.Application.Search;
using Microsoft.Extensions.DependencyInjection;

namespace KnowledgeBase.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<ITextChunker, TextChunker>();
        services.AddScoped<ITextExtractionService, TextExtractionService>();
        services.AddScoped<IDocumentService, DocumentService>();
        services.AddScoped<ChunkRetrievalPipeline>();
        services.AddScoped<ISemanticSearchService, SemanticSearchService>();
        services.AddScoped<IRagChatService, RagChatService>();

        return services;
    }
}
