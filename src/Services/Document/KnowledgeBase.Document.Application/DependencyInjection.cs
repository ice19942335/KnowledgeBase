using Microsoft.Extensions.DependencyInjection;

namespace KnowledgeBase.Document.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddDocumentApplication(this IServiceCollection services)
    {
        services.AddScoped<DocumentAppService>();
        return services;
    }
}
