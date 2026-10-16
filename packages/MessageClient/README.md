# MessageClient

A small messaging abstraction for Bizcord services. Application code depends on `IMessageClient`. Only `RabbitMqMessageClient` knows about RabbitMQ (through EasyNetQ 7.8), so another broker can be supported by writing one more implementation of the interface.

## Interface

```csharp
public interface IMessageClient
{
    Task PublishAsync<T>(T message, CancellationToken ct = default) where T : class;
    Task SubscribeAsync<T>(Func<T, CancellationToken, Task> handler, CancellationToken ct = default) where T : class;
}

public interface IMessageHandler<in T> where T : class
{
    Task Handle(T message, CancellationToken ct);
}
```

## Use it in a service

```csharp
using MessageClient.Configuration;
using MessageClient.Extension;

var host = builder.Configuration["RABBITMQ_HOST"] ?? "localhost";

builder.Services.AddRabbitMqMessageClient(
    new RabbitMqClientOptions { ConnectionString = $"host={host}" },
    new MessageHandlerOptions { SubscriptionPrefix = "messaging" });   // the service's name

// Consumers only: subscribe a handler when the app starts
builder.Services.AddMessageHandler<MessagePostedEvent, MessagePostedHandler>();
```

Then inject `IMessageClient` wherever you need to publish.

## Things to know

- **`SubscriptionPrefix` must be unique per service.** It forms the queue name (`<prefix>-<MessageType>`). Two services with the same prefix would share one queue and each receive only some of the messages. Several instances of the same service share a queue on purpose, so the work is split between them.
- The RabbitMQ connection is created on first use, not at startup. The app can start and be tested without a broker.
- Handlers are resolved from a new DI scope for every message, so they can use scoped services.
- EasyNetQ serializes messages as JSON, so message types need public properties with setters (or init).

## Tests

```bash
dotnet test packages/MessageClient.Tests
```
