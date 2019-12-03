namespace EventBus.RabbitMQ.Configuration
{
    public class EventBusOption
    {
        public string BrokerName { get; set; }
        public string AutofacScopeName { get; set; }
        public string QueueName { get; set; }
        public string RetryCount { get; set; }
    }
}