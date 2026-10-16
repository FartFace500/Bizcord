using System.Net;
using System.Net.Http.Json;
using Bizcord.Messaging.Tests.Support;
using EasyNetQ;
using MessageClient;
using MessageClient.Configuration;
using MessageClient.Extension;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Shared.Contracts.Events;

namespace Bizcord.Messaging.Tests.Integration;

/// <summary>
/// Integration tests with a REAL RabbitMQ and the real EasyNetQ message client.
/// Needs Docker (or a broker pointed to by RABBITMQ_HOST). Run only these with: dotnet test --filter Category=Broker
/// </summary>
[Trait("Category", "Broker")]
public sealed class RealBrokerTests : IClassFixture<RabbitMqFixture>, IDisposable
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;

    public RealBrokerTests(RabbitMqFixture rabbit)
    {
        _factory = new WebApplicationFactory<Program>().WithWebHostBuilder(builder =>
            builder.ConfigureTestServices(services =>
            {
                services.RemoveAll<IMessageClient>();
                services.RemoveAll<IBus>();
                services.AddRabbitMqMessageClient(
                    new RabbitMqClientOptions { ConnectionString = rabbit.ConnectionString },
                    new MessageHandlerOptions { SubscriptionPrefix = "messaging" });
            }));
        _client = _factory.CreateClient();
    }

    public void Dispose()
    {
        _client.Dispose();
        _factory.Dispose();
    }

    // A subscriber with its own queue, like another service (for example Search) would have.
    private IMessageClient NewSubscriber()
        => new RabbitMqMessageClient(
            _factory.Services.GetRequiredService<IBus>(),
            new MessageHandlerOptions { SubscriptionPrefix = $"test-subscriber-{Guid.NewGuid():N}" });

    [Fact]
    public async Task PostMessage_PublishesEventThatAnotherServiceReceivesFromTheBroker()
    {
        var channelId = Guid.NewGuid();
        var authorId = Guid.NewGuid();
        var capture = new MessageCapture<MessagePostedEvent>(NewSubscriber());

        var response = await _client.PostAsJsonAsync("/api/messages", new { ChannelId = channelId, AuthorId = authorId, Content = "Hello via RabbitMQ" });

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var evt = await capture.WaitForMessageAsync(TimeSpan.FromSeconds(10));
        evt.MessageId.Should().NotBe(Guid.Empty);
        evt.ChannelId.Should().Be(channelId);
        evt.AuthorId.Should().Be(authorId);
        evt.Content.Should().Be("Hello via RabbitMQ");
        evt.PostedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(10));
    }

    [Fact]
    public async Task EveryServiceWithItsOwnQueueReceivesTheEvent()
    {
        var search = new MessageCapture<MessagePostedEvent>(NewSubscriber());
        var notifications = new MessageCapture<MessagePostedEvent>(NewSubscriber());

        await _client.PostAsJsonAsync("/api/messages", new { ChannelId = Guid.NewGuid(), AuthorId = Guid.NewGuid(), Content = "fan out" });

        var forSearch = await search.WaitForMessageAsync(TimeSpan.FromSeconds(10));
        var forNotifications = await notifications.WaitForMessageAsync(TimeSpan.FromSeconds(10));
        forSearch.MessageId.Should().Be(forNotifications.MessageId);
    }

    [Fact]
    public async Task PostMessage_Returns201_AndPersistsMessage_WithTheRealBroker()
    {
        var response = await _client.PostAsJsonAsync("/api/messages", new { ChannelId = Guid.NewGuid(), AuthorId = Guid.NewGuid(), Content = "Hello world" });

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var body = await response.Content.ReadFromJsonAsync<Bizcord.Messaging.Controllers.MessageDto>();
        (await _client.GetAsync($"/api/messages/{body!.MessageId}")).StatusCode.Should().Be(HttpStatusCode.OK);
    }
}
