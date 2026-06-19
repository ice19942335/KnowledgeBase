using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace KnowledgeBase.Messaging;

public static class MessagingExtensions
{
    /// <summary>
    /// Registers MassTransit with the shared RabbitMQ broker. Each service passes
    /// its own consumer registrations via <paramref name="configure"/>.
    /// </summary>
    public static IServiceCollection AddKnowledgeBaseMessaging(
        this IServiceCollection services,
        IConfiguration configuration,
        Action<IBusRegistrationConfigurator>? configure = null)
    {
        var connectionString = configuration.GetConnectionString("rabbitmq")
            ?? configuration.GetConnectionString("messaging")
            ?? throw new InvalidOperationException(
                "RabbitMQ connection string ('rabbitmq') is not configured.");

        services.AddMassTransit(x =>
        {
            x.SetKebabCaseEndpointNameFormatter();

            configure?.Invoke(x);

            x.UsingRabbitMq((context, cfg) =>
            {
                cfg.Host(new Uri(connectionString));
                cfg.ConfigureEndpoints(context);
            });
        });

        return services;
    }
}
