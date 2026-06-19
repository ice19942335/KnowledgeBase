using KnowledgeBase.Contracts;
using KnowledgeBase.Document.Application;
using MassTransit;

namespace KnowledgeBase.Document.Infrastructure.Messaging;

public sealed class DocumentProcessingCompletedConsumer : IConsumer<DocumentProcessingCompleted>
{
    private readonly DocumentAppService documentService;

    public DocumentProcessingCompletedConsumer(DocumentAppService documentService)
    {
        this.documentService = documentService;
    }

    public Task Consume(ConsumeContext<DocumentProcessingCompleted> context)
    {
        var message = context.Message;
        return documentService.ApplyProcessingResultAsync(
            message.TenantId,
            message.DocumentId,
            message.ChunkCount,
            context.CancellationToken);
    }
}
