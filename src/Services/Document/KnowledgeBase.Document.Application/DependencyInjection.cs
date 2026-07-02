using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace KnowledgeBase.Document.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddDocumentApplication(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.Configure<DocumentOptions>(configuration.GetSection(DocumentOptions.SectionName));
        services.AddScoped<DocumentAppService>();
        return services;
    }
}
