using EventBus.RabbitMQ.Standard.Interfaces;
using EventBus.RabbitMQ.Standard.Options;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace EventBus.RabbitMQ.Standard.Configuration;

public static class Registration
{
    public static IServiceCollection AddRabbitMqRegistration(this IServiceCollection services, RabbitMqOptions options)
    {
        services.AddSingleton<IEventBus, EventBusRabbitMq>(sp =>
        {
            var rabbitMqPersistentConnection = sp.GetRequiredService<IRabbitMqPersistentConnection>();
            var lifetimeScope = sp.GetRequiredService<IServiceScopeFactory>();
            var eventBusSubscriptionsManager = sp.GetRequiredService<IEventBusSubscriptionManager>();
            var logger = sp.GetRequiredService<ILogger<EventBusRabbitMq>>();

            var brokerName = options.BrokerName;
            var queueName = options.QueueName;
            var durableExchange = options.DurableExchange;
            var durableQueue = options.DurableQueue;
            var retryCount = 5;

            if (!string.IsNullOrEmpty(options.RetryCount))
            {
                retryCount = int.Parse(options.RetryCount);
            }

            return new EventBusRabbitMq(rabbitMqPersistentConnection,
                lifetimeScope,
                eventBusSubscriptionsManager,
                logger,
                brokerName,
                queueName,
                durableExchange,
                durableQueue,
                retryCount);
        });

        services.AddSingleton<IEventBusSubscriptionManager, InMemoryEventBusSubscriptionManager>();

        return services;
    }
}