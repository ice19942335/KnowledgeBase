using KnowledgeBase.Contracts;
using KnowledgeBase.Search.Application;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace KnowledgeBase.Search.Infrastructure.Messaging;

public sealed class ChunksGeneratedConsumer : IConsumer<ChunksGenerated>
{
    private readonly ChunkIndexingService indexingService;
    private readonly ILogger<ChunksGeneratedConsumer> logger;

    public ChunksGeneratedConsumer(
        ChunkIndexingService indexingService,
        ILogger<ChunksGeneratedConsumer> logger)
    {
        this.indexingService = indexingService;
        this.logger = logger;
    }

    public async Task Consume(ConsumeContext<ChunksGenerated> context)
    {
        var message = context.Message;

        await indexingService.IndexAsync(message, context.CancellationToken);

        logger.LogInformation(
            "Indexed {ChunkCount} chunks for document {DocumentId}.",
            message.Chunks.Count,
            message.DocumentId);
    }
}
