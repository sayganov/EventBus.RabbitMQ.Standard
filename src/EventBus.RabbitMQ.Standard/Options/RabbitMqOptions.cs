using RabbitMQ.Client;

namespace EventBus.RabbitMQ.Standard.Options
{
    public class RabbitMqOptions
    {
        public string BrokerName { get; set; }
        public string Host { get; set; }
        public int Port { get; set; }
        public string Password { get; set; }
        public string QueueName { get; set; }
        public string RetryCount { get; set; }
        public string Username { get; set; }
        public string VirtualHost { get; set; }

        public bool DispatchConsumersAsync { get; set; }
    }
}
