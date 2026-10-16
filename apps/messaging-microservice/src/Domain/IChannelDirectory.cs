namespace Bizcord.Messaging.Domain;

/// <summary>Asks the Channel Management service whether a channel exists.</summary>
public interface IChannelDirectory
{
    /// <exception cref="ChannelServiceUnavailableException">The Channel service could not give a clear answer.</exception>
    Task<bool> ExistsAsync(Guid channelId, CancellationToken cancellationToken);
}

public class ChannelServiceUnavailableException : Exception
{
    public ChannelServiceUnavailableException(string message, Exception? inner = null) : base(message, inner) { }
}
