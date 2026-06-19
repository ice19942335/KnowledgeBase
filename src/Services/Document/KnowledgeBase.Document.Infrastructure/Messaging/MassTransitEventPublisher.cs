using KnowledgeBase.SharedKernel.Messaging;
using MassTransit;

namespace KnowledgeBase.Document.Infrastructure.Messaging;

public sealed class MassTransitEventPublisher : IEventPublisher
{
    private readonly IPublishEndpoint publishEndpoint;

    public MassTransitEventPublisher(IPublishEndpoint publishEndpoint)
    {
        this.publishEndpoint = publishEndpoint;
    }

    public Task PublishAsync<TEvent>(TEvent integrationEvent, CancellationToken cancellationToken)
        where TEvent : class
    {
        return publishEndpoint.Publish(integrationEvent, cancellationToken);
    }
}
