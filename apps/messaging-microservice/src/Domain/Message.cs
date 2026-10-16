namespace Bizcord.Messaging.Domain;

public class Message
{
    public const int MaxContentLength = 4000;

    public Guid Id { get; }
    public Guid ChannelId { get; }
    public Guid AuthorId { get; }
    public string Content { get; }
    public DateTime PostedAt { get; }

    private Message(Guid channelId, Guid authorId, string content)
    {
        Id = Guid.NewGuid();
        ChannelId = channelId;
        AuthorId = authorId;
        Content = content;
        PostedAt = DateTime.UtcNow;
    }

    public static Message Create(Guid channelId, Guid authorId, string? content)
    {
        if (channelId == Guid.Empty)
            throw new DomainException("A message must belong to a channel.");
        if (authorId == Guid.Empty)
            throw new DomainException("A message must have an author.");
        if (string.IsNullOrWhiteSpace(content))
            throw new DomainException("A message cannot be empty.");
        if (content.Length > MaxContentLength)
            throw new DomainException($"A message cannot be longer than {MaxContentLength} characters.");

        return new Message(channelId, authorId, content);
    }
}
