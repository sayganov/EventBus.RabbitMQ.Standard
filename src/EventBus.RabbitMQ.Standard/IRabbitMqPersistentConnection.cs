using System;
using RabbitMQ.Client;

namespace EventBus.RabbitMQ.Standard
{
    public interface IRabbitMqPersistentConnection : IDisposable
    {
        bool IsConnected { get; }
        bool TryConnect();

        IModel CreateModel();
    }
}