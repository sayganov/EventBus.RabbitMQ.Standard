namespace EventBus.RabbitMQ.Configuration
{
    public class IntegrationServiceOption
    {
        public string VirtualHost { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string Connection { get; set; }
        public string RetryCount { get; set; }

        public bool DispatchConsumersAsync { get; set; }
    }
}