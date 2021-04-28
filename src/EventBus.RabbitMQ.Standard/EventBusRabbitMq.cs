using System;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using EventBus.Base.Standard;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Polly;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RabbitMQ.Client.Exceptions;

namespace EventBus.RabbitMQ.Standard
{
    public class EventBusRabbitMq : IEventBus, IDisposable
    {
        private readonly IRabbitMqPersistentConnection _persistentConnection;
        private readonly IEventBusSubscriptionManager _subsManager;
        private readonly IServiceScopeFactory _serviceScopeFactory;
        private readonly string _brokerName;
        private readonly int _retryCount;
        private readonly bool _durableExchange;
        private readonly bool _durableQueue;

        private IModel _consumerChannel;
        private string _queueName;

        public EventBusRabbitMq(IRabbitMqPersistentConnection persistentConnection,
            IServiceScopeFactory serviceScopeFactory,
            IEventBusSubscriptionManager subsManager,
            string brokerName,
            string queueName = null,
            int retryCount = 5,
            bool durableExchange = false,
            bool durableQueue = true)
        {
            _persistentConnection = persistentConnection ?? throw new ArgumentNullException(nameof(persistentConnection));
            _subsManager = subsManager ?? new InMemoryEventBusSubscriptionManager();
            _serviceScopeFactory = serviceScopeFactory ?? throw new ArgumentNullException(nameof(serviceScopeFactory));

            _brokerName = brokerName;
            _queueName = queueName;
            _retryCount = retryCount;
            _durableExchange = durableExchange;
            _durableQueue = durableQueue;
            _consumerChannel = CreateConsumerChannel();
            _subsManager.OnEventRemoved += SubsManager_OnEventRemoved;
        }

        private void SubsManager_OnEventRemoved(object sender, string eventName)
        {
            if (!_persistentConnection.IsConnected)
            {
                _persistentConnection.TryConnect();
            }

            using (var channel = _persistentConnection.CreateModel())
            {
                channel.QueueUnbind(_queueName, _brokerName, eventName);

                if (!_subsManager.IsEmpty)
                {
                    return;
                }

                _queueName = string.Empty;
                _consumerChannel.Close();
            }
        }

        public void Publish(IntegrationEvent @event)
        {
            if (!_persistentConnection.IsConnected)
            {
                _persistentConnection.TryConnect();
            }

            var policy = Policy.Handle<BrokerUnreachableException>()
                .Or<SocketException>()
                .WaitAndRetry(_retryCount, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
                    (ex, time) => { });

            var eventName = @event.GetType().Name;

            using (var channel = _persistentConnection.CreateModel())
            {
                channel.ExchangeDeclare(_brokerName, "direct", _durableExchange);

                var message = JsonConvert.SerializeObject(@event);
                var body = Encoding.UTF8.GetBytes(message);

                policy.Execute(() =>
                {
                    var properties = channel.CreateBasicProperties();
                    properties.DeliveryMode = 2;

                    channel.BasicPublish(_brokerName, eventName, true, properties, body);
                });
            }
        }

        public void SubscribeDynamic<TH>(string eventName) where TH : IDynamicIntegrationEventHandler
        {
            DoInternalSubscription(eventName);

            _subsManager.AddDynamicSubscription<TH>(eventName);

            StartBasicConsume();
        }

        public void Subscribe<T, TH>() where T : IntegrationEvent where TH : IIntegrationEventHandler<T>
        {
            var eventName = _subsManager.GetEventKey<T>();

            DoInternalSubscription(eventName);

            _subsManager.AddSubscription<T, TH>();

            StartBasicConsume();
        }

        private void DoInternalSubscription(string eventName)
        {
            var containsKey = _subsManager.HasSubscriptionsForEvent(eventName);

            if (containsKey)
            {
                return;
            }

            if (!_persistentConnection.IsConnected)
            {
                _persistentConnection.TryConnect();
            }

            using (var channel = _persistentConnection.CreateModel())
            {
                channel.QueueBind(_queueName, _brokerName, eventName);
            }
        }

        public void Unsubscribe<T, TH>() where T : IntegrationEvent where TH : IIntegrationEventHandler<T>
        {
            _subsManager.RemoveSubscription<T, TH>();
        }

        public void UnsubscribeDynamic<TH>(string eventName) where TH : IDynamicIntegrationEventHandler
        {
            _subsManager.RemoveDynamicSubscription<TH>(eventName);
        }

        public void Dispose()
        {
            _consumerChannel?.Dispose();

            _subsManager.Clear();
        }

        private void StartBasicConsume()
        {
            if (_consumerChannel == null)
            {
                return;
            }

            var consumer = new AsyncEventingBasicConsumer(_consumerChannel);

            consumer.Received += Consumer_Received;

            _consumerChannel.BasicConsume(_queueName, false, consumer);
        }

        private async Task Consumer_Received(object sender, BasicDeliverEventArgs eventArgs)
        {
            var eventName = eventArgs.RoutingKey;
            var message = Encoding.UTF8.GetString(eventArgs.Body.Span.ToArray());

            try
            {
                if (message.ToLowerInvariant().Contains("throw-fake-exception"))
                {
                    throw new InvalidOperationException($"Fake exception requested: \"{message}\"");
                }

                await ProcessEvent(eventName, message);
            }
            catch (Exception)
            {
                throw new Exception($"Error processing message \"{message}\"");
            }

            _consumerChannel.BasicAck(eventArgs.DeliveryTag, false);
        }

        private IModel CreateConsumerChannel()
        {
            if (!_persistentConnection.IsConnected)
            {
                _persistentConnection.TryConnect();
            }

            var channel = _persistentConnection.CreateModel();

            channel.ExchangeDeclare(_brokerName, "direct", _durableExchange);
            channel.QueueDeclare(_queueName, _durableQueue, false, false, null);

            channel.CallbackException += (sender, ea) =>
            {
                _consumerChannel.Dispose();
                _consumerChannel = CreateConsumerChannel();

                StartBasicConsume();
            };

            return channel;
        }

        private async Task ProcessEvent(string eventName, string message)
        {
            if (_subsManager.HasSubscriptionsForEvent(eventName))
            {
                using (var scope = _serviceScopeFactory.CreateScope())
                {
                    var subscriptions = _subsManager.GetHandlersForEvent(eventName);
                    foreach (var subscription in subscriptions)
                        if (subscription.IsDynamic)
                        {
                            if (!(scope.ServiceProvider.GetService(subscription.HandlerType) is
                                IDynamicIntegrationEventHandler handler))
                            {
                                continue;
                            }

                            dynamic eventData = JObject.Parse(message);

                            await Task.Yield();
                            await handler.Handle(eventData);
                        }
                        else
                        {
                            var handler = scope.ServiceProvider.GetService(subscription.HandlerType);
                            if (handler == null)
                            {
                                continue;
                            }

                            var eventType = _subsManager.GetEventTypeByName(eventName);
                            var integrationEvent = JsonConvert.DeserializeObject(message, eventType);
                            var concreteType = typeof(IIntegrationEventHandler<>).MakeGenericType(eventType);

                            await Task.Yield();
                            await (Task) concreteType.GetMethod("Handle").Invoke(handler, new[] {integrationEvent});
                        }
                }
            }
        }
    }
}