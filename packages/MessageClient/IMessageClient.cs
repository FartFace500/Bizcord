namespace MessageClient;

/// <summary>
/// Broker-agnostic messaging abstraction. No broker types appear here, so an
/// implementation backed by another broker (for example Kafka) can replace the
/// RabbitMQ one without changing any caller.
/// </summary>
public interface IMessageClient
{
    /// <summary>Publishes <paramref name="message"/> to every service subscribed to type <typeparamref name="T"/>.</summary>
    Task PublishAsync<T>(T message, CancellationToken cancellationToken = default) where T : class;

    /// <summary>
    /// Subscribes this service to messages of type <typeparamref name="T"/>. Instances of the
    /// same service share the work, different services each receive every message.
    /// </summary>
    Task SubscribeAsync<T>(Func<T, CancellationToken, Task> handler, CancellationToken cancellationToken = default) where T : class;
}
