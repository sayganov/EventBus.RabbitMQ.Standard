# EventBus.RabbitMQ.Standard  

![Actions Status](https://github.com/sayganov/EventBus.RabbitMQ.Standard/workflows/Build/badge.svg) [![NuGet Badge](https://buildstats.info/nuget/EventBus.RabbitMQ.Standard?includePreReleases=false)](https://www.nuget.org/packages/EventBus.RabbitMQ.Standard) [![CodeFactor](https://www.codefactor.io/repository/github/sayganov/eventbus.rabbitmq.standard/badge)](https://www.codefactor.io/repository/github/sayganov/eventbus.rabbitmq.standard)

A library for the event-based communication by using RabbitMQ.

## Samples

- [Publisher app](https://github.com/sayganov/EventBus.RabbitMQ.Standard/tree/master/samples/Publisher)
- [Subscriber app](https://github.com/sayganov/EventBus.RabbitMQ.Standard/tree/master/samples/Subscriber)

## How-To

Install a **`NuGet`** package.

```console
PM> Install-Package EventBus.RabbitMQ.Standard
```

Add configuration to **`appsettings.json`**.

```json
{
  "RabbitMq": {
    "BrokerName": "test_broker",
    "QueueName": "test_queue",
    "RetryCount": "5",
    "VirtualHost": "your_virtual_host",
    "Username": "your_username",
    "Password": "your_password",
    "Host": "your_host",
    "Port": 5671,
    "DispatchConsumersAsync": true
  }
}
```

**Note:** I find pretty easy to use [CloudAMQP](https://www.cloudamqp.com/). Alternatively, you can run a Docker container for RabbiMQ on a local machine.

In **`publisher`** and **`subscriber`** apps, create a new class called **`ItemCreatedIntegrationEvent`**.

```csharp
public class ItemCreatedIntegrationEvent : IntegrationEvent
{
    public string Title { get; set; }
    public string Description { get; set; }

    public ItemCreatedIntegrationEvent(string title, string description)
    {
        Title = title;
        Description = description;
    }
}
```

In the **`subscriber`** app, create a new class called **`ItemCreatedIntegrationEventHandler`**.

```csharp
public class ItemCreatedIntegrationEventHandler : IIntegrationEventHandler<ItemCreatedIntegrationEvent>
{
    public ItemCreatedIntegrationEventHandler()
    {
    }

    public async Task Handle(ItemCreatedIntegrationEvent @event)
    {
        //Handle the ItemCreatedIntegrationEvent event here.
    }
}
```

Modify **`Program.cs`** by adding one line of code to the class.

```csharp
public class Program
{
    public static void Main(string[] args)
    {
        CreateHostBuilder(args).Build().Run();
    }

    public static IHostBuilder CreateHostBuilder(string[] args) =>
        Host.CreateDefaultBuilder(args)
            .ConfigureWebHostDefaults(webBuilder =>
            {
                webBuilder.UseStartup<Startup>();
            });
}
```

In the **`publisher`** app, modify the method **`ConfigureServices`** in **`Startup.cs`**.

```csharp
public class Startup
{
    public void ConfigureServices(IServiceCollection services)
    {
        ...

        var rabbitMqOptions = Configuration.GetSection("RabbitMq").Get<RabbitMqOptions>();

        services.AddRabbitMqConnection(rabbitMqOptions);
        services.AddRabbitMqRegistration(rabbitMqOptions);

        ...
    }
}
```

In the **`subscriber`** app, create an extension called **`EventBusExtension`**.

```csharp
public static class EventBusExtension
{
    public static IEnumerable<IIntegrationEventHandler> GetHandlers()
    {
        return new List<IIntegrationEventHandler>
        {
            new ItemCreatedIntegrationEventHandler()
        };
    }

    public static IApplicationBuilder SubscribeToEvents(this IApplicationBuilder app)
    {
        var eventBus = app.ApplicationServices.GetRequiredService<IEventBus>();

        eventBus.Subscribe<ItemCreatedIntegrationEvent, ItemCreatedIntegrationEventHandler>();

        return app;
    }
}
```

In the **`subscriber`** app, modify **`ConfigureServices`** and **`Configure`** methods in **`Startup.cs`**.

```csharp
public class Startup
{
    public void ConfigureServices(IServiceCollection services)
    {
        ...

        var rabbitMqOptions = Configuration.GetSection("RabbitMq").Get<RabbitMqOptions>();

        services.AddRabbitMqConnection(rabbitMqOptions);
        services.AddRabbitMqRegistration(rabbitMqOptions);
        services.AddEventBusHandling(EventBusExtension.GetHandlers());

        ...
    }

    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
        ...

        app.SubscribeToEvents();

        ...
    }
}
```

Publish the ItemCreatedIntegrationEvent event in the **`publisher`** app by using the following code, for example in a controller.

```csharp
public class ItemController : ControllerBase
{
    private readonly IEventBus _eventBus;

    public ItemController(IEventBus eventBus)
    {
        _eventBus = eventBus;
    }

    [HttpPost]
    public IActionResult Publish()
    {
        var message = new ItemCreatedIntegrationEvent("Item title", "Item description");

        _eventBus.Publish(message);

        return Ok();
    }
}
```

## Code of Conduct

See [CODE_OF_CONDUCT.md](https://github.com/sayganov/EventBus.RabbitMQ.Standard/blob/master/CODE_OF_CONDUCT.md).

## Contributing

See [CONTRIBUTING.md](https://github.com/sayganov/EventBus.RabbitMQ.Standard/blob/master/CONTRIBUTING.md).

## References

- [Publisher-Subscriber pattern](https://docs.microsoft.com/en-us/azure/architecture/patterns/publisher-subscriber)
- [RabbitMQ](https://www.rabbitmq.com/)
- [CloudAMQP](https://www.cloudamqp.com/)
- [eShopOnContainers](https://github.com/dotnet-architecture/eShopOnContainers)
- [.NET Core + RabbitMQ = 🎉](https://medium.com/@georgysay/net-core-rabbitmq-part-1-8da5e718d9dc)
