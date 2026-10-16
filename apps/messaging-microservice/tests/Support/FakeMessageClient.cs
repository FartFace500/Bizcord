using MessageClient;

namespace Bizcord.Messaging.Tests.Support;

/// <summary>
/// In-memory <see cref="IMessageClient"/> for tests. Records everything that is published and
/// delivers published messages to handlers subscribed in the same process.
/// </summary>
public class FakeMessageClient : IMessageClient
{
    private readonly object _lock = new();
    private readonly List<object> _published = new();
    private readonly List<(Type Type, Delegate Handler)> _subscriptions = new();

    public IReadOnlyList<object> Published
    {
        get { lock (_lock) return _published.ToList(); }
    }

    public T? SinglePublished<T>() where T : class => Published.OfType<T>().Single();

    public async Task PublishAsync<T>(T message, CancellationToken cancellationToken = default) where T : class
    {
        List<Func<T, CancellationToken, Task>> handlers;
        lock (_lock)
        {
            _published.Add(message);
            handlers = _subscriptions.Where(s => s.Type == typeof(T)).Select(s => (Func<T, CancellationToken, Task>)s.Handler).ToList();
        }

        foreach (var handler in handlers)
            await handler(message, cancellationToken);
    }

    public Task SubscribeAsync<T>(Func<T, CancellationToken, Task> handler, CancellationToken cancellationToken = default) where T : class
    {
        lock (_lock) _subscriptions.Add((typeof(T), handler));
        return Task.CompletedTask;
    }
}
