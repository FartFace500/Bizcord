namespace MessageClient;

/// <summary>Handles messages of type <typeparamref name="T"/>. Register with <c>AddMessageHandler</c>.</summary>
public interface IMessageHandler<in T> where T : class
{
    Task Handle(T message, CancellationToken cancellationToken);
}
