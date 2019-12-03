using System;
using System.Collections.Generic;

namespace EventBus.Base
{
    public interface IEventBusSubscriptionManager
    {
        bool IsEmpty { get; }
        event EventHandler<string> OnEventRemoved;

        void AddDynamicSubscription<TH>(string eventName)
            where TH : IDynamicIntegrationEventHandler;

        void AddSubscription<T, TH>()
            where T : IntegrationEvent
            where TH : IIntegrationEventHandler<T>;

        void RemoveSubscription<T, TH>()
            where TH : IIntegrationEventHandler<T>
            where T : IntegrationEvent;

        void RemoveDynamicSubscription<TH>(string eventName)
            where TH : IDynamicIntegrationEventHandler;

        void Clear();

        bool HasSubscriptionsForEvent<T>() where T : IntegrationEvent;
        bool HasSubscriptionsForEvent(string eventName);

        string GetEventKey<T>();

        Type GetEventTypeByName(string eventName);

        IEnumerable<InMemoryEventBusSubscriptionManager.SubscriptionInfo> GetHandlersForEvent<T>() where T : IntegrationEvent;
        IEnumerable<InMemoryEventBusSubscriptionManager.SubscriptionInfo> GetHandlersForEvent(string eventName);
    }
}