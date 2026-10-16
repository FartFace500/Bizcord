using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace MessageClient;

/// <summary>
/// Subscribes to <typeparamref name="T"/> at startup and routes each incoming message to a
/// fresh <see cref="IMessageHandler{T}"/> from a new DI scope, so handlers may use scoped services.
/// </summary>
internal sealed class MessageHandlerSubscriber<T> : IHostedService where T : class
{
    private readonly IMessageClient _client;
    private readonly IServiceScopeFactory _scopeFactory;

    public MessageHandlerSubscriber(IMessageClient client, IServiceScopeFactory scopeFactory)
    {
        _client = client;
        _scopeFactory = scopeFactory;
    }

    public Task StartAsync(CancellationToken cancellationToken)
        => _client.SubscribeAsync<T>(Dispatch, cancellationToken);

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    private async Task Dispatch(T message, CancellationToken cancellationToken)
    {
        using var scope = _scopeFactory.CreateScope();
        var handler = scope.ServiceProvider.GetRequiredService<IMessageHandler<T>>();
        await handler.Handle(message, cancellationToken);
    }
}
