using System.Collections.Generic;
using EventBus.Base.Standard;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Subscriber.IntegrationEvents.Events;
using Subscriber.IntegrationEvents.Handlers;

namespace Subscriber.Extensions
{
public static class EventBusExtension
{
    public static IEnumerable<IIntegrationEventHandler> GetHandlers()
    {
        return new List<IIntegrationEventHandler>
        {
            new ItemCreatedIntegrationEventHandler()
        };
    }

    public static IApplicationBuilder SubscribeToEvents(this IApplicationBuilder app)
    {
        var eventBus = app.ApplicationServices.GetRequiredService<IEventBus>();

        eventBus.Subscribe<ItemCreatedIntegrationEvent, ItemCreatedIntegrationEventHandler>();

        return app;
    }
}
}
