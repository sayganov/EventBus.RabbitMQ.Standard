using EventBus.Base.Standard;
using EventBus.RabbitMQ.Standard.Options;
using Microsoft.Extensions.DependencyInjection;

namespace EventBus.RabbitMQ.Standard.Configuration
{
    public static class Registration
    {
        public static IServiceCollection AddRabbitMqRegistration(this IServiceCollection services,
            RabbitMqOptions options)
        {
            services.AddSingleton<IEventBus, EventBusRabbitMq>(sp =>
            {
                var rabbitMqPersistentConnection = sp.GetRequiredService<IRabbitMqPersistentConnection>();
                var lifetimeScope = sp.GetRequiredService<IServiceScopeFactory>();
                var eventBusSubscriptionsManager = sp.GetRequiredService<IEventBusSubscriptionManager>();

                var brokerName = options.BrokerName;
                var queueName = options.QueueName;
                var retryCount = 5;

                if (!string.IsNullOrEmpty(options.RetryCount))
                {
                    retryCount = int.Parse(options.RetryCount);
                }

                return new EventBusRabbitMq(rabbitMqPersistentConnection,
                    lifetimeScope,
                    eventBusSubscriptionsManager,
                    brokerName,
                    queueName,
                    retryCount);
            });

            services.AddSingleton<IEventBusSubscriptionManager, InMemoryEventBusSubscriptionManager>();

            return services;
        }
    }
}