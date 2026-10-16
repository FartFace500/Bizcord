using MessageClient;

namespace Bizcord.Messaging.Tests.Support;

/// <summary>
/// Subscribes to messages of type <typeparamref name="T"/> and lets a test wait for the first one.
/// The constructor returns only once the subscription is active, so a message published afterwards cannot be missed.
/// </summary>
public sealed class MessageCapture<T> where T : class
{
    private readonly TaskCompletionSource<T> _first = new(TaskCreationOptions.RunContinuationsAsynchronously);

    public MessageCapture(IMessageClient client)
    {
        client.SubscribeAsync<T>((message, _) =>
        {
            _first.TrySetResult(message);
            return Task.CompletedTask;
        }).GetAwaiter().GetResult();
    }

    public async Task<T> WaitForMessageAsync(TimeSpan timeout)
    {
        var finished = await Task.WhenAny(_first.Task, Task.Delay(timeout));
        if (finished != _first.Task)
            throw new TimeoutException($"No {typeof(T).Name} arrived within {timeout.TotalSeconds:0.#} seconds.");
        return await _first.Task;
    }
}
