namespace KnowledgeBase.SharedKernel.Messaging;

/// <summary>
/// Publishes integration events to the message broker. Keeps the application
/// layer free of any MassTransit dependency.
/// </summary>
public interface IEventPublisher
{
    Task PublishAsync<TEvent>(TEvent integrationEvent, CancellationToken cancellationToken)
        where TEvent : class;
}
