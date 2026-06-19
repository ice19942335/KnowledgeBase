using KnowledgeBase.Document.Application;
using KnowledgeBase.Document.Infrastructure.Messaging;
using KnowledgeBase.Document.Infrastructure.Persistence;
using KnowledgeBase.Messaging;
using KnowledgeBase.SharedKernel.Messaging;
using KnowledgeBase.SharedKernel.Storage;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace KnowledgeBase.Document.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddDocumentInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("documentdb")
            ?? throw new InvalidOperationException("Connection string 'documentdb' is not configured.");

        services.AddDbContext<DocumentDbContext>(options =>
            options.UseNpgsql(connectionString));

        services.AddScoped<IDocumentRepository, DocumentRepository>();
        services.AddScoped<IEventPublisher, MassTransitEventPublisher>();

        services.AddLocalFileStorage(configuration);

        services.AddKnowledgeBaseMessaging(configuration, bus =>
        {
            bus.AddConsumer<DocumentProcessingCompletedConsumer>();
            bus.AddConsumer<DocumentProcessingFailedConsumer>();
        });

        return services;
    }
}
