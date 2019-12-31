using System;
using RabbitMQ.Client;

namespace EventBus.RabbitMQ.Standard.RabbitMq
{
    public interface IRabbitMqPersistentConnection : IDisposable
    {
        bool IsConnected { get; }
        bool TryConnect();

        IModel CreateModel();
    }
}