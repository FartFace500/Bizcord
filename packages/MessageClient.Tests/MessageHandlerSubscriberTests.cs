using MessageClient.Configuration;
using MessageClient.Extension;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace MessageClient.Tests;

public class MessageHandlerSubscriberTests
{
    private sealed class CapturingClient : IMessageClient
    {
        public Func<PingMessage, CancellationToken, Task>? Subscribed { get; private set; }

        public Task PublishAsync<T>(T message, CancellationToken cancellationToken = default) where T : class
            => Task.CompletedTask;

        public Task SubscribeAsync<T>(Func<T, CancellationToken, Task> handler, CancellationToken cancellationToken = default) where T : class
        {
            Subscribed = (Func<PingMessage, CancellationToken, Task>)(object)handler;
            return Task.CompletedTask;
        }
    }

    [Fact]
    public async Task OnStart_SubscribesAndRoutesMessagesToTheHandler()
    {
        RecordingHandler.Received.Clear();
        var client = new CapturingClient();
        var builder = Host.CreateApplicationBuilder();
        builder.Services.AddSingleton<IMessageClient>(client);
        builder.Services.AddMessageHandler<PingMessage, RecordingHandler>();

        using var host = builder.Build();
        await host.StartAsync();

        Assert.NotNull(client.Subscribed);
        await client.Subscribed!(new PingMessage { Text = "hello" }, CancellationToken.None);
        Assert.Equal(new[] { "hello" }, RecordingHandler.Received);

        await host.StopAsync();
    }

    [Fact]
    public void AddRabbitMqMessageClient_RegistersTheMessageClientWithoutConnecting()
    {
        var services = new ServiceCollection();

        services.AddRabbitMqMessageClient(new RabbitMqClientOptions { ConnectionString = "host=nowhere" }, new MessageHandlerOptions());

        var descriptor = Assert.Single(services, d => d.ServiceType == typeof(IMessageClient));
        Assert.Equal(typeof(RabbitMqMessageClient), descriptor.ImplementationType);
    }
}
