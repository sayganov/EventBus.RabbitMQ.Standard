using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;

namespace EventBus.RabbitMQ.Configuration
{
    public static class EventBusHandler
    {
        public static IServiceCollection AddEventBusHandler<T>(this IServiceCollection services, IEnumerable<T> handlers)
        {
            foreach (var handler in handlers)
            {
                services.AddTransient(handler.GetType());
            }

            return services;
        }
    }
}
