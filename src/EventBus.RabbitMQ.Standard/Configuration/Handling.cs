using Microsoft.Extensions.DependencyInjection;

namespace EventBus.RabbitMQ.Standard.Configuration;

public static class Handling
{
    public static IServiceCollection AddEventBusHandling<T>(this IServiceCollection services, IEnumerable<T> handlers)
    {
        foreach (var handler in handlers)
        {
            services.AddTransient(handler.GetType());
        }

        return services;
    }
}