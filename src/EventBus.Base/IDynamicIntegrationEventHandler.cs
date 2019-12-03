using System.Threading.Tasks;

namespace EventBus.Base
{
    public interface IDynamicIntegrationEventHandler
    {
        Task Handle(dynamic eventData);
    }
}
