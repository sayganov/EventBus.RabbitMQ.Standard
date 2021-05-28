﻿namespace EventBus.RabbitMQ.Standard.Options
{
    public class RabbitMqOptions
    {
        public string BrokerName { get; set; }
        public string Host { get; set; }
        public string Password { get; set; }
        public string QueueName { get; set; }
        public string RetryCount { get; set; }
        public string Username { get; set; }
        public string VirtualHost { get; set; }
        public bool DurableExchange { get; set; }
        public bool DurableQueue { get; set; } = true;
        public bool DispatchConsumersAsync { get; set; }
    }
}
