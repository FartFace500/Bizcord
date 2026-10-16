namespace MessageClient.Configuration;

public class RabbitMqClientOptions
{
    /// <summary>EasyNetQ connection string, for example <c>host=localhost</c>.</summary>
    public string ConnectionString { get; set; } = "host=localhost";
}
