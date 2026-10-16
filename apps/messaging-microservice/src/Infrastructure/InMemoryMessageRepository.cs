using System.Collections.Concurrent;
using Bizcord.Messaging.Domain;

namespace Bizcord.Messaging.Infrastructure;

public class InMemoryMessageRepository : IMessageRepository
{
    private readonly ConcurrentDictionary<Guid, Message> _messages = new();

    public Task Save(Message message)
    {
        _messages[message.Id] = message;
        return Task.CompletedTask;
    }

    public Task<Message?> GetById(Guid id)
        => Task.FromResult(_messages.TryGetValue(id, out var message) ? message : null);
}
