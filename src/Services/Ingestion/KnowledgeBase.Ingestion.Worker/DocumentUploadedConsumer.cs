using KnowledgeBase.Contracts;
using KnowledgeBase.Ingestion.Application;
using MassTransit;

namespace KnowledgeBase.Ingestion.Worker;

public sealed class DocumentUploadedConsumer : IConsumer<DocumentUploaded>
{
    private readonly IngestionService ingestionService;
    private readonly ILogger<DocumentUploadedConsumer> logger;

    public DocumentUploadedConsumer(
        IngestionService ingestionService,
        ILogger<DocumentUploadedConsumer> logger)
    {
        this.ingestionService = ingestionService;
        this.logger = logger;
    }

    public async Task Consume(ConsumeContext<DocumentUploaded> context)
    {
        var message = context.Message;

        try
        {
            var chunks = await ingestionService.IngestAsync(message, context.CancellationToken);

            await context.Publish(new ChunksGenerated(
                message.DocumentId,
                message.TenantId,
                message.DocumentName,
                message.FileName,
                chunks));

            await context.Publish(new DocumentProcessingCompleted(
                message.DocumentId,
                message.TenantId,
                chunks.Count));

            logger.LogInformation(
                "Ingested document {DocumentId} into {ChunkCount} chunks.",
                message.DocumentId,
                chunks.Count);
        }
        catch (Exception exception)
        {
            logger.LogError(
                exception,
                "Failed to ingest document {DocumentId}.",
                message.DocumentId);

            await context.Publish(new DocumentProcessingFailed(
                message.DocumentId,
                message.TenantId,
                exception.Message));
        }
    }
}
