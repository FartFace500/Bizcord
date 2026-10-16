using Bizcord.Messaging.Domain;
using MessageClient;
using Microsoft.AspNetCore.Mvc;
using Shared.Contracts.Events;

namespace Bizcord.Messaging.Controllers;

[ApiController]
[Route("api/messages")]
public class MessagesController : ControllerBase
{
    private readonly IMessageRepository _repository;
    private readonly IMessageClient _messageClient;
    private readonly IChannelDirectory _channels;

    public MessagesController(IMessageRepository repository, IMessageClient messageClient, IChannelDirectory channels)
    {
        _repository = repository;
        _messageClient = messageClient;
        _channels = channels;
    }

    [HttpPost]
    [ProducesResponseType<MessageDto>(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
    public async Task<IActionResult> PostMessage(CreateMessageRequest request, CancellationToken cancellationToken)
    {
        Message message;
        try
        {
            message = Message.Create(request.ChannelId, request.AuthorId, request.Content);
        }
        catch (DomainException ex)
        {
            return Fail(StatusCodes.Status400BadRequest, "Invalid message", ex.Message);
        }

        try
        {
            if (!await _channels.ExistsAsync(message.ChannelId, cancellationToken))
                return Fail(StatusCodes.Status404NotFound, "Unknown channel", $"Channel {message.ChannelId} does not exist.");
        }
        catch (ChannelServiceUnavailableException ex)
        {
            return Fail(StatusCodes.Status503ServiceUnavailable, "Channel service unavailable", ex.Message);
        }

        await _repository.Save(message);

        await _messageClient.PublishAsync(new MessagePostedEvent
        {
            MessageId = message.Id,
            ChannelId = message.ChannelId,
            AuthorId = message.AuthorId,
            Content = message.Content,
            PostedAt = message.PostedAt
        }, cancellationToken);

        return CreatedAtAction(nameof(GetMessage), new { id = message.Id }, MessageDto.From(message));
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType<MessageDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetMessage(Guid id)
    {
        var message = await _repository.GetById(id);
        return message is null ? NotFound() : Ok(MessageDto.From(message));
    }

    private ObjectResult Fail(int status, string title, string detail)
        => StatusCode(status, new ProblemDetails { Status = status, Title = title, Detail = detail });
}
