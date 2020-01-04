using System.Threading.Tasks;
using EventBus.RabbitMQ.Standard.Base;
using Subscriber.IntegrationEvents.Events;

namespace Subscriber.IntegrationEvents.Handlers
{
    public class ItemCreatedIntegrationEventHandler : IIntegrationEventHandler<ItemCreatedIntegrationEvent>
    {
        public ItemCreatedIntegrationEventHandler()
        {
        }

        public async Task Handle(ItemCreatedIntegrationEvent @event)
        {
            var itemTitle = @event.Title;
            var itemDescription = @event.Description;
        }
    }
}
