using System;
using Microsoft.Extensions.DependencyInjection;
using RabbitMQ.Client;

namespace EventBus.RabbitMQ.Configuration
{
    public static class IntegrationService
    {
        public static IServiceCollection AddIntegrationService(this IServiceCollection services, Action<IntegrationServiceOption> options)
        {
            var configureOptions = new IntegrationServiceOption();
            options(configureOptions);

            services.AddSingleton<IRabbitMqPersistentConnection>(sp =>
            {
                var retryCount = 5;

                var factory = new ConnectionFactory
                {
                    HostName = configureOptions.Connection,
                    DispatchConsumersAsync = configureOptions.DispatchConsumersAsync
                };

                if (!string.IsNullOrEmpty(configureOptions.VirtualHost))
                {
                    factory.VirtualHost = configureOptions.VirtualHost;
                }

                if (!string.IsNullOrEmpty(configureOptions.Username))
                {
                    factory.UserName = configureOptions.Username;
                }

                if (!string.IsNullOrEmpty(configureOptions.Password))
                {
                    factory.Password = configureOptions.Password;
                }

                if (!string.IsNullOrEmpty(configureOptions.RetryCount))
                {
                    retryCount = int.Parse(configureOptions.RetryCount);
                }

                return new DefaultRabbitMqPersistentConnection(factory, retryCount);
            });

            return services;
        }
    }
}
