using System.Threading.Tasks;

namespace EventBus.RabbitMQ.Standard.Base
{
    public interface IDynamicIntegrationEventHandler
    {
        Task Handle(dynamic eventData);
    }
}
