using System.Net;
using System.Net.Http.Json;
using Bizcord.Messaging.Controllers;
using Bizcord.Messaging.Tests.Support;
using Shared.Contracts.Events;

namespace Bizcord.Messaging.Tests.Api;

/// <summary>
/// Tests against the whole service through HTTP, with the broker replaced by <see cref="FakeMessageClient"/>.
/// </summary>
public class PostMessageTests : IClassFixture<MessagingApiFactory>
{
    private readonly MessagingApiFactory _factory;
    private readonly HttpClient _client;

    public PostMessageTests(MessagingApiFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    // Provider contract test: verifies the VALUES of what we publish, not just that the type compiles.
    [Fact]
    public async Task PostMessage_PublishedEvent_MeetsContract()
    {
        var channelId = Guid.NewGuid();
        var authorId = Guid.NewGuid();
        var capture = new MessageCapture<MessagePostedEvent>(_factory.MessageClient);

        var response = await _client.PostAsJsonAsync("/api/messages", new
        {
            ChannelId = channelId,
            AuthorId = authorId,
            Content = "Hello world"
        });

        response.StatusCode.Should().Be(HttpStatusCode.Created);

        var evt = await capture.WaitForMessageAsync(TimeSpan.FromSeconds(5));

        // Every field another service reads must be asserted here
        evt.MessageId.Should().NotBe(Guid.Empty);
        evt.ChannelId.Should().Be(channelId);
        evt.AuthorId.Should().Be(authorId);
        evt.Content.Should().Be("Hello world");
        evt.PostedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public async Task PostMessage_PublishedEvent_CarriesTheSameMessageIdAsTheResponse()
    {
        var capture = new MessageCapture<MessagePostedEvent>(_factory.MessageClient);

        var response = await _client.PostAsJsonAsync("/api/messages", new { ChannelId = Guid.NewGuid(), AuthorId = Guid.NewGuid(), Content = "trace me" });
        var body = await response.Content.ReadFromJsonAsync<MessageDto>();
        var evt = await capture.WaitForMessageAsync(TimeSpan.FromSeconds(5));

        evt.MessageId.Should().Be(body!.MessageId);
    }

    [Fact]
    public async Task PostMessage_Returns201_AndPersistsMessage()
    {
        var response = await _client.PostAsJsonAsync("/api/messages", new
        {
            ChannelId = Guid.NewGuid(),
            AuthorId = Guid.NewGuid(),
            Content = "Hello world"
        });

        response.StatusCode.Should().Be(HttpStatusCode.Created);

        var body = await response.Content.ReadFromJsonAsync<MessageDto>();
        body.Should().NotBeNull();
        body!.MessageId.Should().NotBe(Guid.Empty);

        var get = await _client.GetAsync($"/api/messages/{body.MessageId}");
        get.StatusCode.Should().Be(HttpStatusCode.OK);
        response.Headers.Location!.ToString().Should().EndWith($"/api/messages/{body.MessageId}");
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public async Task PostMessage_Returns400_AndPublishesNothing_WhenContentIsBlank(string content)
    {
        var before = _factory.MessageClient.Published.Count;

        var response = await _client.PostAsJsonAsync("/api/messages", new { ChannelId = Guid.NewGuid(), AuthorId = Guid.NewGuid(), Content = content });

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        _factory.MessageClient.Published.Count.Should().Be(before);
    }

    [Fact]
    public async Task PostMessage_Returns400_WhenContentIsMissing()
    {
        var response = await _client.PostAsJsonAsync("/api/messages", new { ChannelId = Guid.NewGuid(), AuthorId = Guid.NewGuid() });

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task PostMessage_Returns400_WhenIdsAreMissing()
    {
        var response = await _client.PostAsJsonAsync("/api/messages", new { Content = "no ids" });

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task GetMessage_Returns404_ForUnknownId()
    {
        var response = await _client.GetAsync($"/api/messages/{Guid.NewGuid()}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}
