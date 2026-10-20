using Bizcord.Messaging.Controllers;
using Bizcord.Messaging.Domain;
using MessageClient;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Shared.Contracts.Events;

namespace Bizcord.Messaging.Tests.Controllers;

/// <summary>
/// Unit tests of the controller on its own. The repository, message client and channel directory
/// are Moq mocks, so these tests also verify the interactions (what was called, and in which order).
/// </summary>
public class MessagesControllerTests
{
    private readonly Mock<IMessageRepository> _repository = new();
    private readonly Mock<IMessageClient> _messageClient = new();
    private readonly Mock<IChannelDirectory> _channels = new();
    private readonly List<string> _calls = new();
    private readonly MessagesController _controller;

    public MessagesControllerTests()
    {
        _channels.Setup(c => c.ExistsAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(true);
        _repository.Setup(r => r.Save(It.IsAny<Message>()))
            .Callback(() => _calls.Add("save"))
            .Returns(Task.CompletedTask);
        _messageClient.Setup(m => m.PublishAsync(It.IsAny<MessagePostedEvent>(), It.IsAny<CancellationToken>()))
            .Callback(() => _calls.Add("publish"))
            .Returns(Task.CompletedTask);

        _controller = new MessagesController(_repository.Object, _messageClient.Object, _channels.Object, NullLogger<MessagesController>.Instance);
    }

    private static CreateMessageRequest ValidRequest() => new(Guid.NewGuid(), Guid.NewGuid(), "Hello world");

    [Fact]
    public async Task PostMessage_SavesThenPublishes_AndReturnsCreated()
    {
        var request = ValidRequest();

        var result = await _controller.PostMessage(request, CancellationToken.None);

        var created = result.Should().BeOfType<CreatedAtActionResult>().Subject;
        created.Value.Should().BeOfType<MessageDto>().Which.Content.Should().Be("Hello world");
        _calls.Should().Equal("save", "publish");
    }

    [Fact]
    public async Task PostMessage_PublishesEventMatchingTheSavedMessage()
    {
        Message? saved = null;
        MessagePostedEvent? published = null;
        _repository.Setup(r => r.Save(It.IsAny<Message>())).Callback<Message>(m => saved = m).Returns(Task.CompletedTask);
        _messageClient.Setup(m => m.PublishAsync(It.IsAny<MessagePostedEvent>(), It.IsAny<CancellationToken>()))
            .Callback<MessagePostedEvent, CancellationToken>((e, _) => published = e).Returns(Task.CompletedTask);

        await _controller.PostMessage(ValidRequest(), CancellationToken.None);

        published.Should().NotBeNull();
        published!.MessageId.Should().Be(saved!.Id);
        published.ChannelId.Should().Be(saved.ChannelId);
        published.AuthorId.Should().Be(saved.AuthorId);
        published.Content.Should().Be(saved.Content);
        published.PostedAt.Should().Be(saved.PostedAt);
    }

    [Fact]
    public async Task PostMessage_Returns400_AndDoesNotSaveOrPublish_WhenContentIsBlank()
    {
        var result = await _controller.PostMessage(new CreateMessageRequest(Guid.NewGuid(), Guid.NewGuid(), " "), CancellationToken.None);

        result.Should().BeOfType<ObjectResult>().Which.StatusCode.Should().Be(400);
        _repository.Verify(r => r.Save(It.IsAny<Message>()), Times.Never);
        _messageClient.Verify(m => m.PublishAsync(It.IsAny<MessagePostedEvent>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task PostMessage_DoesNotAskTheChannelService_WhenTheMessageIsInvalid()
    {
        await _controller.PostMessage(new CreateMessageRequest(Guid.NewGuid(), Guid.NewGuid(), ""), CancellationToken.None);

        _channels.Verify(c => c.ExistsAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task PostMessage_Returns404_AndDoesNotSaveOrPublish_WhenChannelDoesNotExist()
    {
        _channels.Setup(c => c.ExistsAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(false);

        var result = await _controller.PostMessage(ValidRequest(), CancellationToken.None);

        result.Should().BeOfType<ObjectResult>().Which.StatusCode.Should().Be(404);
        _calls.Should().BeEmpty();
    }

    [Fact]
    public async Task PostMessage_Returns503_AndDoesNotSaveOrPublish_WhenChannelServiceIsUnavailable()
    {
        _channels.Setup(c => c.ExistsAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new ChannelServiceUnavailableException("down"));

        var result = await _controller.PostMessage(ValidRequest(), CancellationToken.None);

        result.Should().BeOfType<ObjectResult>().Which.StatusCode.Should().Be(503);
        _calls.Should().BeEmpty();
    }

    [Fact]
    public async Task PostMessage_Returns503_WhenTheBrokerIsUnavailable()
    {
        _messageClient.Setup(m => m.PublishAsync(It.IsAny<MessagePostedEvent>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new TimeoutException("broker down"));

        var result = await _controller.PostMessage(ValidRequest(), CancellationToken.None);

        var failure = result.Should().BeOfType<ObjectResult>().Subject;
        failure.StatusCode.Should().Be(503);
        failure.Value.Should().BeOfType<ProblemDetails>().Which.Detail.Should().Contain("was saved");
    }

    [Fact]
    public async Task GetMessage_ReturnsTheStoredMessage()
    {
        var message = Message.Create(Guid.NewGuid(), Guid.NewGuid(), "stored");
        _repository.Setup(r => r.GetById(message.Id)).ReturnsAsync(message);

        var result = await _controller.GetMessage(message.Id);

        result.Should().BeOfType<OkObjectResult>().Which.Value.Should().BeOfType<MessageDto>()
            .Which.MessageId.Should().Be(message.Id);
    }

    [Fact]
    public async Task GetMessage_Returns404_WhenUnknown()
    {
        _repository.Setup(r => r.GetById(It.IsAny<Guid>())).ReturnsAsync((Message?)null);

        var result = await _controller.GetMessage(Guid.NewGuid());

        result.Should().BeOfType<NotFoundResult>();
    }
}
