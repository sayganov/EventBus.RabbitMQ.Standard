using EventBus.RabbitMQ.Standard.Options;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;

namespace EventBus.RabbitMQ.Standard.Configuration;

public static class Connection
{
    public static IServiceCollection AddRabbitMqConnection(this IServiceCollection services,
        RabbitMqOptions options)
    {
        services.AddSingleton<IRabbitMqPersistentConnection>(sp =>
        {
            var logger = sp.GetRequiredService<ILogger<DefaultRabbitMqPersistentConnection>>();
            var retryCount = 5;

            var factory = new ConnectionFactory
            {
                HostName = options.Host,
                DispatchConsumersAsync = options.DispatchConsumersAsync,
                Port = AmqpTcpEndpoint.DefaultAmqpSslPort
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
            
            if (options.Port != default)
            {
                factory.Port = options.Port;
            }

            return new DefaultRabbitMqPersistentConnection(factory, logger, retryCount);
        });

        return services;
    }
}