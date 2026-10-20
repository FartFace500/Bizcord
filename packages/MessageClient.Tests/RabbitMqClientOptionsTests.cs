using MessageClient.Configuration;

namespace MessageClient.Tests;

public class RabbitMqClientOptionsTests
{
    [Fact]
    public void BuildConnectionString_TurnsOnPublisherConfirmsAndATimeoutByDefault()
        => Assert.Equal("host=rabbitmq;publisherConfirms=true;timeout=10",
            new RabbitMqClientOptions { ConnectionString = "host=rabbitmq" }.BuildConnectionString());

    [Fact]
    public void BuildConnectionString_UsesTheConfiguredValues()
        => Assert.Equal("host=rabbitmq;publisherConfirms=false;timeout=3",
            new RabbitMqClientOptions { ConnectionString = "host=rabbitmq;", PublisherConfirms = false, TimeoutSeconds = 3 }.BuildConnectionString());

    [Fact]
    public void BuildConnectionString_KeepsSettingsAlreadyInTheConnectionString()
        => Assert.Equal("host=rabbitmq;publisherConfirms=false;timeout=30",
            new RabbitMqClientOptions { ConnectionString = "host=rabbitmq;publisherConfirms=false;timeout=30" }.BuildConnectionString());
}
