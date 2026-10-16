using EasyNetQ;
using EasyNetQ.Internals;
using MessageClient.Configuration;
using Moq;

namespace MessageClient.Tests;

public class RabbitMqMessageClientTests
{
    private readonly Mock<IBus> _bus = new();
    private readonly Mock<IPubSub> _pubSub = new();
    private readonly RabbitMqMessageClient _client;

    public RabbitMqMessageClientTests()
    {
        _bus.SetupGet(b => b.PubSub).Returns(_pubSub.Object);
        _pubSub.Setup(p => p.SubscribeAsync(It.IsAny<string>(), It.IsAny<Func<PingMessage, CancellationToken, Task>>(), It.IsAny<Action<ISubscriptionConfiguration>>(), It.IsAny<CancellationToken>()))
            .Returns(new AwaitableDisposable<SubscriptionResult>(Task.FromResult(default(SubscriptionResult))));
        _client = new RabbitMqMessageClient(_bus.Object, new MessageHandlerOptions { SubscriptionPrefix = "messaging" });
    }

    [Fact]
    public async Task PublishAsync_ForwardsTheMessageToTheBus()
    {
        var message = new PingMessage { Text = "hi" };

        await _client.PublishAsync(message);

        _pubSub.Verify(p => p.PublishAsync(message, It.IsAny<Action<IPublishConfiguration>>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task SubscribeAsync_UsesPrefixedSubscriptionId()
    {
        Func<PingMessage, CancellationToken, Task> handler = (_, _) => Task.CompletedTask;

        await _client.SubscribeAsync(handler);

        _pubSub.Verify(p => p.SubscribeAsync(
            "messaging-PingMessage",
            handler,
            It.IsAny<Action<ISubscriptionConfiguration>>(),
            It.IsAny<CancellationToken>()), Times.Once);
    }
}
