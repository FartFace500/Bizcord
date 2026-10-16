using EasyNetQ;
using MessageClient.Configuration;

namespace MessageClient;

/// <summary>EasyNetQ (RabbitMQ) implementation of <see cref="IMessageClient"/>.</summary>
public sealed class RabbitMqMessageClient : IMessageClient
{
    private readonly IBus _bus;
    private readonly string _subscriptionPrefix;

    public RabbitMqMessageClient(IBus bus, MessageHandlerOptions options)
    {
        _bus = bus;
        _subscriptionPrefix = options.SubscriptionPrefix;
    }

    public Task PublishAsync<T>(T message, CancellationToken cancellationToken = default) where T : class
        => _bus.PubSub.PublishAsync(message, cancellationToken);

    public Task SubscribeAsync<T>(Func<T, CancellationToken, Task> handler, CancellationToken cancellationToken = default) where T : class
        => _bus.PubSub.SubscribeAsync(SubscriptionIds.For<T>(_subscriptionPrefix), handler, _ => { }, cancellationToken);
}
