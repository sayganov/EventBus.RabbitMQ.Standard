namespace EventBus.RabbitMQ.Standard.Interfaces;

public interface IDynamicIntegrationEventHandler
{
    Task Handle(dynamic eventData);
}