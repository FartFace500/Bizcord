namespace Bizcord.Messaging.Domain;

public interface IMessageRepository
{
    Task Save(Message message);
    Task<Message?> GetById(Guid id);
}
