using EventBus.RabbitMQ.Standard.Options;
using EventBus.RabbitMQ.Standard.RabbitMq;
using Microsoft.Extensions.DependencyInjection;
using RabbitMQ.Client;

namespace EventBus.RabbitMQ.Standard
{
    public static class EventBusConnection
    {
        public static IServiceCollection AddEventBusConnection(this IServiceCollection services, EventBusOptions options)
        {
            services.AddSingleton<IRabbitMqPersistentConnection>(sp =>
            {
                var retryCount = 5;

                var factory = new ConnectionFactory
                {
                    HostName = options.Connection,
                    DispatchConsumersAsync = options.DispatchConsumersAsync
                };

                if (!string.IsNullOrEmpty(options.VirtualHost))
                {
                    factory.VirtualHost = options.VirtualHost;
                }

                if (!string.IsNullOrEmpty(options.Username))
                {
                    factory.UserName = options.Username;
                }

                if (!string.IsNullOrEmpty(options.Password))
                {
                    factory.Password = options.Password;
                }

                if (!string.IsNullOrEmpty(options.RetryCount))
                {
                    retryCount = int.Parse(options.RetryCount);
                }

                return new DefaultRabbitMqPersistentConnection(factory, retryCount);
            });

            return services;
        }
    }
}
