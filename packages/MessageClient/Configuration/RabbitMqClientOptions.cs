namespace MessageClient.Configuration;

public class RabbitMqClientOptions
{
    /// <summary>EasyNetQ connection string, for example <c>host=localhost</c>.</summary>
    public string ConnectionString { get; set; } = "host=localhost";

    /// <summary>
    /// When true, <c>PublishAsync</c> completes only after RabbitMQ has confirmed it received the
    /// message, and throws if it did not. Without confirms a message can be lost silently.
    /// </summary>
    public bool PublisherConfirms { get; set; } = true;

    /// <summary>How long to wait for the broker (connecting, confirms) before failing.</summary>
    public int TimeoutSeconds { get; set; } = 10;

    internal string BuildConnectionString()
    {
        var connectionString = ConnectionString.TrimEnd(';');
        if (!connectionString.Contains("publisherConfirms=", StringComparison.OrdinalIgnoreCase))
            connectionString += $";publisherConfirms={PublisherConfirms.ToString().ToLowerInvariant()}";
        if (!connectionString.Contains("timeout=", StringComparison.OrdinalIgnoreCase))
            connectionString += $";timeout={TimeoutSeconds}";
        return connectionString;
    }
}
