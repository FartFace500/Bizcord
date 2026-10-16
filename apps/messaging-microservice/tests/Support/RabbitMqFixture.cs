using Testcontainers.RabbitMq;

namespace Bizcord.Messaging.Tests.Support;

/// <summary>
/// Provides a real RabbitMQ for integration tests. Starts a throwaway container (needs Docker),
/// or uses an existing broker when the RABBITMQ_HOST environment variable is set (for example in CI).
/// </summary>
public sealed class RabbitMqFixture : IAsyncLifetime
{
    private RabbitMqContainer? _container;

    public string Host { get; private set; } = "localhost";
    public int Port { get; private set; } = 5672;

    private string _credentials = string.Empty;

    public string ConnectionString => $"host={Host};port={Port}{_credentials}";

    public async Task InitializeAsync()
    {
        var existing = Environment.GetEnvironmentVariable("RABBITMQ_HOST");
        if (!string.IsNullOrWhiteSpace(existing))
        {
            Host = existing;
            return;
        }

        _container = new RabbitMqBuilder()
            .WithImage("rabbitmq:3-management")
            .WithUsername("bizcord")
            .WithPassword("bizcord")
            .Build();
        _credentials = ";username=bizcord;password=bizcord";
        await _container.StartAsync();
        Host = _container.Hostname;
        Port = _container.GetMappedPublicPort(5672);
    }

    public async Task DisposeAsync()
    {
        if (_container is not null)
            await _container.DisposeAsync();
    }
}
