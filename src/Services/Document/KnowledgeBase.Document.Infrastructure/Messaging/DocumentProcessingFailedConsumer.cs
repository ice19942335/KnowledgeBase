using KnowledgeBase.Contracts;
using KnowledgeBase.Document.Application;
using MassTransit;

namespace KnowledgeBase.Document.Infrastructure.Messaging;

public sealed class DocumentProcessingFailedConsumer : IConsumer<DocumentProcessingFailed>
{
    private readonly DocumentAppService documentService;

    public DocumentProcessingFailedConsumer(DocumentAppService documentService)
    {
        this.documentService = documentService;
    }

    public Task Consume(ConsumeContext<DocumentProcessingFailed> context)
    {
        var message = context.Message;
        return documentService.ApplyProcessingFailureAsync(
            message.TenantId,
            message.DocumentId,
            message.Reason,
            context.CancellationToken);
    }
}
