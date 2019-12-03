using System;
using System.Collections.Generic;
using Autofac;
using EventBus.Base;
using Microsoft.Extensions.DependencyInjection;

namespace EventBus.RabbitMQ.Configuration
{
    public static class EventBus
    {
        public static IServiceCollection AddEventBus<T>(this IServiceCollection services, Action<EventBusOption> options, List<T> handlers)
        {
            var configureOptions = new EventBusOption();
            options(configureOptions);

            services.AddSingleton<IEventBus, EventBusRabbitMq>(sp =>
            {
                var rabbitMqPersistentConnection = sp.GetRequiredService<IRabbitMqPersistentConnection>();
                var lifetimeScope = sp.GetRequiredService<ILifetimeScope>();
                var eventBusSubscriptionsManager = sp.GetRequiredService<IEventBusSubscriptionManager>();

                var brokerName = configureOptions.BrokerName;
                var autofacScopeName = configureOptions.AutofacScopeName;
                var queueName = configureOptions.QueueName;
                var retryCount = 5;

                if (!string.IsNullOrEmpty(configureOptions.RetryCount))
                {
                    retryCount = int.Parse(configureOptions.RetryCount);
                }

                return new EventBusRabbitMq(rabbitMqPersistentConnection,
                    lifetimeScope,
                    eventBusSubscriptionsManager,
                    brokerName,
                    autofacScopeName,
                    queueName,
                    retryCount);
            });

            services.AddSingleton<IEventBusSubscriptionManager, InMemoryEventBusSubscriptionManager>();

            //Event handlers
            foreach (var handler in handlers) services.AddTransient(handler.GetType());

            return services;
        }
    }
}