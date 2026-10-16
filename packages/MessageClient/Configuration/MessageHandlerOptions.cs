namespace MessageClient.Configuration;

public class MessageHandlerOptions
{
    /// <summary>
    /// Identifies the subscribing service. It is part of every queue name, so each service
    /// gets its own queue per message type. Without a distinct prefix, two services would
    /// share one queue and each would only see some of the messages.
    /// </summary>
    public string SubscriptionPrefix { get; set; } = string.Empty;
}
