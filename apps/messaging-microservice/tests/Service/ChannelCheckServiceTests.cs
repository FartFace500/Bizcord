using System.Net;
using System.Net.Http.Json;
using Bizcord.Messaging.Tests.Support;
using Shared.Contracts.Events;
using WireMock.RequestBuilders;
using WireMock.ResponseBuilders;
using WireMock.Server;

namespace Bizcord.Messaging.Tests.Service;

/// <summary>
/// Service tests: the Messaging Service runs for real and the Channel service is replaced by a
/// WireMock HTTP stub that returns canned answers. A failure here can only be caused by Messaging.
/// </summary>
public sealed class ChannelCheckServiceTests : IDisposable
{
    private readonly WireMockServer _channelService = WireMockServer.Start();
    private readonly MessagingApiFactory _factory;
    private readonly HttpClient _client;

    public ChannelCheckServiceTests()
    {
        _factory = new MessagingApiFactory { ChannelServiceUrl = _channelService.Url };
        _client = _factory.CreateClient();
    }

    public void Dispose()
    {
        _client.Dispose();
        _factory.Dispose();
        _channelService.Stop();
    }

    private void ChannelServiceAnswers(Guid channelId, int status)
        => _channelService
            .Given(Request.Create().WithPath($"/api/channels/{channelId}").UsingGet())
            .RespondWith(Response.Create().WithStatusCode(status));

    private Task<HttpResponseMessage> Post(Guid channelId)
        => _client.PostAsJsonAsync("/api/messages", new { ChannelId = channelId, AuthorId = Guid.NewGuid(), Content = "Hello" });

    [Fact]
    public async Task PostMessage_Returns201_AndPublishes_WhenChannelExists()
    {
        var channelId = Guid.NewGuid();
        ChannelServiceAnswers(channelId, 200);

        var response = await Post(channelId);

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        _factory.MessageClient.SinglePublished<MessagePostedEvent>()!.ChannelId.Should().Be(channelId);
        _channelService.LogEntries.Should().ContainSingle()
            .Which.RequestMessage.Path.Should().Be($"/api/channels/{channelId}");
    }

    [Fact]
    public async Task PostMessage_Returns404_AndPublishesNothing_WhenChannelDoesNotExist()
    {
        var channelId = Guid.NewGuid();
        ChannelServiceAnswers(channelId, 404);

        var response = await Post(channelId);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        _factory.MessageClient.Published.Should().BeEmpty();
    }

    [Fact]
    public async Task PostMessage_Returns503_AndPublishesNothing_WhenChannelServiceFails()
    {
        var channelId = Guid.NewGuid();
        ChannelServiceAnswers(channelId, 500);

        var response = await Post(channelId);

        response.StatusCode.Should().Be(HttpStatusCode.ServiceUnavailable);
        _factory.MessageClient.Published.Should().BeEmpty();
    }

    [Fact]
    public async Task PostMessage_Returns503_WhenChannelServiceIsUnreachable()
    {
        _channelService.Stop();

        var response = await Post(Guid.NewGuid());

        response.StatusCode.Should().Be(HttpStatusCode.ServiceUnavailable);
        _factory.MessageClient.Published.Should().BeEmpty();
    }

    [Fact]
    public async Task PostMessage_DoesNotCallTheChannelService_WhenTheMessageIsInvalid()
    {
        var response = await _client.PostAsJsonAsync("/api/messages", new { ChannelId = Guid.NewGuid(), AuthorId = Guid.NewGuid(), Content = "" });

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        _channelService.LogEntries.Should().BeEmpty();
    }
}

public sealed class ChannelCheckDisabledTests : IClassFixture<MessagingApiFactory>
{
    private readonly MessagingApiFactory _factory;

    public ChannelCheckDisabledTests(MessagingApiFactory factory) => _factory = factory;

    [Fact]
    public async Task PostMessage_AcceptsAnyChannel_WhenNoChannelServiceIsConfigured()
    {
        var response = await _factory.CreateClient().PostAsJsonAsync("/api/messages",
            new { ChannelId = Guid.NewGuid(), AuthorId = Guid.NewGuid(), Content = "Hello" });

        response.StatusCode.Should().Be(HttpStatusCode.Created);
    }
}
