using Bizcord.Messaging.Domain;

namespace Bizcord.Messaging.Controllers;

public record CreateMessageRequest(Guid ChannelId, Guid AuthorId, string? Content);

public record MessageDto(Guid MessageId, Guid ChannelId, Guid AuthorId, string Content, DateTime PostedAt)
{
    public static MessageDto From(Message message)
        => new(message.Id, message.ChannelId, message.AuthorId, message.Content, message.PostedAt);
}
