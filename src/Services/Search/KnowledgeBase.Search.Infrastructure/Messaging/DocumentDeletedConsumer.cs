using KnowledgeBase.Contracts;
using KnowledgeBase.Search.Application;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace KnowledgeBase.Search.Infrastructure.Messaging;

public sealed class DocumentDeletedConsumer : IConsumer<DocumentDeleted>
{
    private readonly ChunkIndexingService indexingService;
    private readonly ILogger<DocumentDeletedConsumer> logger;

    public DocumentDeletedConsumer(
        ChunkIndexingService indexingService,
        ILogger<DocumentDeletedConsumer> logger)
    {
        this.indexingService = indexingService;
        this.logger = logger;
    }

    public async Task Consume(ConsumeContext<DocumentDeleted> context)
    {
        var message = context.Message;

        await indexingService.RemoveAsync(message.TenantId, message.DocumentId, context.CancellationToken);

        logger.LogInformation(
            "Removed chunks for deleted document {DocumentId}.",
            message.DocumentId);
    }
}
