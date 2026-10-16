namespace Shared.Contracts.Events;

/// <summary>
/// Published by the Messaging Service when a message has been saved. Consumers (Notifications,
/// Presence, Search, Moderation, Analytics, Media, Realtime Gateway) react to it.
/// The Messaging Service promises that every field below is filled in (see the provider contract test).
/// </summary>
public class MessagePostedEvent
{
    public Guid MessageId { get; set; }
    public Guid ChannelId { get; set; }
    public Guid AuthorId { get; set; }
    public required string Content { get; set; }
    public DateTime PostedAt { get; set; }
}
